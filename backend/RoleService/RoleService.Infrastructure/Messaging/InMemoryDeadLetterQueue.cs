using System.Collections.Concurrent;

namespace RoleService.Infrastructure.Messaging;

/// <summary>
/// Implementación en memoria de Dead Letter Queue para eventos fallidos de RabbitMQ
/// Thread-safe usando ConcurrentDictionary
/// </summary>
public class InMemoryDeadLetterQueue : IDeadLetterQueue
{
    private readonly ConcurrentDictionary<Guid, FailedEvent> _events = new();
    private readonly int _maxRetries;

    public InMemoryDeadLetterQueue(int maxRetries = 5)
    {
        _maxRetries = maxRetries;
    }

    public void Enqueue(FailedEvent failedEvent)
    {
        _events.TryAdd(failedEvent.Id, failedEvent);
    }

    public IEnumerable<FailedEvent> GetEventsReadyForRetry()
    {
        var now = DateTime.UtcNow;
        return _events.Values
            .Where(e => e.RetryCount < _maxRetries)
            .Where(e => e.NextRetryAt == null || e.NextRetryAt <= now)
            .OrderBy(e => e.FailedAt)
            .ToList();
    }

    public void Remove(Guid eventId)
    {
        _events.TryRemove(eventId, out _);
    }

    public void MarkAsFailed(Guid eventId, string error)
    {
        if (_events.TryGetValue(eventId, out var failedEvent))
        {
            failedEvent.LastError = error;

            if (failedEvent.RetryCount >= _maxRetries)
            {
                // Max retries alcanzado, marcar como definitivamente fallido
                // En producción, podrías mover esto a persistent storage o dead-letter exchange
                failedEvent.NextRetryAt = null;
            }
            else
            {
                failedEvent.ScheduleNextRetry();
            }
        }
    }

    public (int TotalEvents, int ReadyForRetry, int MaxRetries) GetStats()
    {
        var now = DateTime.UtcNow;
        var totalEvents = _events.Count;
        var readyForRetry = _events.Values.Count(e =>
            e.RetryCount < _maxRetries &&
            (e.NextRetryAt == null || e.NextRetryAt <= now));
        var maxRetriesReached = _events.Values.Count(e => e.RetryCount >= _maxRetries);

        return (totalEvents, readyForRetry, maxRetriesReached);
    }
}
