using Microsoft.Extensions.Options;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Platforms;

public sealed class TumblrPlatformPublisher(IOptions<PlatformsConfiguration> options)
    : PlatformPublisherBase<TumblrPlatformConfiguration>(options.Value.Tumblr)
{
    public override PlatformId PlatformId => PlatformId.Tumblr;

    protected override string DisplayName => "Tumblr";

    protected override string DestinationType => "Blog-style social publishing";

    protected override string Description => "Video posts with captions, tags, queue states, and single-video transcoding behavior per account.";

    protected override PlatformCapability Capability => new()
    {
        SupportsTitle = false,
        SupportsTags = true,
        SupportsDescription = true,
        SupportsThumbnail = false,
        SupportsScheduling = true,
        SupportsPrivacy = true,
        SupportsPlatformOverrides = true,
        SupportsWatermark = true,
        SupportsTrim = true
    };

    protected override string[] CoreFields => ["Caption", "Tags", "Post state"];

    protected override string[] AdvancedFields => ["Queue date", "Slug", "Format"];

    protected override string[] Notes =>
    [
        "Tumblr serializes video transcoding per account, so publish jobs should be queued instead of parallelized.",
        "Tags are comma-separated and remain useful for discovery on Tumblr.",
        "Queue and draft states are worth supporting directly."
    ];

    protected override void ValidatePlatformSpecific(
        PlatformSubmissionContext context,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        if (context.Draft.Advanced.TumblrPostState == TumblrPostState.Queue
            && !context.Draft.Advanced.ScheduledPublishAt.HasValue)
        {
            warnings.Add("Queued Tumblr posts usually benefit from a scheduled date.");
        }

        warnings.Add($"Tumblr uploads should be serialized because only one video can transcode at a time per account; the daily video ceiling is {Configuration.MaxDailyVideos}.");
    }
}
