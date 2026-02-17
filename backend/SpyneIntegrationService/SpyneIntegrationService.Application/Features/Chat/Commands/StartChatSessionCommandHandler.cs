using MediatR;
using Microsoft.Extensions.Logging;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Chat.Commands;

/// <summary>
/// Handler para iniciar sesión de chat con Vini AI
/// ⚠️ FASE 4: Este handler está IMPLEMENTADO pero el endpoint NO debe ser 
/// consumido por el frontend en esta versión.
/// </summary>
public class StartChatSessionCommandHandler : IRequestHandler<StartChatSessionCommand, ChatSessionDto>
{
    private readonly IChatSessionRepository _repository;
    private readonly ILogger<StartChatSessionCommandHandler> _logger;

    public StartChatSessionCommandHandler(
        IChatSessionRepository repository,
        ILogger<StartChatSessionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ChatSessionDto> Handle(StartChatSessionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[FASE 4 - NOT IN USE] Starting chat session for vehicle {VehicleId}", request.VehicleId);

        var session = new ChatSession
        {
            VehicleId = request.VehicleId,
            DealerId = request.DealerId,
            UserId = request.UserId,
            SessionIdentifier = request.SessionIdentifier ?? Guid.NewGuid().ToString(),
            Language = request.Language,
            Status = ChatSessionStatus.Active
        };

        // Add welcome message
        var welcomeMessage = new ChatMessage
        {
            ChatSessionId = session.Id,
            Role = ChatMessageRole.Assistant,
            Content = GetDefaultWelcomeMessage(request.Language)
        };
        session.Messages.Add(welcomeMessage);

        await _repository.AddAsync(session, cancellationToken);

        _logger.LogInformation("[FASE 4] Chat session {SessionId} created", session.Id);

        return MapToDto(session);
    }

    private static string GetDefaultWelcomeMessage(string language)
    {
        return language.ToLower() switch
        {
            "en" => "Hello! I'm Vini, your virtual assistant. How can I help you today?",
            _ => "¡Hola! Soy Vini, tu asistente virtual. ¿En qué puedo ayudarte hoy?"
        };
    }

    private static ChatSessionDto MapToDto(ChatSession s) => new()
    {
        Id = s.Id,
        VehicleId = s.VehicleId,
        DealerId = s.DealerId,
        UserId = s.UserId,
        SessionIdentifier = s.SessionIdentifier,
        SpyneChatId = s.SpyneChatId,
        Language = s.Language,
        Status = s.Status,
        IsQualifiedLead = s.IsQualifiedLead,
        UserRating = s.UserRating,
        MessageCount = s.Messages.Count,
        LastMessageAt = s.Messages.LastOrDefault()?.Timestamp,
        CreatedAt = s.CreatedAt,
        StartedAt = s.StartedAt,
        ClosedAt = s.ClosedAt,
        LastActivityAt = s.LastActivityAt,
        Duration = s.GetDuration(),
        Messages = s.Messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            ChatSessionId = m.ChatSessionId,
            Role = m.Role,
            Content = m.Content,
            Timestamp = m.Timestamp,
            CreatedAt = m.Timestamp
        }).ToList()
    };
}
