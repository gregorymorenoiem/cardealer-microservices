using MediatR;
using Microsoft.AspNetCore.Mvc;
using FeatureStoreService.Application.DTOs;
using FeatureStoreService.Application.Features.Commands;
using FeatureStoreService.Application.Features.Queries;

namespace FeatureStoreService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeaturesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FeaturesController> _logger;

    public FeaturesController(IMediator mediator, ILogger<FeaturesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todas las features de un usuario
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<ActionResult<List<UserFeatureDto>>> GetUserFeatures(Guid userId)
    {
        var query = new GetUserFeaturesQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Crear o actualizar feature de usuario
    /// </summary>
    [HttpPost("users")]
    public async Task<ActionResult<UserFeatureDto>> UpsertUserFeature([FromBody] UpsertUserFeatureRequest request)
    {
        var command = new UpsertUserFeatureCommand(
            request.UserId,
            request.FeatureName,
            request.FeatureValue,
            request.FeatureType,
            request.Version,
            request.ExpiresAt
        );
        
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Obtener todas las features de un vehículo
    /// </summary>
    [HttpGet("vehicles/{vehicleId}")]
    public async Task<ActionResult<List<VehicleFeatureDto>>> GetVehicleFeatures(Guid vehicleId)
    {
        var query = new GetVehicleFeaturesQuery(vehicleId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Crear o actualizar feature de vehículo
    /// </summary>
    [HttpPost("vehicles")]
    public async Task<ActionResult<VehicleFeatureDto>> UpsertVehicleFeature([FromBody] UpsertVehicleFeatureRequest request)
    {
        var command = new UpsertVehicleFeatureCommand(
            request.VehicleId,
            request.FeatureName,
            request.FeatureValue,
            request.FeatureType,
            request.Version,
            request.ExpiresAt
        );
        
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Obtener todas las definiciones de features
    /// </summary>
    [HttpGet("definitions")]
    public async Task<ActionResult<List<FeatureDefinitionDto>>> GetFeatureDefinitions([FromQuery] string? category = null)
    {
        var query = new GetFeatureDefinitionsQuery(category);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("/health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", service = "FeatureStoreService", timestamp = DateTime.UtcNow });
    }
}
