using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Platforms;

public abstract class PlatformPublisherBase<TConfiguration>(TConfiguration configuration) : IPlatformPublisher
    where TConfiguration : PlatformApiConfiguration
{
    protected TConfiguration Configuration { get; } = configuration;

    public abstract PlatformId PlatformId { get; }

    protected abstract string DisplayName { get; }

    protected abstract string DestinationType { get; }

    protected abstract string Description { get; }

    protected abstract PlatformCapability Capability { get; }

    protected abstract string[] CoreFields { get; }

    protected abstract string[] AdvancedFields { get; }

    protected abstract string[] Notes { get; }

    public PlatformDefinition GetDefinition()
    {
        return PlatformDefinitionFactory.Create(
            PlatformId,
            DisplayName,
            DestinationType,
            Description,
            Capability,
            Configuration,
            CoreFields,
            AdvancedFields,
            Notes);
    }

    public virtual PlatformValidationResult Validate(PlatformSubmissionContext context)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        ValidateCommon(context, errors, warnings);
        ValidatePlatformSpecific(context, errors, warnings);

        return new PlatformValidationResult
        {
            PlatformId = PlatformId,
            DisplayName = DisplayName,
            IsValid = errors.Count == 0,
            Errors = errors.ToArray(),
            Warnings = warnings.ToArray(),
            Notes = Notes
        };
    }

    protected virtual void ValidateCommon(
        PlatformSubmissionContext context,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        var limits = context.Definition.Limits;

        if (!Configuration.Enabled)
        {
            errors.Add("This platform is currently disabled in configuration.");
        }

        if (limits.MaxDurationSeconds is { } maxDuration
            && context.TrimmedDurationSeconds > maxDuration)
        {
            errors.Add($"Trimmed duration exceeds the {maxDuration}-second platform limit.");
        }

        if (limits.MaxFileSizeMb is { } maxFileSizeMb
            && context.EstimatedFileSizeMb > maxFileSizeMb)
        {
            errors.Add($"Estimated file size exceeds the {maxFileSizeMb} MB limit.");
        }

        if (Capability.SupportsTitle
            && limits.MaxTitleLength is { } maxTitleLength
            && context.EffectiveTitle.Length > maxTitleLength)
        {
            errors.Add($"Title exceeds the {maxTitleLength}-character limit.");
        }

        if (Capability.SupportsDescription
            && limits.MaxDescriptionLength is { } maxDescriptionLength
            && context.EffectiveDescription.Length > maxDescriptionLength)
        {
            errors.Add($"Caption or description exceeds the {maxDescriptionLength}-character limit.");
        }

        if (Capability.SupportsTags
            && limits.MaxTags is { } maxTags
            && context.EffectiveTags.Count > maxTags)
        {
            errors.Add($"Tag count exceeds the {maxTags}-tag limit.");
        }

        if (context.Draft.Editing.WatermarkText.Length > 60)
        {
            errors.Add("Watermark text must be 60 characters or fewer.");
        }

        if (context.Draft.Editing.WatermarkOpacityPercent is < 10 or > 100)
        {
            errors.Add("Watermark opacity must be between 10% and 100%.");
        }

        if (Configuration.RequiresPublicUrlStaging)
        {
            warnings.Add("Publishing will require public staging storage before the API call.");
        }

        if (context.Draft.Advanced.ScheduledPublishAt.HasValue && !Capability.SupportsScheduling)
        {
            warnings.Add("Scheduling is not natively supported for this platform and will need app-side scheduling.");
        }

        if (!Capability.SupportsTitle && !string.IsNullOrWhiteSpace(context.EffectiveTitle))
        {
            warnings.Add("This platform does not prominently use a separate title field.");
        }

        if (!Capability.SupportsTags && context.EffectiveTags.Count > 0)
        {
            warnings.Add("Tags will need to be folded into the caption or omitted for this platform.");
        }
    }

    protected virtual void ValidatePlatformSpecific(
        PlatformSubmissionContext context,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
    }
}
