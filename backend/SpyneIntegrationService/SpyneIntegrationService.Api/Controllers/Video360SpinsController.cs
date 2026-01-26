using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Application.Features.Video360Spins.Commands;
using SpyneIntegrationService.Application.Features.Video360Spins.Queries;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Api.Controllers;

/// <summary>
/// Controller for video-based 360° spin generation.
/// 
/// FLUJO PRINCIPAL:
/// 1. Usuario graba video de 30-90 segundos caminando alrededor del vehículo
/// 2. Video se sube a S3/MediaService → obtiene URL
/// 3. POST /api/video360spins/generate con la URL del video
/// 4. Spyne extrae automáticamente 36-72 frames del video
/// 5. Spyne procesa los frames (background, enhancement)
/// 6. Spyne genera viewer 360° interactivo
/// 7. GET /api/video360spins/{id}/status para polling o webhook callback
/// </summary>
[ApiController]
[Route("api/video360spins")]
[Produces("application/json")]
public class Video360SpinsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<Video360SpinsController> _logger;

    public Video360SpinsController(IMediator mediator, ILogger<Video360SpinsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Generate a 360° spin view from a video walkthrough
    /// </summary>
    /// <remarks>
    /// Send a URL to a video (30-90 seconds recommended) where the camera walks around the vehicle.
    /// Spyne will:
    /// 1. Extract frames from the video automatically
    /// 2. Process each frame (background replacement, color correction)
    /// 3. Generate an interactive 360° spin viewer
    /// 
    /// Recommended video specs:
    /// - Duration: 30-90 seconds for one complete rotation
    /// - Resolution: 1080p or higher
    /// - Format: MP4 or MOV
    /// - Movement: Steady, consistent speed around vehicle
    /// - Lighting: Well-lit, avoid shadows
    /// </remarks>
    [HttpPost("generate")]
    [Authorize]
    [ProducesResponseType(typeof(GenerateVideo360SpinResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateFromVideo([FromBody] GenerateVideo360SpinRequest request)
    {
        if (string.IsNullOrEmpty(request.VideoUrl))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = "VideoUrl is required. Upload the video to MediaService first to get a URL.",
                Status = 400
            });
        }

        var command = new GenerateVideo360SpinCommand
        {
            VehicleId = request.VehicleId,
            DealerId = request.DealerId,
            UserId = GetCurrentUserId(),
            VideoUrl = request.VideoUrl,
            VideoDurationSeconds = request.VideoDurationSeconds,
            VideoFileSizeBytes = request.VideoFileSizeBytes,
            VideoFormat = request.VideoFormat,
            VideoResolution = request.VideoResolution,
            Type = request.Type,
            FrameCount = request.FrameCount,
            BackgroundPreset = request.BackgroundPreset,
            CustomBackgroundId = request.CustomBackgroundId,
            EnableHotspots = request.EnableHotspots,
            MaskLicensePlate = request.MaskLicensePlate,
            WebhookUrl = request.WebhookUrl
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation(
            "Video360Spin generation started. SpinId: {SpinId}, VehicleId: {VehicleId}, VideoUrl: {VideoUrl}", 
            result.SpinId, 
            request.VehicleId, 
            !string.IsNullOrEmpty(request.VideoUrl) 
                ? request.VideoUrl.Substring(0, Math.Min(100, request.VideoUrl.Length))
                : "(empty)");

        return AcceptedAtAction(
            nameof(GetStatus), 
            new { id = result.SpinId }, 
            result);
    }

    /// <summary>
    /// Get 360° spin generation status by ID
    /// </summary>
    /// <remarks>
    /// Use this endpoint to poll for status updates after submitting a video.
    /// 
    /// Status values:
    /// - Pending: Waiting in queue
    /// - Uploading: Video being sent to Spyne
    /// - ExtractingFrames: Spyne extracting frames from video
    /// - Processing: Frames being processed (background, enhancement)
    /// - Completed: 360° viewer ready
    /// - Failed: Error occurred (see errorMessage)
    /// 
    /// Once completed, response includes:
    /// - spinViewerUrl: Interactive 360° viewer URL
    /// - extractedFrameUrls: Array of processed frame images
    /// - thumbnailUrl: Preview image
    /// </remarks>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(Video360SpinStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var result = await _mediator.Send(new GetVideo360SpinStatusQuery(id));
        
        if (result == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Video360Spin with ID {id} not found",
                Status = 404
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get 360° spin details by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Video360SpinDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetVideo360SpinStatusQuery(id));
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get the latest 360° spin for a vehicle
    /// </summary>
    [HttpGet("vehicle/{vehicleId:guid}")]
    [ProducesResponseType(typeof(Video360SpinDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByVehicle(Guid vehicleId)
    {
        var result = await _mediator.Send(new GetVideo360SpinByVehicleQuery(vehicleId));
        
        if (result == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"No 360° spin found for vehicle {vehicleId}",
                Status = 404
            });
        }
            
        return Ok(result);
    }

    /// <summary>
    /// Get all 360° spins for a vehicle (history)
    /// </summary>
    [HttpGet("vehicle/{vehicleId:guid}/all")]
    [ProducesResponseType(typeof(List<Video360SpinDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllByVehicle(Guid vehicleId)
    {
        var result = await _mediator.Send(new GetVideo360SpinsByVehicleQuery(vehicleId));
        return Ok(result);
    }

    /// <summary>
    /// Get all 360° spins for a dealer
    /// </summary>
    [HttpGet("dealer/{dealerId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(List<Video360SpinDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDealer(Guid dealerId)
    {
        var result = await _mediator.Send(new GetVideo360SpinsByDealerQuery(dealerId));
        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            return userId;
        return null;
    }
}
