using Microsoft.Extensions.Options;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Platforms;

public sealed class LinkedInPlatformPublisher(IOptions<PlatformsConfiguration> options)
    : PlatformPublisherBase<LinkedInPlatformConfiguration>(options.Value.LinkedIn)
{
    public override PlatformId PlatformId => PlatformId.LinkedIn;

    protected override string DisplayName => "LinkedIn";

    protected override string DestinationType => "Professional feed and organization video posts";

    protected override string Description => "Professional publishing with post commentary, optional video title, and a signed multi-part upload flow.";

    protected override PlatformCapability Capability => new()
    {
        SupportsTitle = true,
        SupportsTags = false,
        SupportsDescription = true,
        SupportsThumbnail = true,
        SupportsScheduling = false,
        SupportsPrivacy = true,
        SupportsPlatformOverrides = true,
        SupportsWatermark = true,
        SupportsTrim = true
    };

    protected override string[] CoreFields => ["Post commentary", "Video title", "Thumbnail", "Visibility"];

    protected override string[] AdvancedFields => ["Reshare behavior", "Captions"];

    protected override string[] Notes =>
    [
        "Use conversational copy for commentary instead of hashtag-heavy captions.",
        "500 MB is the safe planning ceiling even though some APIs expose a larger schema max.",
        "Landscape and vertical variants both fit the supported aspect-ratio grid."
    ];

    protected override void ValidatePlatformSpecific(
        PlatformSubmissionContext context,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        if (context.Draft.Asset.FrameRate > 30)
        {
            warnings.Add("LinkedIn is safer at 30 FPS or lower; the processing profile should downsample higher frame rates.");
        }

        if (context.EstimatedFileSizeMb > Configuration.SafeFileSizeMb)
        {
            warnings.Add($"LinkedIn should be compressed below {Configuration.SafeFileSizeMb} MB for the safest upload path.");
        }
    }
}
