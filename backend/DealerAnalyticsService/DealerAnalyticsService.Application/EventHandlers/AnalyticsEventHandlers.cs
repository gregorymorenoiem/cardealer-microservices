using DealerAnalyticsService.Application.Services;
using DealerAnalyticsService.Domain.Events;
using DealerAnalyticsService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DealerAnalyticsService.Application.EventHandlers;

/// <summary>
/// Handles DealerSnapshotCreatedNotification and publishes to RabbitMQ
/// </summary>
public class DealerSnapshotCreatedHandler : INotificationHandler<DealerSnapshotCreatedNotification>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<DealerSnapshotCreatedHandler> _logger;

    public DealerSnapshotCreatedHandler(
        IEventPublisher eventPublisher,
        ILogger<DealerSnapshotCreatedHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(DealerSnapshotCreatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling DealerSnapshotCreated for dealer {DealerId}, snapshot {SnapshotId}",
            notification.DealerId,
            notification.SnapshotId);

        var @event = new DealerSnapshotCreatedEvent
        {
            DealerId = notification.DealerId,
            SnapshotId = notification.SnapshotId,
            Date = DateOnly.FromDateTime(notification.Date)
        };

        await _eventPublisher.PublishAsync(@event, cancellationToken);
    }
}

/// <summary>
/// Handles DealerAlertTriggeredNotification and publishes to RabbitMQ
/// </summary>
public class DealerAlertTriggeredHandler : INotificationHandler<DealerAlertTriggeredNotification>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<DealerAlertTriggeredHandler> _logger;

    public DealerAlertTriggeredHandler(
        IEventPublisher eventPublisher,
        ILogger<DealerAlertTriggeredHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(DealerAlertTriggeredNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling DealerAlertTriggered for dealer {DealerId}, alert type {AlertType}",
            notification.DealerId,
            notification.AlertType);

        var @event = new DealerAlertTriggeredEvent
        {
            DealerId = notification.DealerId,
            AlertId = notification.AlertId,
            AlertType = notification.AlertType.ToString(),
            Severity = notification.Severity.ToString(),
            RequiresNotification = notification.Severity >= Domain.Enums.AlertSeverity.Warning
        };

        await _eventPublisher.PublishAsync(@event, cancellationToken);
    }
}
