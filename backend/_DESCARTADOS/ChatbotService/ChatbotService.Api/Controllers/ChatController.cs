using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ChatbotService.Application.Features.Conversations.Commands;
using ChatbotService.Application.Features.Conversations.Queries;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    /// Create a new conversation (alternative to SignalR for REST clients)
    /// </summary>
    [HttpPost("conversations")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ConversationDto>> CreateConversation([FromBody] CreateConversationDto request)
    {
        var command = new CreateConversationCommand(
            request.UserId,
            request.SessionId,
            request.VehicleId,
            request.UserEmail,
            request.UserName,
            request.UserPhone,
            request.VehicleContext
        );

        var conversation = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
    }

    /// <summary>
    /// Get a conversation by ID
    /// </summary>
    [HttpGet("conversations/{id:guid}")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationDto>> GetConversation(Guid id)
    {
        var query = new GetConversationQuery(id);
        var conversation = await _mediator.Send(query);

        if (conversation == null)
            return NotFound();

        return Ok(conversation);
    }

    /// <summary>
    /// Get conversations for a user
    /// </summary>
    [HttpGet("conversations/user/{userId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(List<ConversationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ConversationSummaryDto>>> GetUserConversations(
        Guid userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var query = new GetUserConversationsQuery(userId, skip, take);
        var conversations = await _mediator.Send(query);
        return Ok(conversations);
    }

    /// <summary>
    /// Send a message (alternative to SignalR for REST clients)
    /// </summary>
    [HttpPost("conversations/{conversationId:guid}/messages")]
    [ProducesResponseType(typeof(ChatbotResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatbotResponseDto>> SendMessage(
        Guid conversationId,
        [FromBody] ChatSendMessageRequest request)
    {
        try
        {
            var command = new SendMessageCommand(conversationId, request.Content, request.VehicleContext);
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// End a conversation
    /// </summary>
    [HttpPost("conversations/{conversationId:guid}/end")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationDto>> EndConversation(
        Guid conversationId,
        [FromBody] EndConversationRequest request)
    {
        try
        {
            var command = new EndConversationCommand(conversationId, request.Reason);
            var conversation = await _mediator.Send(command);
            return Ok(conversation);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get chat analytics
    /// </summary>
    [HttpGet("analytics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ChatAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChatAnalyticsDto>> GetAnalytics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var startDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = to ?? DateTime.UtcNow;
        var query = new GetChatAnalyticsQuery(startDate, endDate);
        var analytics = await _mediator.Send(query);
        return Ok(analytics);
    }
}

// Request DTOs
public record ChatSendMessageRequest(string Content, VehicleContextDto? VehicleContext);
public record EndConversationRequest(string Reason);
