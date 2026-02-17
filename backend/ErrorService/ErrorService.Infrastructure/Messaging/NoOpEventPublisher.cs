using CarDealer.Contracts.Abstractions;
using ErrorService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ErrorService.Infrastructure.Messaging;

/// <summary>
/// No-operation implementation of IEventPublisher.
/// Used when RabbitMQ is disabled. Events are logged but not published.
/// </summary>
public class NoOpEventPublisher : IEventPublisher
{
    private readonly ILogger<NoOpEventPublisher> _logger;

    public NoOpEventPublisher(ILogger<NoOpEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        _logger.LogInformation(
            "NoOp: Event {EventType} would be published. RabbitMQ is disabled. EventId: {EventId}",
            @event.EventType,
            @event.EventId);

        return Task.CompletedTask;
    }
}
