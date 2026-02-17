namespace TracingService.Domain.Entities;

/// <summary>
/// Represents an event that occurred during a span (e.g., exception, log)
/// </summary>
public class SpanEvent
{
    /// <summary>
    /// Name of the event
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Attributes associated with the event
    /// </summary>
    public Dictionary<string, string> Attributes { get; set; } = new();
}
