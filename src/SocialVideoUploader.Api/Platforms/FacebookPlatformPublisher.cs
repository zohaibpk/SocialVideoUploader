using Microsoft.Extensions.Options;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Platforms;

public sealed class FacebookPlatformPublisher(IOptions<PlatformsConfiguration> options)
    : PlatformPublisherBase<FacebookPlatformConfiguration>(options.Value.Facebook)
{
    public override PlatformId PlatformId => PlatformId.Facebook;

    protected override string DisplayName => "Facebook Pages";

    protected override string DestinationType => "Page reels and video publishing";

    protected override string Description => "Meta Page publishing focused on reels and short-form page content with standard review-gated permissions.";

    protected override PlatformCapability Capability => new()
    {
        SupportsTitle = false,
        SupportsTags = false,
        SupportsDescription = true,
        SupportsThumbnail = true,
        SupportsScheduling = true,
        SupportsPrivacy = false,
        SupportsPlatformOverrides = true,
        SupportsWatermark = true,
        SupportsTrim = true
    };

    protected override string[] CoreFields => ["Caption", "Thumbnail"];

    protected override string[] AdvancedFields => ["Schedule", "Collaborator", "Reel-safe duration"];

    protected override string[] Notes =>
    [
        "Page-only publishing; personal profile publishing is out of scope.",
        "Reels are public-facing, so privacy selection becomes advisory.",
        "The 90-second reel limit should drive auto-validation."
    ];

    protected override void ValidatePlatformSpecific(
        PlatformSubmissionContext context,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        if (context.TrimmedDurationSeconds > Configuration.MaxReelDurationSeconds)
        {
            errors.Add($"Facebook Reels support up to {Configuration.MaxReelDurationSeconds} seconds.");
        }

        if (context.Draft.Basic.Privacy != PrivacyOption.Public)
        {
            warnings.Add("Facebook Reels are effectively public for Page publishing.");
        }
    }
}
