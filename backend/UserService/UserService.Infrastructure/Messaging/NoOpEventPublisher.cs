using CarDealer.Contracts.Abstractions;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Messaging;

/// <summary>
/// No-op implementation of IEventPublisher for development without RabbitMQ.
/// </summary>
public class NoOpEventPublisher : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        // Log that event was "published" (no-op)
        Console.WriteLine($"[NoOpEventPublisher] Would publish event: {typeof(TEvent).Name}");
        return Task.CompletedTask;
    }
}
