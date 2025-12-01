using AuthService.Shared.Messaging;

namespace AuthService.Domain.Interfaces;

/// <summary>
/// Interface for Dead Letter Queue to manage failed events.
/// Stores events that failed to publish to RabbitMQ and retries them with exponential backoff.
/// </summary>
public interface IDeadLetterQueue
{
    /// <summary>
    /// Enqueues a failed event for retry.
    /// </summary>
    /// <param name="failedEvent">The event that failed to publish.</param>
    void Enqueue(FailedEvent failedEvent);

    /// <summary>
    /// Gets all events that are ready for retry (NextRetryAt <= now).
    /// </summary>
    /// <returns>Collection of events ready for retry.</returns>
    IEnumerable<FailedEvent> GetEventsReadyForRetry();

    /// <summary>
    /// Removes an event from the queue (after successful publish).
    /// </summary>
    /// <param name="eventId">ID of the event to remove.</param>
    void Remove(Guid eventId);

    /// <summary>
    /// Marks an event as failed and schedules next retry.
    /// </summary>
    /// <param name="eventId">ID of the event.</param>
    /// <param name="error">Error message from the failed attempt.</param>
    void MarkAsFailed(Guid eventId, string error);

    /// <summary>
    /// Gets statistics about the DLQ.
    /// </summary>
    /// <returns>Tuple with (TotalEvents, ReadyForRetry, MaxRetries).</returns>
    (int TotalEvents, int ReadyForRetry, int MaxRetries) GetStats();
}
