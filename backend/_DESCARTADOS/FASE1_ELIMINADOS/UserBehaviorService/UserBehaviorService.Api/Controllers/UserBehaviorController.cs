using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserBehaviorService.Application.DTOs;
using UserBehaviorService.Application.Features.Commands;
using UserBehaviorService.Application.Features.Queries;

namespace UserBehaviorService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserBehaviorController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserBehaviorController> _logger;

    public UserBehaviorController(IMediator mediator, ILogger<UserBehaviorController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener perfil de comportamiento de un usuario
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserBehaviorProfileDto>> GetUserProfile(Guid userId)
    {
        var query = new GetUserBehaviorProfileQuery(userId);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound($"Usuario {userId} no tiene perfil de comportamiento");
        
        return Ok(result);
    }

    /// <summary>
    /// Obtener historial de acciones de un usuario
    /// </summary>
    [HttpGet("{userId}/actions")]
    public async Task<ActionResult<List<UserActionDto>>> GetUserActions(Guid userId, [FromQuery] int limit = 50)
    {
        var query = new GetUserActionsQuery(userId, limit);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Registrar una acción de usuario (track comportamiento)
    /// </summary>
    [HttpPost("actions")]
    public async Task<ActionResult<UserActionDto>> RecordAction([FromBody] RecordActionRequest request)
    {
        var command = new RecordUserActionCommand(
            request.UserId,
            request.ActionType,
            request.ActionDetails,
            request.RelatedVehicleId,
            request.SearchQuery,
            request.SessionId,
            request.DeviceType
        );
        
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetUserProfile), new { userId = result.UserId }, result);
    }

    /// <summary>
    /// Obtener resumen de comportamiento de todos los usuarios
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<UserBehaviorSummaryDto>> GetSummary()
    {
        var query = new GetBehaviorSummaryQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("/health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", service = "UserBehaviorService", timestamp = DateTime.UtcNow });
    }
}
