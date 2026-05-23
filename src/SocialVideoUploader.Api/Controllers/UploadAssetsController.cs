using Microsoft.AspNetCore.Mvc;
using SocialVideoUploader.Api.Contracts;
using SocialVideoUploader.Api.Services;

namespace SocialVideoUploader.Api.Controllers;

[ApiController]
[Route("api/uploads/assets")]
public sealed class UploadAssetsController(
    AssetStorageService assetStorageService,
    MediaInspectionService mediaInspectionService) : ControllerBase
{
    [HttpPost("video")]
    [RequestSizeLimit(536_870_912)]
    public async Task<ActionResult<AssetUploadResponse>> UploadVideo(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest("A video file is required.");
        }

        if (!file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("The uploaded file must be a video.");
        }

        try
        {
            var asset = await assetStorageService.SaveVideoAsync(file, cancellationToken);
            var inspection = await mediaInspectionService.InspectAsync(asset, cancellationToken);

            return Ok(BuildResponse(asset, inspection));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost("thumbnail")]
    [RequestSizeLimit(10_485_760)]
    public async Task<ActionResult<AssetUploadResponse>> UploadThumbnail(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest("A thumbnail file is required.");
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("The uploaded file must be an image.");
        }

        try
        {
            var asset = await assetStorageService.SaveThumbnailAsync(file, cancellationToken);

            return Ok(BuildResponse(asset, new MediaInspectionResult
            {
                Status = MediaInspectionStatus.Partial,
                Source = "image-upload",
                Warnings = []
            }));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    private static AssetUploadResponse BuildResponse(StoredAsset asset, MediaInspectionResult inspection)
    {
        return new AssetUploadResponse
        {
            AssetId = asset.AssetId,
            AssetType = asset.AssetType,
            FileName = asset.OriginalFileName,
            ContentType = asset.ContentType,
            SizeBytes = asset.SizeBytes,
            SizeMb = asset.SizeBytes / 1024d / 1024d,
            PublicUrl = asset.PublicUrl,
            Inspection = inspection
        };
    }
}
