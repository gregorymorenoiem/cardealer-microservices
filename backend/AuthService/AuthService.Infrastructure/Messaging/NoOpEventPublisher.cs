using CarDealer.Contracts.Abstractions;
using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Messaging;

/// <summary>
/// No-operation implementation of IEventPublisher for when RabbitMQ is disabled.
/// </summary>
public class NoOpEventPublisher : IEventPublisher
{
    private readonly ILogger<NoOpEventPublisher> _logger;

    public NoOpEventPublisher(ILogger<NoOpEventPublisher> logger)
    {
        _logger = logger;
        _logger.LogInformation("NoOpEventPublisher initialized - RabbitMQ is disabled");
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        _logger.LogDebug("NoOp: Would publish event {EventType} with ID {EventId}",
            @event.EventType, @event.EventId);
        return Task.CompletedTask;
    }
}
