using SocialVideoUploader.Api.Configurations;

namespace SocialVideoUploader.Api.Contracts;

public enum PlatformId
{
    YouTube,
    Facebook,
    Instagram,
    LinkedIn,
    Vimeo,
    Dailymotion,
    Threads,
    Tumblr,
    Telegram
}

public enum PrivacyOption
{
    Public,
    Private,
    Unlisted
}

public enum WatermarkPosition
{
    TopLeft,
    TopRight,
    Center,
    BottomLeft,
    BottomRight
}

public enum InstagramMediaType
{
    Reels,
    Video,
    Story
}

public enum ThreadsReplyControl
{
    Everyone,
    AccountsYouFollow,
    MentionedOnly,
    FollowersOnly,
    ParentPostAuthorOnly
}

public enum LinkedInVisibility
{
    Public,
    LoggedIn
}

public enum VimeoPrivacyView
{
    Anybody,
    Nobody,
    Password,
    Contacts
}

public enum TumblrPostState
{
    Published,
    Draft,
    Queue,
    Private
}

public sealed class UploadPlanningResponse
{
    public string ApplicationName { get; set; } = "SocialVideoUploader";

    public string[] Summary { get; set; } = [];

    public UploadFormSection[] Sections { get; set; } = [];

    public PlatformDefinition[] Platforms { get; set; } = [];

    public UploadDraftRequest Draft { get; set; } = new();
}

public sealed class UploadFormSection
{
    public string Key { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string[] Fields { get; set; } = [];
}

public sealed class PlatformDefinition
{
    public PlatformId Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string DestinationType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public PlatformCapability Capability { get; set; } = new();

    public PlatformLimitProfile Limits { get; set; } = new();

    public string[] CoreFields { get; set; } = [];

    public string[] AdvancedFields { get; set; } = [];

    public string[] Notes { get; set; } = [];

    public PlatformConnectionProfile Connection { get; set; } = new();
}

public sealed class PlatformCapability
{
    public bool SupportsTitle { get; set; }

    public bool SupportsTags { get; set; }

    public bool SupportsDescription { get; set; }

    public bool SupportsThumbnail { get; set; }

    public bool SupportsScheduling { get; set; }

    public bool SupportsPrivacy { get; set; }

    public bool SupportsPlatformOverrides { get; set; }

    public bool SupportsWatermark { get; set; }

    public bool SupportsTrim { get; set; }
}

public sealed class PlatformLimitProfile
{
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

public sealed class PlatformConnectionProfile
{
    public string ApiBaseUrl { get; set; } = string.Empty;

    public string AuthBaseUrl { get; set; } = string.Empty;

    public bool RequiresPublicUrlStaging { get; set; }
}

public sealed class UploadDraftRequest
{
    public AssetSourceInput Asset { get; set; } = new();

    public BasicUploadInput Basic { get; set; } = new();

    public EditingInput Editing { get; set; } = new();

    public AdvancedUploadInput Advanced { get; set; } = new();

    public List<PlatformOverrideInput> PlatformOverrides { get; set; } = [];
}

public sealed class AssetSourceInput
{
    public string VideoAssetId { get; set; } = string.Empty;

    public string SourceFileName { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public double SourceDurationSeconds { get; set; } = 145;

    public double EstimatedFileSizeMb { get; set; } = 180;

    public int Width { get; set; } = 3840;

    public int Height { get; set; } = 2160;

    public double FrameRate { get; set; } = 60;

    public string InspectionStatus { get; set; } = "Planning";

    public string[] InspectionWarnings { get; set; } = [];
}

public sealed class BasicUploadInput
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string TagsText { get; set; } = string.Empty;

    public List<PlatformId> Platforms { get; set; } = [];

    public PrivacyOption Privacy { get; set; } = PrivacyOption.Private;

    public string ThumbnailUrl { get; set; } = string.Empty;
}

public sealed class EditingInput
{
    public double TrimStartSeconds { get; set; }

    public double TrimEndSeconds { get; set; } = 75;

    public string WatermarkText { get; set; } = string.Empty;

    public WatermarkPosition WatermarkPosition { get; set; } = WatermarkPosition.BottomRight;

    public int WatermarkOpacityPercent { get; set; } = 80;
}

public sealed class AdvancedUploadInput
{
    public DateTimeOffset? ScheduledPublishAt { get; set; }

    public string Language { get; set; } = "en";

    public string YouTubeCategoryId { get; set; } = "22";

    public bool? YouTubeMadeForKids { get; set; }

    public string InstagramCoverImageUrl { get; set; } = string.Empty;

    public bool InstagramShareToFeed { get; set; } = true;

    public InstagramMediaType InstagramMediaType { get; set; } = InstagramMediaType.Reels;

    public LinkedInVisibility LinkedInVisibility { get; set; } = LinkedInVisibility.Public;

    public VimeoPrivacyView VimeoPrivacyView { get; set; } = VimeoPrivacyView.Anybody;

    public string DailymotionChannel { get; set; } = "tech";

    public bool DailymotionExplicitContent { get; set; }

    public ThreadsReplyControl ThreadsReplyControl { get; set; } = ThreadsReplyControl.Everyone;

    public string ThreadsTopicTag { get; set; } = string.Empty;

    public TumblrPostState TumblrPostState { get; set; } = TumblrPostState.Published;

    public string TelegramCaption { get; set; } = string.Empty;

    public bool TelegramProtectContent { get; set; }

    public bool TelegramSupportsStreaming { get; set; } = true;

    public bool CommentsAllowed { get; set; } = true;
}

public sealed class PlatformOverrideInput
{
    public PlatformId PlatformId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string TagsText { get; set; } = string.Empty;

    public string ThumbnailUrl { get; set; } = string.Empty;
}

public sealed class UploadValidationResponse
{
    public bool IsValid { get; set; }

    public string[] Errors { get; set; } = [];

    public string[] Warnings { get; set; } = [];

    public string[] SuggestedActions { get; set; } = [];

    public PlatformValidationResult[] Platforms { get; set; } = [];
}

public sealed class PlatformValidationResult
{
    public PlatformId PlatformId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public bool IsValid { get; set; }

    public string[] Errors { get; set; } = [];

    public string[] Warnings { get; set; } = [];

    public string[] Notes { get; set; } = [];
}

public sealed class PlatformSubmissionContext
{
    public required PlatformDefinition Definition { get; init; }

    public required UploadDraftRequest Draft { get; init; }

    public required string EffectiveTitle { get; init; }

    public required string EffectiveDescription { get; init; }

    public required string EffectiveThumbnailUrl { get; init; }

    public required IReadOnlyList<string> EffectiveTags { get; init; }

    public required double TrimmedDurationSeconds { get; init; }

    public required double EstimatedFileSizeMb { get; init; }
}

public static class PlatformDefinitionFactory
{
    public static PlatformDefinition Create(
        PlatformId id,
        string displayName,
        string destinationType,
        string description,
        PlatformCapability capability,
        PlatformApiConfiguration configuration,
        string[] coreFields,
        string[] advancedFields,
        string[] notes)
    {
        return new PlatformDefinition
        {
            Id = id,
            DisplayName = displayName,
            DestinationType = destinationType,
            Description = description,
            Capability = capability,
            CoreFields = coreFields,
            AdvancedFields = advancedFields,
            Notes = notes,
            Limits = new PlatformLimitProfile
            {
                MaxTitleLength = configuration.MaxTitleLength,
                MaxDescriptionLength = configuration.MaxDescriptionLength,
                MaxTags = configuration.MaxTags,
                MaxFileSizeMb = configuration.MaxFileSizeMb,
                MaxDurationSeconds = configuration.MaxDurationSeconds,
                RecommendedAspectRatio = configuration.RecommendedAspectRatio,
                RecommendedResolution = configuration.RecommendedResolution,
                UploadStyle = configuration.UploadStyle,
                SupportedPrivacyValues = configuration.SupportedPrivacyValues
            },
            Connection = new PlatformConnectionProfile
            {
                ApiBaseUrl = configuration.ApiBaseUrl,
                AuthBaseUrl = configuration.AuthBaseUrl,
                RequiresPublicUrlStaging = configuration.RequiresPublicUrlStaging
            }
        };
    }
}
