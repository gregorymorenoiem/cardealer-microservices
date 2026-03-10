using CarDealer.Contracts.Abstractions;

namespace ContactService.Domain.Interfaces;

/// <summary>
/// Publishes domain events to the message broker (RabbitMQ).
/// Each microservice defines its own IEventPublisher following DDD boundaries.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent;
}
