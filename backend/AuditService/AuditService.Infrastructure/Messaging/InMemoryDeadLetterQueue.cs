using System.Collections.Concurrent;
using AuditService.Domain.Interfaces;
using AuditService.Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace AuditService.Infrastructure.Messaging;

/// <summary>
/// Implementación en memoria de Dead Letter Queue
/// Thread-safe usando ConcurrentDictionary
/// </summary>
public class InMemoryDeadLetterQueue : IDeadLetterQueue
{
    private readonly ConcurrentDictionary<Guid, FailedEvent> _failedEvents = new();
    private readonly ILogger<InMemoryDeadLetterQueue> _logger;
    private readonly int _maxRetries;

    public InMemoryDeadLetterQueue(ILogger<InMemoryDeadLetterQueue> logger, int maxRetries = 5)
    {
        _logger = logger;
        _maxRetries = maxRetries;
    }

    public Task Enqueue(string eventType, string eventJson, string error)
    {
        var failedEvent = new FailedEvent
        {
            EventType = eventType,
            EventJson = eventJson,
            LastError = error,
            FailedAt = DateTime.UtcNow,
            RetryCount = 0
        };

        failedEvent.ScheduleNextRetry();

        _failedEvents.TryAdd(failedEvent.Id, failedEvent);

        _logger.LogWarning(
            "📬 Evento encolado en DLQ: {EventType} | NextRetry: {NextRetry} | Error: {Error}",
            eventType, failedEvent.NextRetryAt, error);

        return Task.CompletedTask;
    }

    public Task<List<FailedEvent>> GetEventsReadyForRetry()
    {
        var now = DateTime.UtcNow;
        var readyEvents = _failedEvents.Values
            .Where(e => e.NextRetryAt <= now && !e.HasExceededMaxRetries(_maxRetries))
            .ToList();

        return Task.FromResult(readyEvents);
    }

    public Task Remove(Guid eventId)
    {
        if (_failedEvents.TryRemove(eventId, out var removedEvent))
        {
            _logger.LogInformation(
                "✅ Evento removido de DLQ (éxito): {EventType} | Reintentos: {RetryCount}",
                removedEvent.EventType, removedEvent.RetryCount);
        }

        return Task.CompletedTask;
    }

    public Task MarkAsFailed(Guid eventId, string error)
    {
        if (_failedEvents.TryGetValue(eventId, out var failedEvent))
        {
            failedEvent.LastError = error;
            failedEvent.ScheduleNextRetry();

            if (failedEvent.HasExceededMaxRetries(_maxRetries))
            {
                _logger.LogError(
                    "❌ Evento alcanzó máximo de reintentos ({MaxRetries}): {EventType} | ID: {EventId}",
                    _maxRetries, failedEvent.EventType, eventId);
            }
            else
            {
                _logger.LogWarning(
                    "🔄 Evento fallido nuevamente: {EventType} | Reintento: {RetryCount}/{MaxRetries} | NextRetry: {NextRetry}",
                    failedEvent.EventType, failedEvent.RetryCount, _maxRetries, failedEvent.NextRetryAt);
            }
        }

        return Task.CompletedTask;
    }

    public Task<(int Total, int ReadyForRetry, int Exhausted)> GetStats()
    {
        var now = DateTime.UtcNow;
        var total = _failedEvents.Count;
        var readyForRetry = _failedEvents.Values.Count(e => e.NextRetryAt <= now && !e.HasExceededMaxRetries(_maxRetries));
        var exhausted = _failedEvents.Values.Count(e => e.HasExceededMaxRetries(_maxRetries));

        return Task.FromResult((total, readyForRetry, exhausted));
    }
}
