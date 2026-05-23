using Microsoft.AspNetCore.Mvc;
using SocialVideoUploader.Api.Contracts;
using SocialVideoUploader.Api.Services;

namespace SocialVideoUploader.Api.Controllers;

[ApiController]
[Route("api/uploads/process")]
public sealed class UploadProcessingController(VideoProcessingService videoProcessingService) : ControllerBase
{
    [HttpGet("profiles")]
    public ActionResult<VideoProcessingProfilesResponse> GetProfiles()
    {
        return Ok(new VideoProcessingProfilesResponse
        {
            Profiles = videoProcessingService.GetProfiles().ToArray()
        });
    }

    [HttpPost("variants")]
    public async Task<ActionResult<GenerateVariantsResponse>> GenerateVariants(
        [FromBody] GenerateVariantsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await videoProcessingService.GenerateVariantsAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}
