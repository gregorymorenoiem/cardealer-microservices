using System.Collections.Concurrent;
using AuthService.Domain.Interfaces;
using AuthService.Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Messaging;

/// <summary>
/// In-memory implementation of Dead Letter Queue.
/// Thread-safe using ConcurrentDictionary.
/// </summary>
public class InMemoryDeadLetterQueue : IDeadLetterQueue
{
    private readonly ConcurrentDictionary<Guid, FailedEvent> _events = new();
    private readonly ILogger<InMemoryDeadLetterQueue> _logger;
    private const int MaxRetries = 5;

    public InMemoryDeadLetterQueue(ILogger<InMemoryDeadLetterQueue> logger)
    {
        _logger = logger;
    }

    public void Enqueue(FailedEvent failedEvent)
    {
        if (_events.TryAdd(failedEvent.Id, failedEvent))
        {
            _logger.LogInformation(
                "📮 Event {EventId} ({EventType}) enqueued to DLQ. Next retry at {NextRetryAt}",
                failedEvent.Id, failedEvent.EventType, failedEvent.NextRetryAt);
        }
        else
        {
            _logger.LogWarning(
                "⚠️ Event {EventId} already exists in DLQ",
                failedEvent.Id);
        }
    }

    public IEnumerable<FailedEvent> GetEventsReadyForRetry()
    {
        var now = DateTime.UtcNow;
        return _events.Values
            .Where(e => e.RetryCount < MaxRetries && e.NextRetryAt <= now)
            .ToList();
    }

    public void Remove(Guid eventId)
    {
        if (_events.TryRemove(eventId, out var removedEvent))
        {
            _logger.LogInformation(
                "✅ Event {EventId} ({EventType}) successfully published and removed from DLQ after {RetryCount} retries",
                eventId, removedEvent.EventType, removedEvent.RetryCount);
        }
    }

    public void MarkAsFailed(Guid eventId, string error)
    {
        if (_events.TryGetValue(eventId, out var failedEvent))
        {
            failedEvent.LastError = error;
            failedEvent.ScheduleNextRetry();

            if (failedEvent.RetryCount >= MaxRetries)
            {
                _logger.LogError(
                    "❌ Event {EventId} ({EventType}) exceeded max retries ({MaxRetries}). Keeping in DLQ for manual review. Last error: {Error}",
                    eventId, failedEvent.EventType, MaxRetries, error);
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ Event {EventId} ({EventType}) failed retry attempt {RetryCount}/{MaxRetries}. Next retry at {NextRetryAt}. Error: {Error}",
                    eventId, failedEvent.EventType, failedEvent.RetryCount, MaxRetries, failedEvent.NextRetryAt, error);
            }
        }
    }

    public (int TotalEvents, int ReadyForRetry, int MaxRetries) GetStats()
    {
        var totalEvents = _events.Count;
        var readyForRetry = GetEventsReadyForRetry().Count();
        var maxRetriesReached = _events.Values.Count(e => e.RetryCount >= MaxRetries);

        return (totalEvents, readyForRetry, maxRetriesReached);
    }
}
