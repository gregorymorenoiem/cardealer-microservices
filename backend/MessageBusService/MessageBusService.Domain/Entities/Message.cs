using MessageBusService.Domain.Enums;

namespace MessageBusService.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public MessageStatus Status { get; set; }
    public MessagePriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public string? ErrorMessage { get; set; }
    public string? CorrelationId { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}
