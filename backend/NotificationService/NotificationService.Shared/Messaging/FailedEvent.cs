namespace NotificationService.Shared.Messaging;

public class FailedEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string EventJson { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; } = 0;
    public DateTime NextRetryAt { get; set; } = DateTime.UtcNow;
    public string? LastError { get; set; }

    public void ScheduleNextRetry()
    {
        RetryCount++;
        var delayMinutes = Math.Pow(2, RetryCount - 1);
        NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
    }

    public bool HasExceededMaxRetries(int maxRetries) => RetryCount >= maxRetries;
}
