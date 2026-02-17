namespace LoggingService.Domain;

public class LogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string? RequestId { get; set; }
    public string? TraceId { get; set; }
    public string? SpanId { get; set; }
    public string? UserId { get; set; }
    public string? Exception { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();

    public bool IsCritical() => Level == LogLevel.Critical;

    public bool IsError() => Level == LogLevel.Error || Level == LogLevel.Critical;

    public bool HasException() => !string.IsNullOrEmpty(Exception);

    public bool HasCorrelationId() => !string.IsNullOrEmpty(RequestId) || !string.IsNullOrEmpty(TraceId);

    public TimeSpan GetAge() => DateTime.UtcNow - Timestamp;
}
