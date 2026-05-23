using Microsoft.Extensions.Options;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Platforms;

public sealed class DailymotionPlatformPublisher(IOptions<PlatformsConfiguration> options)
    : PlatformPublisherBase<DailymotionPlatformConfiguration>(options.Value.Dailymotion)
{
    public override PlatformId PlatformId => PlatformId.Dailymotion;

    protected override string DisplayName => "Dailymotion";

    protected override string DestinationType => "Public video channel platform";

    protected override string Description => "Channel-based publishing with title, tags, channel/category, and account-tier upload limits.";

    protected override PlatformCapability Capability => new()
    {
        SupportsTitle = true,
        SupportsTags = true,
        SupportsDescription = true,
        SupportsThumbnail = true,
        SupportsScheduling = false,
        SupportsPrivacy = true,
        SupportsPlatformOverrides = true,
        SupportsWatermark = true,
        SupportsTrim = true
    };

    protected override string[] CoreFields => ["Title", "Description", "Tags", "Channel", "Privacy"];

    protected override string[] AdvancedFields => ["Explicit content", "Allow comments", "Thumbnail"];

    protected override string[] Notes =>
    [
        "Channel selection is an important categorization field for Dailymotion.",
        "The API requires an explicit created-for-kids style decision in the video create call.",
        "Account tier can change the effective duration and size ceiling."
    ];

    protected override void ValidatePlatformSpecific(
        PlatformSubmissionContext context,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(context.Draft.Advanced.DailymotionChannel))
        {
            warnings.Add("Dailymotion channel/category should be selected before publishing.");
        }

        if (Configuration.RequireMadeForKidsSelection)
        {
            warnings.Add("Dailymotion video creation should explicitly set the children-audience style compliance field.");
        }
    }
}
