using Microsoft.AspNetCore.SignalR;
using MediatR;
using ChatbotService.Application.Features.Conversations.Commands;
using ChatbotService.Application.Features.Conversations.Queries;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time chat communication
/// </summary>
public class ChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IMediator mediator, ILogger<ChatHub> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a conversation room for real-time updates
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        _logger.LogInformation("Client {ConnectionId} joined conversation {ConversationId}", Context.ConnectionId, conversationId);
    }

    /// <summary>
    /// Leave a conversation room
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        _logger.LogInformation("Client {ConnectionId} left conversation {ConversationId}", Context.ConnectionId, conversationId);
    }

    /// <summary>
    /// Start a new conversation
    /// </summary>
    public async Task<ConversationDto> StartConversation(CreateConversationDto request)
    {
        var command = new CreateConversationCommand(
            request.UserId,
            request.SessionId ?? Context.ConnectionId,
            request.VehicleId,
            request.UserEmail,
            request.UserName,
            request.UserPhone,
            request.VehicleContext
        );

        var conversation = await _mediator.Send(command);
        await Groups.AddToGroupAsync(Context.ConnectionId, conversation.Id.ToString());

        return conversation;
    }

    /// <summary>
    /// Send a message and receive AI response
    /// </summary>
    public async Task<ChatbotResponseDto> SendMessage(SendMessageDto request)
    {
        // Notify typing indicator
        await Clients.Group(request.ConversationId.ToString())
            .SendAsync("TypingIndicator", new TypingIndicatorDto(request.ConversationId, true));

        try
        {
            var command = new SendMessageCommand(
                request.ConversationId,
                request.Content,
                request.VehicleContext
            );

            var response = await _mediator.Send(command);

            // Stop typing indicator
            await Clients.Group(request.ConversationId.ToString())
                .SendAsync("TypingIndicator", new TypingIndicatorDto(request.ConversationId, false));

            // Broadcast new messages to all clients in the conversation
            await Clients.Group(request.ConversationId.ToString())
                .SendAsync("NewMessage", response);

            // If should handoff to agent, notify
            if (response.ShouldHandoff)
            {
                await Clients.Group(request.ConversationId.ToString())
                    .SendAsync("TransferToAgent", new { ConversationId = request.ConversationId, Reason = response.HandoffReason });
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message for conversation {ConversationId}", request.ConversationId);

            // Stop typing indicator on error
            await Clients.Group(request.ConversationId.ToString())
                .SendAsync("TypingIndicator", new TypingIndicatorDto(request.ConversationId, false));

            throw;
        }
    }

    /// <summary>
    /// End the current conversation
    /// </summary>
    public async Task<ConversationDto> EndConversation(Guid conversationId, string reason)
    {
        var command = new EndConversationCommand(conversationId, reason);
        var conversation = await _mediator.Send(command);

        await Clients.Group(conversationId.ToString())
            .SendAsync("ConversationEnded", new { ConversationId = conversationId, Reason = reason });

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());

        return conversation;
    }

    /// <summary>
    /// Get conversation history
    /// </summary>
    public async Task<ConversationDto?> GetConversation(Guid conversationId)
    {
        var query = new GetConversationQuery(conversationId);
        return await _mediator.Send(query);
    }
}
