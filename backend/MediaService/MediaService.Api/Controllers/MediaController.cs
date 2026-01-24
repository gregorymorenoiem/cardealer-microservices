using MediaService.Application.Features.Media.Commands.InitUpload;
using MediaService.Application.Features.Media.Commands.FinalizeUpload;
using MediaService.Application.Features.Media.Queries.GetMedia;
using MediaService.Domain.Interfaces.Services;
using MediaService.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediaService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMediaStorageService _storageService;

    public MediaController(IMediator mediator, IMediaStorageService storageService)
    {
        _mediator = mediator;
        _storageService = storageService;
    }

    /// <summary>
    /// Simple direct file upload endpoint for KYC documents and other use cases
    /// </summary>
    [HttpPost("upload/image")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<ActionResult<object>> UploadImage([FromForm] IFormFile file, [FromForm] string? folder = "uploads")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file provided" });
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(new { error = "Invalid file type. Allowed: JPEG, PNG, GIF, WebP, PDF" });
        }

        try
        {
            // Generate storage key
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var storageKey = $"{folder}/{DateTime.UtcNow:yyyy/MM/dd}/{fileName}";

            // Upload to storage
            using var stream = file.OpenReadStream();
            await _storageService.UploadFileAsync(storageKey, stream, file.ContentType);

            // Get public URL
            var url = await _storageService.GetFileUrlAsync(storageKey);

            return Ok(new
            {
                url = url,
                publicId = storageKey,
                width = 0, // Would need image processing to get actual dimensions
                height = 0,
                format = Path.GetExtension(file.FileName).TrimStart('.'),
                size = file.Length
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Upload failed: {ex.Message}" });
        }
    }

    [HttpPost("upload/init")]
    public async Task<ActionResult<ApiResponse<InitUploadResponse>>> InitUpload(InitUploadCommand command)
    {
        // Extract DealerId from JWT claims
        var dealerIdClaim = User.FindFirst("dealerId")?.Value;
        if (string.IsNullOrEmpty(dealerIdClaim) || !Guid.TryParse(dealerIdClaim, out var dealerId))
        {
            return BadRequest(ApiResponse<InitUploadResponse>.Fail("Invalid or missing dealerId claim"));
        }

        // Override DealerId from token (security measure)
        var commandWithDealerId = new InitUploadCommand(
            dealerId,
            command.OwnerId,
            command.Context,
            command.FileName,
            command.ContentType,
            command.FileSize
        );

        var result = await _mediator.Send(commandWithDealerId);
        return Ok(result);
    }

    [HttpPost("upload/finalize/{mediaId}")]
    public async Task<ActionResult<ApiResponse<FinalizeUploadResponse>>> FinalizeUpload(string mediaId)
    {
        var command = new FinalizeUploadCommand(mediaId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{mediaId}")]
    public async Task<ActionResult<ApiResponse<GetMediaResponse>>> GetMedia(string mediaId)
    {
        var query = new GetMediaQuery(mediaId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}