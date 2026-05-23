using Microsoft.Extensions.Options;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Platforms;

public sealed class InstagramPlatformPublisher(IOptions<PlatformsConfiguration> options)
    : PlatformPublisherBase<InstagramPlatformConfiguration>(options.Value.Instagram)
{
    public override PlatformId PlatformId => PlatformId.Instagram;

    protected override string DisplayName => "Instagram Professional";

    protected override string DestinationType => "Professional account video, reels, and stories";

    protected override string Description => "Professional-account publishing with a media container flow, strong short-form support, and optional public URL staging.";

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

    protected override string[] CoreFields => ["Caption", "Cover image", "Media type"];

    protected override string[] AdvancedFields => ["Share to feed", "Location", "Collaborators"];

    protected override string[] Notes =>
    [
        "Hashtags live inside the caption instead of a separate tags field.",
        "Story and reel duration caps should be validated separately.",
        "Safe-area-aware watermark placement matters more on vertical outputs."
    ];

    protected override void ValidatePlatformSpecific(
        PlatformSubmissionContext context,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        var mediaType = context.Draft.Advanced.InstagramMediaType;

        if (mediaType == InstagramMediaType.Story
            && context.TrimmedDurationSeconds > Configuration.MaxStoryDurationSeconds)
        {
            errors.Add($"Instagram Stories support up to {Configuration.MaxStoryDurationSeconds} seconds.");
        }

        if (mediaType == InstagramMediaType.Reels
            && context.TrimmedDurationSeconds > (Configuration.MaxDurationSeconds ?? 900))
        {
            errors.Add("Instagram Reels support up to 15 minutes.");
        }

        if (!string.IsNullOrWhiteSpace(context.EffectiveTitle))
        {
            warnings.Add("Instagram does not use a separate title field; title content should move into the caption if needed.");
        }
    }
}
