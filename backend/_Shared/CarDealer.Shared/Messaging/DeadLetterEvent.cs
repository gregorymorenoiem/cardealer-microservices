namespace CarDealer.Shared.Messaging;

/// <summary>
/// Represents a failed event stored in the persistent Dead Letter Queue.
/// Shared model across all microservices — replaces per-service FailedEvent duplicates.
/// </summary>
public class DeadLetterEvent
{
    /// <summary>Unique identifier for this DLQ entry.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Name of the originating service (e.g., "AuthService", "MediaService").</summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>Event type / routing key (e.g., "auth.user.registered", "media.uploaded").</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>JSON-serialized event payload.</summary>
    public string EventJson { get; set; } = string.Empty;

    /// <summary>When the event first failed to publish.</summary>
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Number of retry attempts made so far.</summary>
    public int RetryCount { get; set; }

    /// <summary>Maximum retries allowed before giving up.</summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>Scheduled time for next retry attempt.</summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>Last error message from the most recent failed attempt.</summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Schedules the next retry with exponential backoff.
    /// Pattern: 1min, 2min, 4min, 8min, 16min (capped).
    /// </summary>
    public void ScheduleNextRetry()
    {
        RetryCount++;
        var delayMinutes = Math.Min(Math.Pow(2, RetryCount - 1), 16);
        NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
    }
}
