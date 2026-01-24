using DealerAnalyticsService.Application.DTOs;
using MediatR;

namespace DealerAnalyticsService.Application.Features.Alerts.Queries;

/// <summary>
/// Query para obtener alertas activas de un dealer
/// </summary>
public record GetActiveAlertsQuery(
    Guid DealerId,
    bool IncludeRead = false,
    bool IncludeDismissed = false
) : IRequest<AlertsSummaryDto>;

/// <summary>
/// Query para obtener conteo de alertas sin leer
/// </summary>
public record GetUnreadAlertCountQuery(
    Guid DealerId
) : IRequest<int>;

/// <summary>
/// Query para obtener alertas por tipo
/// </summary>
public record GetAlertsByTypeQuery(
    Guid DealerId,
    string Type
) : IRequest<List<DealerAlertDto>>;

/// <summary>
/// Query para obtener alertas por severidad
/// </summary>
public record GetAlertsBySeverityQuery(
    Guid DealerId,
    string MinSeverity
) : IRequest<List<DealerAlertDto>>;
