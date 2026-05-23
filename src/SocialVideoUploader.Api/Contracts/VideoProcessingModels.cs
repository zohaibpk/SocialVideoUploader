namespace SocialVideoUploader.Api.Contracts;

public sealed class VideoProcessingProfile
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public PlatformId[] Platforms { get; set; } = [];

    public int Width { get; set; }

    public int Height { get; set; }

    public int TargetFrameRate { get; set; }

    public int? MaxDurationSeconds { get; set; }

    public int? TargetMaxFileSizeMb { get; set; }

    public string AspectRatio { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}

public sealed class VideoProcessingProfilesResponse
{
    public VideoProcessingProfile[] Profiles { get; set; } = [];
}

public sealed class GenerateVariantsRequest
{
    public UploadDraftRequest Draft { get; set; } = new();
}

public sealed class GenerateVariantsResponse
{
    public bool FfmpegAvailable { get; set; }

    public string[] Warnings { get; set; } = [];

    public VideoProcessingProfile[] Profiles { get; set; } = [];

    public GeneratedVariantResult[] Variants { get; set; } = [];
}

public sealed class GeneratedVariantResult
{
    public string ProfileId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public PlatformId[] Platforms { get; set; } = [];

    public bool Succeeded { get; set; }

    public string OutputUrl { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public double SizeMb { get; set; }

    public string CommandPreview { get; set; } = string.Empty;

    public MediaInspectionResult Inspection { get; set; } = new();

    public string[] Warnings { get; set; } = [];

    public string Error { get; set; } = string.Empty;
}
