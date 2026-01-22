using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Application.Features.Images.Commands;
using SpyneIntegrationService.Application.Features.Images.Queries;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ImagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(IMediator mediator, ILogger<ImagesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Transform a single vehicle image
    /// </summary>
    /// <remarks>
    /// Applies AI-powered transformations like background removal, enhancement, or license plate masking.
    /// Processing is asynchronous - use the returned ID to check status.
    /// </remarks>
    [HttpPost("transform")]
    [Authorize]
    [ProducesResponseType(typeof(ImageTransformationDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TransformImage([FromBody] TransformImageRequest request)
    {
        var command = new TransformImageCommand
        {
            VehicleId = request.VehicleId,
            DealerId = request.DealerId,
            SourceImageUrl = request.SourceImageUrl,
            TransformationType = request.TransformationType,
            BackgroundPreset = request.BackgroundPreset,
            CustomBackgroundId = request.CustomBackgroundId
        };

        var result = await _mediator.Send(command);
        
        _logger.LogInformation("Image transformation started for vehicle {VehicleId}", request.VehicleId);
        
        return AcceptedAtAction(nameof(GetTransformationStatus), new { id = result.Id }, result);
    }

    /// <summary>
    /// Transform multiple vehicle images in batch
    /// </summary>
    /// <remarks>
    /// Processes up to 50 images in a single request. All images receive the same transformation.
    /// </remarks>
    [HttpPost("transform/batch")]
    [Authorize]
    [ProducesResponseType(typeof(BatchTransformationResultDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TransformBatchImages([FromBody] TransformBatchImagesRequest request)
    {
        var command = new TransformBatchImagesCommand
        {
            VehicleId = request.VehicleId,
            DealerId = request.DealerId,
            SourceImageUrls = request.SourceImageUrls,
            TransformationType = request.TransformationType,
            BackgroundPreset = request.BackgroundPreset,
            CustomBackgroundId = request.CustomBackgroundId
        };

        var result = await _mediator.Send(command);
        
        _logger.LogInformation("Batch transformation started for vehicle {VehicleId} with {Count} images", 
            request.VehicleId, request.SourceImageUrls.Count);
        
        return Accepted(result);
    }

    /// <summary>
    /// Get transformation status by ID
    /// </summary>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(ImageTransformationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransformationStatus(Guid id)
    {
        var result = await _mediator.Send(new GetTransformationStatusQuery(id));
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get all transformations for a vehicle
    /// </summary>
    [HttpGet("vehicle/{vehicleId:guid}")]
    [ProducesResponseType(typeof(List<ImageTransformationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehicleTransformations(Guid vehicleId)
    {
        var result = await _mediator.Send(new GetVehicleTransformationsQuery(vehicleId));
        return Ok(result);
    }
}

// Request DTOs
public record TransformImageRequest
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public string SourceImageUrl { get; init; } = string.Empty;
    public TransformationType TransformationType { get; init; } = TransformationType.BackgroundRemoval;
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Studio;
    public string? CustomBackgroundId { get; init; }
}

public record TransformBatchImagesRequest
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public List<string> SourceImageUrls { get; init; } = new();
    public TransformationType TransformationType { get; init; } = TransformationType.BackgroundRemoval;
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Studio;
    public string? CustomBackgroundId { get; init; }
}
