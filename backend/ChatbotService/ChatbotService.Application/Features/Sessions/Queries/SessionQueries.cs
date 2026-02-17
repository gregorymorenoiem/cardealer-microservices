using MediatR;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Sessions.Queries;

/// <summary>
/// Query para obtener sesión por token
/// </summary>
public record GetSessionByTokenQuery(string SessionToken) : IRequest<SessionDto?>;

/// <summary>
/// Query para obtener historial de mensajes de una sesión
/// </summary>
public record GetSessionMessagesQuery(string SessionToken) : IRequest<List<MessageDto>>;

/// <summary>
/// Query para obtener sesiones de un usuario
/// </summary>
public record GetUserSessionsQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<List<SessionDto>>;

/// <summary>
/// Query para obtener estadísticas del chatbot
/// </summary>
public record GetChatbotStatsQuery(Guid ConfigurationId) : IRequest<ChatbotStatsDto>;

/// <summary>
/// Query para obtener uso de interacciones
/// </summary>
public record GetInteractionUsageQuery(Guid ConfigurationId) : IRequest<InteractionUsageDto>;

/// <summary>
/// Query para obtener reporte de salud del chatbot
/// </summary>
public record GetChatbotHealthQuery(Guid ConfigurationId) : IRequest<ChatbotHealthDto>;

/// <summary>
/// Query para obtener resumen mensual
/// </summary>
public record GetMonthlySummaryQuery(Guid ConfigurationId, int Year, int Month) : IRequest<MonthlySummaryDto?>;

/// <summary>
/// Resultado paginado
/// </summary>
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
