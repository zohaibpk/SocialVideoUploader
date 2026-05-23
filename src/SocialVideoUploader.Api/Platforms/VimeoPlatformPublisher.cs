using Microsoft.Extensions.Options;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Platforms;

public sealed class VimeoPlatformPublisher(IOptions<PlatformsConfiguration> options)
    : PlatformPublisherBase<VimeoPlatformConfiguration>(options.Value.Vimeo)
{
    public override PlatformId PlatformId => PlatformId.Vimeo;

    protected override string DisplayName => "Vimeo";

    protected override string DestinationType => "Professional hosting and sharing library";

    protected override string Description => "Pro video hosting with strong privacy controls, TUS uploads, and account-specific quota considerations.";

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

    protected override string[] AdvancedFields => ["Comments", "Embed privacy", "Language", "Schedule"];

    protected override string[] Notes =>
    [
        "Plan quota is a user-account concern and should be checked before upload.",
        "Vimeo is ideal for long-form, higher-quality variants.",
        "A per-user upload quota check belongs in the eventual publishing service."
    ];
}
