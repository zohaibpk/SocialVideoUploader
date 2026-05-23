using Microsoft.Extensions.Options;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;
using SocialVideoUploader.Api.Infrastructure;

namespace SocialVideoUploader.Api.Services;

public sealed class AssetStorageService(
    IHostEnvironment environment,
    IOptions<StorageConfiguration> options)
{
    private readonly StorageConfiguration _configuration = options.Value;
    private readonly string _rootPath = StoragePathResolver.ResolveRootPath(environment, options.Value);

    public async Task<StoredAsset> SaveVideoAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("The uploaded video file is empty.");
        }

        if (file.Length > _configuration.MaxVideoBytes)
        {
            throw new InvalidOperationException($"Video files must be {BytesToMegabytes(_configuration.MaxVideoBytes):0} MB or smaller.");
        }

        return await SaveAsync(file, _configuration.VideosFolder, "video", cancellationToken);
    }

    public async Task<StoredAsset> SaveThumbnailAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("The uploaded thumbnail file is empty.");
        }

        if (file.Length > _configuration.MaxThumbnailBytes)
        {
            throw new InvalidOperationException($"Thumbnail files must be {BytesToMegabytes(_configuration.MaxThumbnailBytes):0} MB or smaller.");
        }

        return await SaveAsync(file, _configuration.ThumbnailsFolder, "thumbnail", cancellationToken);
    }

    public bool TryGetVideoAsset(string assetId, out StoredAsset asset)
    {
        return TryGetAsset(assetId, _configuration.VideosFolder, "video", out asset);
    }

    public StoredAsset CreateVariantAsset(string sourceAssetId, string profileId)
    {
        Directory.CreateDirectory(_rootPath);

        var targetFolder = Path.Combine(_rootPath, _configuration.VariantsFolder);
        Directory.CreateDirectory(targetFolder);

        var fileName = $"{sourceAssetId}-{profileId}.mp4";
        var storedPath = Path.Combine(targetFolder, fileName);

        return new StoredAsset
        {
            AssetId = $"{sourceAssetId}-{profileId}",
            AssetType = "variant",
            OriginalFileName = fileName,
            ContentType = "video/mp4",
            StoredPath = storedPath,
            PublicUrl = $"{_configuration.RequestPath}/{_configuration.VariantsFolder}/{fileName}"
        };
    }

    private async Task<StoredAsset> SaveAsync(
        IFormFile file,
        string folderName,
        string assetType,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_rootPath);

        var targetFolder = Path.Combine(_rootPath, folderName);
        Directory.CreateDirectory(targetFolder);

        var assetId = Guid.NewGuid().ToString("n");
        var extension = Path.GetExtension(file.FileName);
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".bin" : extension.ToLowerInvariant();
        var storedFileName = $"{assetId}{safeExtension}";
        var storedPath = Path.Combine(targetFolder, storedFileName);

        await using var stream = new FileStream(storedPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        var publicUrl = $"{_configuration.RequestPath}/{folderName}/{storedFileName}";

        return new StoredAsset
        {
            AssetId = assetId,
            AssetType = assetType,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            StoredPath = storedPath,
            PublicUrl = publicUrl
        };
    }

    private bool TryGetAsset(string assetId, string folderName, string assetType, out StoredAsset asset)
    {
        var targetFolder = Path.Combine(_rootPath, folderName);
        asset = new StoredAsset();

        if (!Directory.Exists(targetFolder))
        {
            return false;
        }

        var match = Directory
            .EnumerateFiles(targetFolder, $"{assetId}.*", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();

        if (match is null)
        {
            return false;
        }

        asset = new StoredAsset
        {
            AssetId = assetId,
            AssetType = assetType,
            OriginalFileName = match.Name,
            ContentType = GetContentType(match.Extension),
            SizeBytes = match.Length,
            StoredPath = match.FullName,
            PublicUrl = $"{_configuration.RequestPath}/{folderName}/{match.Name}"
        };

        return true;
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".mkv" => "video/x-matroska",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
    }

    private static double BytesToMegabytes(long value)
    {
        return value / 1024d / 1024d;
    }
}

public sealed class StoredAsset
{
    public string AssetId { get; set; } = string.Empty;

    public string AssetType { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string StoredPath { get; set; } = string.Empty;

    public string PublicUrl { get; set; } = string.Empty;
}
