using MediatR;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Chat.Queries;

/// <summary>
/// Handler para obtener sesión de chat
/// ⚠️ FASE 4: Este handler está IMPLEMENTADO pero el endpoint NO debe ser 
/// consumido por el frontend en esta versión.
/// </summary>
public class GetChatSessionQueryHandler : IRequestHandler<GetChatSessionQuery, ChatSessionDto?>
{
    private readonly IChatSessionRepository _repository;

    public GetChatSessionQueryHandler(IChatSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<ChatSessionDto?> Handle(GetChatSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(request.SessionId, cancellationToken);
        
        if (session == null)
            return null;

        return MapToDto(session);
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
        LeadInfo = s.LeadInfo != null ? new ChatLeadInfoDto
        {
            Id = s.LeadInfo.Id,
            Name = s.LeadInfo.Name,
            Email = s.LeadInfo.Email,
            Phone = s.LeadInfo.Phone,
            PreferredContactMethod = s.LeadInfo.PreferredContactMethod,
            Budget = s.LeadInfo.Budget,
            InterestType = s.LeadInfo.InterestType,
            LeadScore = s.LeadInfo.LeadScore,
            QualityTier = s.LeadInfo.QualityTier,
            CapturedAt = s.LeadInfo.CapturedAt
        } : null,
        Messages = s.Messages.OrderBy(m => m.Timestamp).Select(m => new ChatMessageDto
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
