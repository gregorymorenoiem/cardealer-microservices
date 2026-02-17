using MediatR;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Chat.Queries;

/// <summary>
/// Handler para obtener historial de chat de un vehículo
/// ⚠️ FASE 4: Este handler está IMPLEMENTADO pero el endpoint NO debe ser 
/// consumido por el frontend en esta versión.
/// </summary>
public class GetVehicleChatHistoryQueryHandler : IRequestHandler<GetVehicleChatHistoryQuery, List<ChatSessionSummaryDto>>
{
    private readonly IChatSessionRepository _repository;

    public GetVehicleChatHistoryQueryHandler(IChatSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ChatSessionSummaryDto>> Handle(GetVehicleChatHistoryQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _repository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);
        
        return sessions.Select(MapToSummary).ToList();
    }

    private static ChatSessionSummaryDto MapToSummary(ChatSession s) => new()
    {
        Id = s.Id,
        SessionId = s.Id,
        VehicleId = s.VehicleId,
        UserId = s.UserId,
        DealerId = s.DealerId,
        Status = s.Status,
        MessageCount = s.Messages.Count,
        HasLead = s.LeadInfo != null,
        IsQualifiedLead = s.IsQualifiedLead,
        LeadScore = s.LeadInfo?.LeadScore,
        UserRating = s.UserRating,
        StartedAt = s.StartedAt,
        CreatedAt = s.CreatedAt,
        EndedAt = s.EndedAt,
        ClosedAt = s.ClosedAt,
        Duration = s.GetDuration(),
        LastMessagePreview = s.Messages.LastOrDefault()?.Content?.Substring(0, Math.Min(s.Messages.LastOrDefault()?.Content?.Length ?? 0, 100)),
        LeadInfo = s.LeadInfo != null ? new ChatLeadInfoDto
        {
            Id = s.LeadInfo.Id,
            Name = s.LeadInfo.Name,
            Email = s.LeadInfo.Email,
            Phone = s.LeadInfo.Phone,
            LeadScore = s.LeadInfo.LeadScore,
            QualityTier = s.LeadInfo.QualityTier
        } : null
    };
}
