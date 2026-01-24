using MediatR;

namespace DealerAnalyticsService.Application.Features.Alerts.Commands;

/// <summary>
/// Command para marcar una alerta como leída
/// </summary>
public record MarkAlertAsReadCommand(
    Guid AlertId,
    Guid DealerId
) : IRequest<bool>;

/// <summary>
/// Command para marcar todas las alertas como leídas
/// </summary>
public record MarkAllAlertsAsReadCommand(
    Guid DealerId
) : IRequest<int>; // Returns count of alerts marked

/// <summary>
/// Command para descartar una alerta
/// </summary>
public record DismissAlertCommand(
    Guid AlertId,
    Guid DealerId
) : IRequest<bool>;

/// <summary>
/// Command para descartar todas las alertas de un tipo
/// </summary>
public record DismissAlertsByTypeCommand(
    Guid DealerId,
    string AlertType
) : IRequest<int>;

/// <summary>
/// Command para marcar una alerta como actuada
/// </summary>
public record MarkAlertAsActedUponCommand(
    Guid AlertId,
    Guid DealerId
) : IRequest<bool>;

/// <summary>
/// Command para crear una alerta manual (ej: desde admin)
/// </summary>
public record CreateAlertCommand(
    Guid DealerId,
    string Type,
    string Severity,
    string Title,
    string Message,
    string? ActionUrl = null,
    string? ActionLabel = null,
    int? ExpiresInDays = null
) : IRequest<Guid>;
