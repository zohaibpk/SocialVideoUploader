using Microsoft.Extensions.Options;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Platforms;

public sealed class TelegramPlatformPublisher(IOptions<PlatformsConfiguration> options)
    : PlatformPublisherBase<TelegramPlatformConfiguration>(options.Value.Telegram)
{
    public override PlatformId PlatformId => PlatformId.Telegram;

    protected override string DisplayName => "Telegram Channels";

    protected override string DestinationType => "Broadcast channel delivery";

    protected override string Description => "Channel-based distribution through the Bot API with a strict size ceiling and inline caption support.";

    protected override PlatformCapability Capability => new()
    {
        SupportsTitle = false,
        SupportsTags = false,
        SupportsDescription = true,
        SupportsThumbnail = true,
        SupportsScheduling = false,
        SupportsPrivacy = false,
        SupportsPlatformOverrides = true,
        SupportsWatermark = true,
        SupportsTrim = true
    };

    protected override string[] CoreFields => ["Caption", "Thumbnail", "Supports streaming"];

    protected override string[] AdvancedFields => ["Protect content", "Spoiler", "Inline keyboard"];

    protected override string[] Notes =>
    [
        "The Bot API hard limit is the most restrictive current file-size ceiling in scope.",
        "Telegram works best with a dedicated compact transcode profile and fast-start MP4 output.",
        "Bots must be administrators of the destination channel."
    ];

    protected override void ValidatePlatformSpecific(
        PlatformSubmissionContext context,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        if (context.EstimatedFileSizeMb > (Configuration.MaxFileSizeMb ?? 50))
        {
            errors.Add("Telegram requires a compact variant under the Bot API file-size ceiling.");
        }

        if (context.TrimmedDurationSeconds > 180)
        {
            warnings.Add("Telegram quality is much easier to preserve when clips stay around three minutes or shorter.");
        }

        if (string.IsNullOrWhiteSpace(context.Draft.Advanced.TelegramCaption) && string.IsNullOrWhiteSpace(context.EffectiveDescription))
        {
            warnings.Add("Telegram posts benefit from a dedicated caption because title fields are not used.");
        }
    }
}
