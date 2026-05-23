using Microsoft.Extensions.Options;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Platforms;

public sealed class YouTubePlatformPublisher(IOptions<PlatformsConfiguration> options)
    : PlatformPublisherBase<YouTubePlatformConfiguration>(options.Value.YouTube)
{
    public override PlatformId PlatformId => PlatformId.YouTube;

    protected override string DisplayName => "YouTube";

    protected override string DestinationType => "Public video platform";

    protected override string Description => "Long-form publishing with strong SEO metadata, thumbnails, privacy, and scheduling support.";

    protected override PlatformCapability Capability => new()
    {
        SupportsTitle = true,
        SupportsTags = true,
        SupportsDescription = true,
        SupportsThumbnail = true,
        SupportsScheduling = true,
        SupportsPrivacy = true,
        SupportsPlatformOverrides = true,
        SupportsWatermark = true,
        SupportsTrim = true
    };

    protected override string[] CoreFields => ["Title", "Description", "Tags", "Privacy", "Thumbnail"];

    protected override string[] AdvancedFields => ["Category", "Made for kids", "Schedule", "Language"];

    protected override string[] Notes =>
    [
        "Quota is the main operational limit, not media size.",
        "Use private or unlisted defaults for test uploads.",
        "Watermark and trim are safe to apply in the preprocessing pipeline."
    ];

    protected override void ValidatePlatformSpecific(
        PlatformSubmissionContext context,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(context.EffectiveTitle))
        {
            errors.Add("YouTube requires a title.");
        }

        if (!context.Draft.Advanced.YouTubeMadeForKids.HasValue)
        {
            warnings.Add("YouTube made-for-kids selection should be explicit before publishing.");
        }

        if (context.Draft.Advanced.ScheduledPublishAt.HasValue
            && context.Draft.Basic.Privacy != PrivacyOption.Private)
        {
            warnings.Add("Scheduled YouTube publishing typically starts from a private state before publishAt.");
        }
    }
}
