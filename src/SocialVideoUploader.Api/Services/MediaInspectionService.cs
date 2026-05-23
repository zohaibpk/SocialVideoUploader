using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Services;

public sealed class MediaInspectionService(IOptions<MediaToolsConfiguration> options)
{
    private readonly MediaToolsConfiguration _configuration = options.Value;

    public async Task<MediaInspectionResult> InspectAsync(
        StoredAsset asset,
        CancellationToken cancellationToken)
    {
        if (!asset.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            return new MediaInspectionResult
            {
                Status = MediaInspectionStatus.Partial,
                Source = "content-type",
                Container = Path.GetExtension(asset.OriginalFileName).TrimStart('.').ToLowerInvariant(),
                Warnings = ["Non-video assets are not probed for media stream metadata."]
            };
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _configuration.FfprobePath,
            Arguments = $"-v quiet -print_format json -show_format -show_streams \"{asset.StoredPath}\"",
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
                return BuildUnavailable("ffprobe did not start.");
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_configuration.ProcessTimeoutSeconds));

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(timeoutCts.Token);

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
            {
                return new MediaInspectionResult
                {
                    Status = MediaInspectionStatus.Failed,
                    Source = "ffprobe",
                    Container = Path.GetExtension(asset.OriginalFileName).TrimStart('.').ToLowerInvariant(),
                    Warnings =
                    [
                        "ffprobe could not inspect this file.",
                        string.IsNullOrWhiteSpace(error) ? "The media tool returned no additional details." : error.Trim()
                    ]
                };
            }

            return Parse(output);
        }
        catch (OperationCanceledException)
        {
            return new MediaInspectionResult
            {
                Status = MediaInspectionStatus.Failed,
                Source = "ffprobe",
                Container = Path.GetExtension(asset.OriginalFileName).TrimStart('.').ToLowerInvariant(),
                Warnings = ["ffprobe timed out before inspection could complete."]
            };
        }
        catch (Win32Exception)
        {
            return BuildUnavailable("ffprobe is not installed or is not on the PATH for the API process.");
        }
        catch (Exception exception)
        {
            return new MediaInspectionResult
            {
                Status = MediaInspectionStatus.Failed,
                Source = "ffprobe",
                Container = Path.GetExtension(asset.OriginalFileName).TrimStart('.').ToLowerInvariant(),
                Warnings = [$"Inspection failed: {exception.Message}"]
            };
        }
    }

    private static MediaInspectionResult Parse(string json)
    {
        var payload = JsonSerializer.Deserialize<FfprobePayload>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var videoStream = payload?.Streams?.FirstOrDefault(stream => stream.CodecType == "video");
        var audioStream = payload?.Streams?.FirstOrDefault(stream => stream.CodecType == "audio");

        return new MediaInspectionResult
        {
            Status = MediaInspectionStatus.Complete,
            Source = "ffprobe",
            DurationSeconds = ParseNullableDouble(payload?.Format?.Duration),
            Width = videoStream?.Width,
            Height = videoStream?.Height,
            FrameRate = ParseFrameRate(videoStream?.AvgFrameRate ?? videoStream?.RFrameRate),
            Container = payload?.Format?.FormatName ?? string.Empty,
            VideoCodec = videoStream?.CodecName ?? string.Empty,
            AudioCodec = audioStream?.CodecName ?? string.Empty,
            Warnings = []
        };
    }

    private static double? ParseNullableDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    private static double? ParseFrameRate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parts = value.Split('/');
        if (parts.Length == 2
            && double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var numerator)
            && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var denominator)
            && denominator != 0)
        {
            return numerator / denominator;
        }

        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    private static MediaInspectionResult BuildUnavailable(string warning)
    {
        return new MediaInspectionResult
        {
            Status = MediaInspectionStatus.Unavailable,
            Source = "fallback",
            Warnings =
            [
                warning,
                "Install FFmpeg/ffprobe or configure MediaTools:FfprobePath to enable automatic duration, resolution, and frame-rate inspection."
            ]
        };
    }

    private sealed class FfprobePayload
    {
        [JsonPropertyName("streams")]
        public FfprobeStream[] Streams { get; set; } = [];

        [JsonPropertyName("format")]
        public FfprobeFormat? Format { get; set; }
    }

    private sealed class FfprobeStream
    {
        [JsonPropertyName("codec_type")]
        public string CodecType { get; set; } = string.Empty;

        [JsonPropertyName("codec_name")]
        public string CodecName { get; set; } = string.Empty;

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("avg_frame_rate")]
        public string AvgFrameRate { get; set; } = string.Empty;

        [JsonPropertyName("r_frame_rate")]
        public string RFrameRate { get; set; } = string.Empty;
    }

    private sealed class FfprobeFormat
    {
        [JsonPropertyName("duration")]
        public string Duration { get; set; } = string.Empty;

        [JsonPropertyName("format_name")]
        public string FormatName { get; set; } = string.Empty;
    }
}
