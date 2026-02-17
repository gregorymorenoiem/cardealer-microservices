using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Application.Features.Spins.Commands;
using SpyneIntegrationService.Application.Features.Spins.Queries;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SpinsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SpinsController> _logger;

    public SpinsController(IMediator mediator, ILogger<SpinsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Generate a 360° interactive spin view
    /// </summary>
    /// <remarks>
    /// Requires 8-72 images taken at equal intervals around the vehicle.
    /// Recommended: 36 images (10° increments) for smooth rotation.
    /// Processing time: 3-10 minutes depending on image count.
    /// </remarks>
    [HttpPost("generate")]
    [Authorize]
    [ProducesResponseType(typeof(GenerateSpinResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateSpin([FromBody] GenerateSpinRequest request)
    {
        var command = new GenerateSpinCommand
        {
            VehicleId = request.VehicleId,
            DealerId = request.DealerId,
            ImageUrls = request.ImageUrls,
            BackgroundPreset = request.BackgroundPreset,
            CustomBackgroundId = request.CustomBackgroundId
        };

        var result = await _mediator.Send(command);
        
        _logger.LogInformation("Spin generation started for vehicle {VehicleId} with {Count} images", 
            request.VehicleId, request.ImageUrls.Count);
        
        return AcceptedAtAction(nameof(GetSpinStatus), new { id = result.SpinId }, result);
    }

    /// <summary>
    /// Get spin generation status by ID
    /// </summary>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(SpinGenerationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSpinStatus(Guid id)
    {
        var result = await _mediator.Send(new GetSpinStatusQuery(id));
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get spin for a vehicle
    /// </summary>
    [HttpGet("vehicle/{vehicleId:guid}")]
    [ProducesResponseType(typeof(SpinGenerationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVehicleSpin(Guid vehicleId)
    {
        var result = await _mediator.Send(new GetVehicleSpinQuery(vehicleId));
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }
}

// Request DTOs
public record GenerateSpinRequest
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Studio;
    public string? CustomBackgroundId { get; init; }
}
