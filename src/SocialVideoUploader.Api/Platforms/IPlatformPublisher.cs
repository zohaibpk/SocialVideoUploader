using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Platforms;

public interface IPlatformPublisher
{
    PlatformId PlatformId { get; }

    PlatformDefinition GetDefinition();

    PlatformValidationResult Validate(PlatformSubmissionContext context);
}
