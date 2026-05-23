using Microsoft.Extensions.Options;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Platforms;

public sealed class ThreadsPlatformPublisher(IOptions<PlatformsConfiguration> options)
    : PlatformPublisherBase<ThreadsPlatformConfiguration>(options.Value.Threads)
{
    public override PlatformId PlatformId => PlatformId.Threads;

    protected override string DisplayName => "Threads";

    protected override string DestinationType => "Public short-form social feed";

    protected override string Description => "Text-first short-form publishing with video attachments, public URL staging, and reply-control options.";

    protected override PlatformCapability Capability => new()
    {
        SupportsTitle = false,
        SupportsTags = false,
        SupportsDescription = true,
        SupportsThumbnail = false,
        SupportsScheduling = false,
        SupportsPrivacy = false,
        SupportsPlatformOverrides = true,
        SupportsWatermark = true,
        SupportsTrim = true
    };

    protected override string[] CoreFields => ["Text", "Reply control"];

    protected override string[] AdvancedFields => ["Topic tag", "Spoiler media", "Country restriction"];

    protected override string[] Notes =>
    [
        "Threads expects concise text with a hard 500-character style ceiling.",
        "The media upload path pulls from a public URL instead of receiving a direct binary upload.",
        "Per-platform caption overrides are important because Threads tone differs from YouTube or LinkedIn."
    ];

    protected override void ValidatePlatformSpecific(
        PlatformSubmissionContext context,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        if (context.EffectiveDescription.Length > Configuration.TextLimit)
        {
            errors.Add($"Threads text must stay within {Configuration.TextLimit} characters.");
        }

        if (context.Draft.Advanced.ScheduledPublishAt.HasValue)
        {
            warnings.Add("Threads does not provide native scheduling; queueing must happen in the application.");
        }
    }
}
