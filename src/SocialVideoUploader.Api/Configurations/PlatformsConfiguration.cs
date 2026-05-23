namespace SocialVideoUploader.Api.Configurations;

public sealed class PlatformsConfiguration
{
    public const string SectionName = "Platforms";

    public YouTubePlatformConfiguration YouTube { get; set; } = new();

    public FacebookPlatformConfiguration Facebook { get; set; } = new();

    public InstagramPlatformConfiguration Instagram { get; set; } = new();

    public LinkedInPlatformConfiguration LinkedIn { get; set; } = new();

    public VimeoPlatformConfiguration Vimeo { get; set; } = new();

    public DailymotionPlatformConfiguration Dailymotion { get; set; } = new();

    public ThreadsPlatformConfiguration Threads { get; set; } = new();

    public TumblrPlatformConfiguration Tumblr { get; set; } = new();

    public TelegramPlatformConfiguration Telegram { get; set; } = new();
}

public abstract class PlatformApiConfiguration
{
    public bool Enabled { get; set; } = true;

    public string ApiBaseUrl { get; set; } = string.Empty;

    public string AuthBaseUrl { get; set; } = string.Empty;

    public bool RequiresPublicUrlStaging { get; set; }

    public int? MaxTitleLength { get; set; }

    public int? MaxDescriptionLength { get; set; }

    public int? MaxTags { get; set; }

    public int? MaxFileSizeMb { get; set; }

    public int? MaxDurationSeconds { get; set; }

    public string RecommendedAspectRatio { get; set; } = string.Empty;

    public string RecommendedResolution { get; set; } = string.Empty;

    public string UploadStyle { get; set; } = string.Empty;

    public string[] SupportedPrivacyValues { get; set; } = [];
}

public sealed class YouTubePlatformConfiguration : PlatformApiConfiguration
{
    public string UploadScope { get; set; } = string.Empty;
}

public sealed class FacebookPlatformConfiguration : PlatformApiConfiguration
{
    public int MaxReelDurationSeconds { get; set; } = 90;
}

public sealed class InstagramPlatformConfiguration : PlatformApiConfiguration
{
    public int MaxStoryDurationSeconds { get; set; } = 60;
}

public sealed class LinkedInPlatformConfiguration : PlatformApiConfiguration
{
    public int SafeFileSizeMb { get; set; } = 500;
}

public sealed class VimeoPlatformConfiguration : PlatformApiConfiguration
{
    public string UploadApproach { get; set; } = "tus";
}

public sealed class DailymotionPlatformConfiguration : PlatformApiConfiguration
{
    public bool RequireMadeForKidsSelection { get; set; } = true;
}

public sealed class ThreadsPlatformConfiguration : PlatformApiConfiguration
{
    public int TextLimit { get; set; } = 500;
}

public sealed class TumblrPlatformConfiguration : PlatformApiConfiguration
{
    public int MaxDailyVideos { get; set; } = 20;
}

public sealed class TelegramPlatformConfiguration : PlatformApiConfiguration
{
    public bool SupportsStreamingByDefault { get; set; } = true;
}
