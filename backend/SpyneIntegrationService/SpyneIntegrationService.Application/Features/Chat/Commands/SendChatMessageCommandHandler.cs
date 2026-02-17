using MediatR;
using Microsoft.Extensions.Logging;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Chat.Commands;

/// <summary>
/// Handler para enviar mensaje en sesión de chat
/// ⚠️ FASE 4: Este handler está IMPLEMENTADO pero el endpoint NO debe ser 
/// consumido por el frontend en esta versión.
/// </summary>
public class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, ChatMessageResponseDto>
{
    private readonly IChatSessionRepository _repository;
    private readonly ILogger<SendChatMessageCommandHandler> _logger;

    public SendChatMessageCommandHandler(
        IChatSessionRepository repository,
        ILogger<SendChatMessageCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ChatMessageResponseDto> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[FASE 4 - NOT IN USE] Processing chat message for session {SessionId}", request.SessionId);

        var session = await _repository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            throw new InvalidOperationException($"Chat session {request.SessionId} not found");
        }

        // Add user message
        var userMessage = new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = ChatMessageRole.User,
            Content = request.Message
        };
        session.AddMessage(userMessage);

        // Generate simple response (placeholder for Spyne AI integration)
        var assistantMessage = new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = ChatMessageRole.Assistant,
            Content = GenerateSimpleResponse(request.Message)
        };
        session.AddMessage(assistantMessage);

        await _repository.UpdateAsync(session, cancellationToken);

        return new ChatMessageResponseDto
        {
            SessionId = session.Id,
            UserMessage = MapMessageToDto(userMessage),
            AssistantMessage = MapMessageToDto(assistantMessage),
            AssistantResponse = MapMessageToDto(assistantMessage),
            LeadDetected = false,
            IsQualifiedLead = false,
            SuggestedFollowUps = new List<string>(),
            SuggestedActions = new List<string>()
        };
    }

    private static string GenerateSimpleResponse(string message)
    {
        // Simple placeholder response - will be replaced with actual Spyne AI integration
        return "Gracias por tu mensaje. Un representante se pondrá en contacto contigo pronto.";
    }

    private static ChatMessageDto MapMessageToDto(ChatMessage m) => new()
    {
        Id = m.Id,
        ChatSessionId = m.ChatSessionId,
        Role = m.Role,
        Content = m.Content,
        Timestamp = m.Timestamp,
        CreatedAt = m.Timestamp
    };
}
