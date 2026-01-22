using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Application.Features.Videos.Commands;
using SpyneIntegrationService.Application.Features.Videos.Queries;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VideosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VideosController> _logger;

    public VideosController(IMediator mediator, ILogger<VideosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Generate a video tour from vehicle images
    /// </summary>
    /// <remarks>
    /// Creates a cinematic video from 5-100 images with optional background music.
    /// Available styles: Cinematic, Dynamic, Showcase, Social, Premium.
    /// Processing time: 5-15 minutes depending on settings.
    /// </remarks>
    [HttpPost("generate")]
    [Authorize]
    [ProducesResponseType(typeof(GenerateVideoResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateVideo([FromBody] GenerateVideoRequest request)
    {
        var command = new GenerateVideoCommand
        {
            VehicleId = request.VehicleId,
            DealerId = request.DealerId,
            ImageUrls = request.ImageUrls,
            Style = request.Style,
            OutputFormat = request.OutputFormat,
            BackgroundPreset = request.BackgroundPreset,
            CustomBackgroundId = request.CustomBackgroundId,
            IncludeMusic = request.IncludeMusic,
            MusicTrackId = request.MusicTrackId,
            DurationSeconds = request.DurationSeconds
        };

        var result = await _mediator.Send(command);
        
        _logger.LogInformation("Video generation started for vehicle {VehicleId} with style {Style}", 
            request.VehicleId, request.Style);
        
        return AcceptedAtAction(nameof(GetVideoStatus), new { id = result.VideoId }, result);
    }

    /// <summary>
    /// Get video generation status by ID
    /// </summary>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(VideoGenerationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVideoStatus(Guid id)
    {
        var result = await _mediator.Send(new GetVideoStatusQuery(id));
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get all videos for a vehicle
    /// </summary>
    [HttpGet("vehicle/{vehicleId:guid}")]
    [ProducesResponseType(typeof(List<VideoGenerationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehicleVideos(Guid vehicleId)
    {
        var result = await _mediator.Send(new GetVehicleVideosQuery(vehicleId));
        return Ok(result);
    }
}

// Request DTOs
public record GenerateVideoRequest
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public VideoStyle Style { get; init; } = VideoStyle.Cinematic;
    public VideoFormat OutputFormat { get; init; } = VideoFormat.Mp4_1080p;
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Studio;
    public string? CustomBackgroundId { get; init; }
    public bool IncludeMusic { get; init; } = true;
    public string? MusicTrackId { get; init; }
    public int? DurationSeconds { get; init; }
}
