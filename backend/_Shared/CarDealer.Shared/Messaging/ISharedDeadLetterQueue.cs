namespace CarDealer.Shared.Messaging;

/// <summary>
/// Shared Dead Letter Queue interface for all microservices.
/// Replaces per-service InMemoryDeadLetterQueue implementations
/// that lose data on pod restart during auto-scaling.
/// </summary>
public interface ISharedDeadLetterQueue
{
    /// <summary>
    /// Enqueues a failed event for retry with exponential backoff.
    /// </summary>
    Task EnqueueAsync(DeadLetterEvent failedEvent, CancellationToken ct = default);

    /// <summary>
    /// Gets events that are ready for retry (NextRetryAt <= now and RetryCount < MaxRetries).
    /// </summary>
    Task<IReadOnlyList<DeadLetterEvent>> GetEventsReadyForRetryAsync(CancellationToken ct = default);

    /// <summary>
    /// Removes an event after successful reprocessing.
    /// </summary>
    Task RemoveAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    /// Marks an event as failed and schedules next retry with exponential backoff.
    /// </summary>
    Task MarkAsFailedAsync(Guid eventId, string error, CancellationToken ct = default);

    /// <summary>
    /// Gets statistics about the DLQ.
    /// </summary>
    Task<(int TotalEvents, int ReadyForRetry, int MaxRetriesReached)> GetStatsAsync(CancellationToken ct = default);
}
