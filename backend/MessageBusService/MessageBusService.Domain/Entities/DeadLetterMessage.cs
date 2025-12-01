namespace MessageBusService.Domain.Entities;

public class DeadLetterMessage
{
    public Guid Id { get; set; }
    public Guid OriginalMessageId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string FailureReason { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public DateTime FailedAt { get; set; }
    public DateTime? RetriedAt { get; set; }
    public bool IsDiscarded { get; set; }
    public string? StackTrace { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}
