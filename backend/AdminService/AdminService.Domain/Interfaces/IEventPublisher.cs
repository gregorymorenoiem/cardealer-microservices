using CarDealer.Contracts.Abstractions;

namespace AdminService.Domain.Interfaces;

// ═══════════════════════════════════════════════════════════════════════════════
// EVENT PUBLISHER INTERFACE — AdminService DDD boundary
//
// Follows the same pattern as AuthService, ContactService, ErrorService.
// Enables RabbitMQ publishing from AdminService (previously read-only).
// ═══════════════════════════════════════════════════════════════════════════════

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent;
}
