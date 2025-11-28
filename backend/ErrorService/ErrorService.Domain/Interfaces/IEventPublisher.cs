using CarDealer.Contracts.Abstractions;

namespace ErrorService.Domain.Interfaces;

/// <summary>
/// Interface for publishing events to message broker (RabbitMQ).
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to the message broker.
    /// </summary>
    /// <typeparam name="TEvent">Type of event to publish</typeparam>
    /// <param name="event">Event instance to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : IEvent;
}
