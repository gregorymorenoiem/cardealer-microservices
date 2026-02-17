using MediatR;
using SpyneIntegrationService.Application.DTOs;

namespace SpyneIntegrationService.Application.Features.Chat.Queries;

/// <summary>
/// Query to get chat history for a vehicle
/// 
/// ⚠️ FASE 4: Este endpoint está IMPLEMENTADO pero NO debe ser consumido 
/// por el frontend en esta versión.
/// </summary>
public record GetVehicleChatHistoryQuery : IRequest<List<ChatSessionSummaryDto>>
{
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
