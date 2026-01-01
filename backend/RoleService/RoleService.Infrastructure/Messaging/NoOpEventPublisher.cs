using CarDealer.Contracts.Abstractions;
using RoleService.Domain.Interfaces;

namespace RoleService.Infrastructure.Messaging;

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
