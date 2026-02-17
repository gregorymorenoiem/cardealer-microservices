using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Application.Features.Chat.Commands;
using SpyneIntegrationService.Application.Features.Chat.Queries;

namespace SpyneIntegrationService.Api.Controllers;

/// <summary>
/// Chat AI Controller - Vini Integration
/// 
/// ⚠️ FASE 4: Este controlador está COMPLETAMENTE IMPLEMENTADO pero 
/// NO debe ser consumido por el frontend en esta versión.
/// 
/// Está preparado para futuras iteraciones cuando se active el chat AI.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IMediator mediator, ILogger<ChatController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Start a new chat session with Vini AI
    /// </summary>
    /// <remarks>
    /// ⚠️ FASE 4 - NOT FOR FRONTEND CONSUMPTION
    /// 
    /// Initializes an AI-powered chat session for a specific vehicle.
    /// The AI will greet the user and be ready to answer questions about the vehicle.
    /// </remarks>
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(ChatSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartChatSession([FromBody] StartChatSessionRequest request)
    {
        _logger.LogInformation("[FASE 4 - NOT IN USE] Starting chat session for vehicle {VehicleId}", request.VehicleId);

        var command = new StartChatSessionCommand
        {
            VehicleId = request.VehicleId,
            DealerId = request.DealerId,
            UserId = request.UserId,
            SessionIdentifier = request.SessionIdentifier,
            Language = request.Language,
            VehicleContext = request.VehicleContext != null ? new VehicleContextDto
            {
                Make = request.VehicleContext.Make,
                Model = request.VehicleContext.Model,
                Year = request.VehicleContext.Year ?? 0,
                Price = request.VehicleContext.Price ?? 0,
                Currency = request.VehicleContext.Currency,
                Mileage = request.VehicleContext.Mileage,
                Transmission = request.VehicleContext.Transmission,
                FuelType = request.VehicleContext.FuelType,
                Color = request.VehicleContext.Color,
                Features = request.VehicleContext.Features ?? new()
            } : null
        };

        var result = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetChatSession), new { sessionId = result.Id }, result);
    }

    /// <summary>
    /// Send a message in an active chat session
    /// </summary>
    /// <remarks>
    /// ⚠️ FASE 4 - NOT FOR FRONTEND CONSUMPTION
    /// 
    /// Sends user message and receives AI response.
    /// The AI will analyze the conversation for lead qualification.
    /// </remarks>
    [HttpPost("sessions/{sessionId:guid}/messages")]
    [ProducesResponseType(typeof(ChatMessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessage(Guid sessionId, [FromBody] SendMessageRequest request)
    {
        _logger.LogDebug("[FASE 4 - NOT IN USE] Sending message to session {SessionId}", sessionId);

        var command = new SendChatMessageCommand
        {
            SessionId = sessionId,
            Message = request.Message,
            Metadata = request.Metadata
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// End a chat session
    /// </summary>
    /// <remarks>
    /// ⚠️ FASE 4 - NOT FOR FRONTEND CONSUMPTION
    /// 
    /// Closes the chat session and optionally records user feedback.
    /// </remarks>
    [HttpPost("sessions/{sessionId:guid}/end")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EndChatSession(Guid sessionId, [FromBody] EndChatSessionRequest? request = null)
    {
        _logger.LogInformation("[FASE 4 - NOT IN USE] Ending chat session {SessionId}", sessionId);

        var command = new EndChatSessionCommand
        {
            SessionId = sessionId,
            EndReason = request?.EndReason,
            UserRating = request?.UserRating,
            UserFeedback = request?.UserFeedback
        };

        var success = await _mediator.Send(command);
        
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Get a chat session with full message history
    /// </summary>
    /// <remarks>
    /// ⚠️ FASE 4 - NOT FOR FRONTEND CONSUMPTION
    /// </remarks>
    [HttpGet("sessions/{sessionId:guid}")]
    [ProducesResponseType(typeof(ChatSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChatSession(Guid sessionId)
    {
        var result = await _mediator.Send(new GetChatSessionQuery(sessionId));
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get chat history for a vehicle (dealer view)
    /// </summary>
    /// <remarks>
    /// ⚠️ FASE 4 - NOT FOR FRONTEND CONSUMPTION
    /// 
    /// Returns all chat sessions associated with a vehicle, useful for dealers to see customer interactions.
    /// </remarks>
    [HttpGet("vehicle/{vehicleId:guid}/history")]
    [Authorize]
    [ProducesResponseType(typeof(List<ChatSessionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehicleChatHistory(Guid vehicleId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetVehicleChatHistoryQuery
        {
            VehicleId = vehicleId,
            PageNumber = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

// Request DTOs
public record StartChatSessionRequest
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public Guid? UserId { get; init; }
    public string? SessionIdentifier { get; init; }
    public string Language { get; init; } = "es";
    public VehicleContextRequest? VehicleContext { get; init; }
}

public record VehicleContextRequest
{
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int? Year { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; } = "DOP";
    public int? Mileage { get; init; }
    public string? Transmission { get; init; }
    public string? FuelType { get; init; }
    public string? Color { get; init; }
    public List<string>? Features { get; init; }
}

public record SendMessageRequest
{
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; init; }
}

public record EndChatSessionRequest
{
    public string? EndReason { get; init; }
    public int? UserRating { get; init; }
    public string? UserFeedback { get; init; }
}
