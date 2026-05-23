namespace SocialVideoUploader.Api.Contracts;

public enum MediaInspectionStatus
{
    Complete,
    Partial,
    Unavailable,
    Failed
}

public sealed class AssetUploadResponse
{
    public string AssetId { get; set; } = string.Empty;

    public string AssetType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public double SizeMb { get; set; }

    public string PublicUrl { get; set; } = string.Empty;

    public MediaInspectionResult Inspection { get; set; } = new();
}

public sealed class MediaInspectionResult
{
    public MediaInspectionStatus Status { get; set; } = MediaInspectionStatus.Unavailable;

    public string Source { get; set; } = string.Empty;

    public double? DurationSeconds { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public double? FrameRate { get; set; }

    public string Container { get; set; } = string.Empty;

    public string VideoCodec { get; set; } = string.Empty;

    public string AudioCodec { get; set; } = string.Empty;

    public string[] Warnings { get; set; } = [];
}
