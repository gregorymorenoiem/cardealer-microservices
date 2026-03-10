using AdminService.Domain.Interfaces;
using CarDealer.Contracts.Abstractions;
using Microsoft.Extensions.Logging;

namespace AdminService.Infrastructure.Messaging;

// ═══════════════════════════════════════════════════════════════════════════════
// NO-OP EVENT PUBLISHER — Fallback when RabbitMQ is disabled
// Follows the AuthService/ErrorService pattern.
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class NoOpEventPublisher : IEventPublisher
{
    private readonly ILogger<NoOpEventPublisher> _logger;

    public NoOpEventPublisher(ILogger<NoOpEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        _logger.LogDebug("[NoOp-Admin] Would publish event {EventType} with ID {EventId}",
            @event.EventType, @event.EventId);
        return Task.CompletedTask;
    }
}
