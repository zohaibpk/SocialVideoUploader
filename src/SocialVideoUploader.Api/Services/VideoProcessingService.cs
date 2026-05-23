using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Options;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Services;

public sealed class VideoProcessingService(
    AssetStorageService assetStorageService,
    MediaInspectionService mediaInspectionService,
    VideoProcessingProfileRegistry profileRegistry,
    IOptions<MediaToolsConfiguration> options)
{
    private readonly MediaToolsConfiguration _configuration = options.Value;

    public IReadOnlyList<VideoProcessingProfile> GetProfiles()
    {
        return profileRegistry.GetProfiles();
    }

    public async Task<GenerateVariantsResponse> GenerateVariantsAsync(
        GenerateVariantsRequest request,
        CancellationToken cancellationToken)
    {
        var draft = request.Draft;
        var profiles = profileRegistry.ResolveProfiles(draft).ToArray();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(draft.Asset.VideoAssetId))
        {
            throw new InvalidOperationException("Upload a source video before generating variants.");
        }

        if (!assetStorageService.TryGetVideoAsset(draft.Asset.VideoAssetId, out var sourceAsset))
        {
            throw new InvalidOperationException("The source video asset could not be found in storage.");
        }

        var availability = await CheckFfmpegAvailabilityAsync(cancellationToken);

        if (!availability.Available)
        {
            warnings.Add(availability.Warning);

            return new GenerateVariantsResponse
            {
                FfmpegAvailable = false,
                Warnings = warnings.ToArray(),
                Profiles = profiles,
                Variants = []
            };
        }

        var variants = new List<GeneratedVariantResult>();

        foreach (var profile in profiles)
        {
            variants.Add(await GenerateVariantAsync(sourceAsset, draft, profile, cancellationToken));
        }

        return new GenerateVariantsResponse
        {
            FfmpegAvailable = true,
            Warnings = warnings.ToArray(),
            Profiles = profiles,
            Variants = variants.ToArray()
        };
    }

    private async Task<GeneratedVariantResult> GenerateVariantAsync(
        StoredAsset sourceAsset,
        UploadDraftRequest draft,
        VideoProcessingProfile profile,
        CancellationToken cancellationToken)
    {
        var target = assetStorageService.CreateVariantAsset(sourceAsset.AssetId, profile.Id);

        if (File.Exists(target.StoredPath))
        {
            File.Delete(target.StoredPath);
        }

        var trimmedDuration = Math.Max(draft.Editing.TrimEndSeconds - draft.Editing.TrimStartSeconds, 0);
        var effectiveDuration = profile.MaxDurationSeconds.HasValue
            ? Math.Min(trimmedDuration, profile.MaxDurationSeconds.Value)
            : trimmedDuration;

        var warnings = new List<string>();
        var filterSegments = new List<string>
        {
            $"scale={profile.Width}:{profile.Height}:force_original_aspect_ratio=decrease",
            $"pad={profile.Width}:{profile.Height}:(ow-iw)/2:(oh-ih)/2:color=black",
            $"fps={profile.TargetFrameRate}",
            "setsar=1"
        };

        var drawTextFilter = BuildDrawTextFilter(draft, profile, warnings);
        if (!string.IsNullOrWhiteSpace(drawTextFilter))
        {
            filterSegments.Add(drawTextFilter);
        }

        var args = BuildArguments(
            sourceAsset.StoredPath,
            target.StoredPath,
            draft,
            effectiveDuration,
            profile,
            filterSegments);

        var startInfo = new ProcessStartInfo
        {
            FileName = _configuration.FfmpegPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(startInfo);

            if (process is null)
            {
                return Failed(profile, "FFmpeg did not start.", args, warnings);
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_configuration.ProcessTimeoutSeconds * 6));

            var stdOutTask = process.StandardOutput.ReadToEndAsync();
            var stdErrTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(timeoutCts.Token);
            await stdOutTask;
            var stdErr = await stdErrTask;

            if (process.ExitCode != 0)
            {
                return Failed(profile, stdErr.Trim(), args, warnings);
            }

            var inspection = await mediaInspectionService.InspectAsync(target, cancellationToken);
            var fileInfo = new FileInfo(target.StoredPath);

            return new GeneratedVariantResult
            {
                ProfileId = profile.Id,
                DisplayName = profile.DisplayName,
                Platforms = profile.Platforms,
                Succeeded = true,
                OutputUrl = target.PublicUrl,
                SizeBytes = fileInfo.Length,
                SizeMb = fileInfo.Length / 1024d / 1024d,
                CommandPreview = $"{_configuration.FfmpegPath} {args}",
                Inspection = inspection,
                Warnings = warnings.Concat(inspection.Warnings).Distinct().ToArray(),
                Error = string.Empty
            };
        }
        catch (OperationCanceledException)
        {
            return Failed(profile, "FFmpeg timed out while generating this variant.", args, warnings);
        }
        catch (Win32Exception)
        {
            return Failed(profile, "FFmpeg is not installed or is not available on the configured path.", args, warnings);
        }
    }

    private string BuildArguments(
        string inputPath,
        string outputPath,
        UploadDraftRequest draft,
        double effectiveDuration,
        VideoProcessingProfile profile,
        IReadOnlyList<string> filters)
    {
        var trimStart = draft.Editing.TrimStartSeconds.ToString("0.###", CultureInfo.InvariantCulture);
        var duration = effectiveDuration.ToString("0.###", CultureInfo.InvariantCulture);
        var vf = string.Join(",", filters);
        var args =
            $"-y -i \"{inputPath}\" -ss {trimStart} -t {duration} -vf \"{vf}\" -c:v libx264 -pix_fmt yuv420p -preset fast -movflags +faststart";

        if (profile.Id == "telegram-compact")
        {
            var bitrate = CalculateTelegramBitrateKbps(effectiveDuration);
            args += $" -b:v {bitrate}k -maxrate {bitrate}k -bufsize {bitrate * 2}k -c:a aac -b:a 96k -ar 44100 -ac 2";
        }
        else
        {
            args += " -crf 23 -c:a aac -b:a 128k -ar 48000 -ac 2";
        }

        args += $" \"{outputPath}\"";
        return args;
    }

    private string BuildDrawTextFilter(
        UploadDraftRequest draft,
        VideoProcessingProfile profile,
        ICollection<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(draft.Editing.WatermarkText))
        {
            return string.Empty;
        }

        var fontFile = _configuration.DefaultFontFile;
        if (!File.Exists(fontFile))
        {
            warnings.Add("Configured drawtext font file was not found, so the watermark was skipped.");
            return string.Empty;
        }

        var safePadding = profile.AspectRatio == "9:16"
            ? (profile.Id == "story-vertical" ? 140 : 180)
            : 48;
        var text = EscapeDrawText(draft.Editing.WatermarkText);
        var alpha = Math.Clamp(draft.Editing.WatermarkOpacityPercent / 100d, 0.1d, 1d)
            .ToString("0.##", CultureInfo.InvariantCulture);
        var fontPath = EscapeFilterPath(fontFile);
        var (x, y) = draft.Editing.WatermarkPosition switch
        {
            WatermarkPosition.TopLeft => ($"{safePadding}", $"{safePadding}"),
            WatermarkPosition.TopRight => ($"main_w-text_w-{safePadding}", $"{safePadding}"),
            WatermarkPosition.Center => ("(main_w-text_w)/2", "(main_h-text_h)/2"),
            WatermarkPosition.BottomLeft => ($"{safePadding}", $"main_h-text_h-{safePadding}"),
            _ => ($"main_w-text_w-{safePadding}", $"main_h-text_h-{safePadding}")
        };

        return
            $"drawtext=text='{text}':fontfile='{fontPath}':fontsize=h/28:fontcolor=white@{alpha}:shadowx=2:shadowy=2:shadowcolor=black@0.55:box=1:boxcolor=black@0.25:boxborderw=10:x={x}:y={y}:fix_bounds=1";
    }

    private static string EscapeDrawText(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace(":", "\\:", StringComparison.Ordinal)
            .Replace("'", "\\'", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace(",", "\\,", StringComparison.Ordinal);
    }

    private static string EscapeFilterPath(string value)
    {
        return value
            .Replace("\\", "/", StringComparison.Ordinal)
            .Replace(":", "\\:", StringComparison.Ordinal)
            .Replace("'", "\\'", StringComparison.Ordinal);
    }

    private static int CalculateTelegramBitrateKbps(double durationSeconds)
    {
        var safeDuration = Math.Max(durationSeconds, 1);
        var totalKilobits = 45 * 8192d;
        var audioKilobitsPerSecond = 96d;
        var target = (int)Math.Max(550, Math.Floor((totalKilobits / safeDuration) - audioKilobitsPerSecond));
        return Math.Min(target, 2000);
    }

    private async Task<(bool Available, string Warning)> CheckFfmpegAvailabilityAsync(CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _configuration.FfmpegPath,
            Arguments = "-version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(startInfo);

            if (process is null)
            {
                return (false, "FFmpeg did not start from the configured path.");
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_configuration.ProcessTimeoutSeconds));

            await process.WaitForExitAsync(timeoutCts.Token);
            return (process.ExitCode == 0, "FFmpeg returned a non-zero exit code when checking availability.");
        }
        catch (Exception)
        {
            return (false, "Install FFmpeg or configure MediaTools:FfmpegPath before generating variants.");
        }
    }

    private static GeneratedVariantResult Failed(
        VideoProcessingProfile profile,
        string error,
        string args,
        IEnumerable<string> warnings)
    {
        return new GeneratedVariantResult
        {
            ProfileId = profile.Id,
            DisplayName = profile.DisplayName,
            Platforms = profile.Platforms,
            Succeeded = false,
            OutputUrl = string.Empty,
            SizeBytes = 0,
            SizeMb = 0,
            CommandPreview = args,
            Inspection = new MediaInspectionResult
            {
                Status = MediaInspectionStatus.Failed,
                Source = "ffmpeg",
                Warnings = []
            },
            Warnings = warnings.ToArray(),
            Error = string.IsNullOrWhiteSpace(error) ? "FFmpeg processing failed." : error
        };
    }
}
