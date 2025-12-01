using CarDealer.Contracts.Abstractions;

namespace VehicleService.Domain.Interfaces;

/// <summary>
/// Interface for publishing events to RabbitMQ message broker
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to the message broker
    /// </summary>
    /// <typeparam name="TEvent">The type of event to publish</typeparam>
    /// <param name="event">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent;
}
