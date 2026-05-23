using Microsoft.AspNetCore.Mvc;

namespace SocialVideoUploader.Api.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("status")]
    public ActionResult<ApiStatusResponse> GetStatus()
    {
        return Ok(new ApiStatusResponse(
            Name: "SocialVideoUploader API",
            Status: "Ready for development",
            Version: "v0",
            ServerTimeUtc: DateTimeOffset.UtcNow));
    }
}

public sealed record ApiStatusResponse(
    string Name,
    string Status,
    string Version,
    DateTimeOffset ServerTimeUtc);
