using Microsoft.AspNetCore.Mvc;
using SocialVideoUploader.Api.Contracts;
using SocialVideoUploader.Api.Services;

namespace SocialVideoUploader.Api.Controllers;

[ApiController]
[Route("api/uploads")]
public sealed class UploadPlanningController(UploadPlanningService uploadPlanningService) : ControllerBase
{
    [HttpGet("form")]
    public ActionResult<UploadPlanningResponse> GetForm()
    {
        return Ok(uploadPlanningService.GetForm());
    }

    [HttpPost("validate")]
    public ActionResult<UploadValidationResponse> Validate([FromBody] UploadDraftRequest request)
    {
        return Ok(uploadPlanningService.Validate(request));
    }
}
