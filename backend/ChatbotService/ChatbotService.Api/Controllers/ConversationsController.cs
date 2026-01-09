using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ChatbotService.Application.DTOs;
using ChatbotService.Application.Features.Conversations.Commands;
using ChatbotService.Application.Features.Conversations.Queries;

namespace ChatbotService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ConversationsController> _logger;

    public ConversationsController(IMediator mediator, ILogger<ConversationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Inicia una nueva conversación con el chatbot
    /// </summary>
    [HttpPost("start")]
    [Authorize]
    public async Task<ActionResult<ConversationDto>> StartConversation([FromBody] StartConversationRequest request)
    {
        var command = new StartConversationCommand(
            request.UserId,
            request.UserName,
            request.UserEmail,
            request.UserPhone,
            request.VehicleId,
            null, // VehicleTitle - would be fetched from VehiclesSaleService
            null, // VehiclePrice
            null, // DealerId
            null, // DealerName
            null  // DealerWhatsApp
        );

        var conversation = await _mediator.Send(command);
        return Ok(conversation);
    }

    /// <summary>
    /// Envía mensaje del usuario y recibe respuesta del chatbot
    /// </summary>
    [HttpPost("message")]
    [Authorize]
    public async Task<ActionResult<ChatbotResponseDto>> SendMessage([FromBody] SendMessageRequest request)
    {
        var command = new SendMessageCommand(request.ConversationId, request.Message);
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Transfiere conversación a WhatsApp del dealer
    /// </summary>
    [HttpPost("{conversationId}/handoff")]
    [Authorize]
    public async Task<ActionResult<WhatsAppHandoffDto>> HandoffToWhatsApp(
        Guid conversationId,
        [FromBody] HandoffToWhatsAppRequest request)
    {
        var command = new HandoffToWhatsAppCommand(conversationId, request.CustomMessage);
        var handoff = await _mediator.Send(command);
        return Ok(handoff);
    }

    /// <summary>
    /// Obtiene detalles de una conversación
    /// </summary>
    [HttpGet("{conversationId}")]
    [Authorize]
    public async Task<ActionResult<ConversationDto>> GetConversation(Guid conversationId)
    {
        var query = new GetConversationQuery(conversationId);
        var conversation = await _mediator.Send(query);
        
        if (conversation == null)
            return NotFound();

        return Ok(conversation);
    }

    /// <summary>
    /// Obtiene mensajes de una conversación
    /// </summary>
    [HttpGet("{conversationId}/messages")]
    [Authorize]
    public async Task<ActionResult<List<MessageDto>>> GetMessages(Guid conversationId)
    {
        var query = new GetConversationMessagesQuery(conversationId);
        var messages = await _mediator.Send(query);
        return Ok(messages);
    }

    /// <summary>
    /// Obtiene conversaciones de un usuario
    /// </summary>
    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<ActionResult<List<ConversationDto>>> GetUserConversations(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetUserConversationsQuery(userId, page, pageSize);
        var conversations = await _mediator.Send(query);
        return Ok(conversations);
    }

    /// <summary>
    /// Obtiene conversaciones de un dealer (admin/dealer)
    /// </summary>
    [HttpGet("dealer/{dealerId}")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<ActionResult<List<ConversationDto>>> GetDealerConversations(
        Guid dealerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetDealerConversationsQuery(dealerId, page, pageSize);
        var conversations = await _mediator.Send(query);
        return Ok(conversations);
    }

    /// <summary>
    /// Obtiene leads HOT para seguimiento (admin/dealer)
    /// </summary>
    [HttpGet("hot-leads")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<ActionResult<List<ConversationDto>>> GetHotLeads(
        [FromQuery] int minScore = 85,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetHotLeadsQuery(minScore, page, pageSize);
        var leads = await _mediator.Send(query);
        return Ok(leads);
    }

    /// <summary>
    /// Obtiene estadísticas del chatbot (admin/dealer)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Dealer")]
    public async Task<ActionResult<ChatbotStatsDto>> GetStatistics(
        [FromQuery] Guid? dealerId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetChatbotStatisticsQuery(dealerId, startDate, endDate);
        var stats = await _mediator.Send(query);
        return Ok(stats);
    }
}
