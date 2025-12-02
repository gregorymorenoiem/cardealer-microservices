using TracingService.Domain.Enums;

namespace TracingService.Domain.Entities;

/// <summary>
/// Represents a single span in a distributed trace
/// A span is a named, timed operation that represents a piece of the workflow
/// </summary>
public class Span
{
    /// <summary>
    /// Unique identifier for this span (16-byte hex string)
    /// </summary>
    public string SpanId { get; set; } = string.Empty;
    
    /// <summary>
    /// Identifier of the trace this span belongs to (16-byte hex string)
    /// </summary>
    public string TraceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Parent span ID (null for root spans)
    /// </summary>
    public string? ParentSpanId { get; set; }
    
    /// <summary>
    /// Name of the span (e.g., "GET /api/users", "DatabaseQuery")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Kind of span (Client, Server, Producer, Consumer, Internal)
    /// </summary>
    public SpanKind Kind { get; set; }
    
    /// <summary>
    /// Status of the span (Unset, Ok, Error)
    /// </summary>
    public SpanStatus Status { get; set; }
    
    /// <summary>
    /// Status message (error message if Status is Error)
    /// </summary>
    public string? StatusMessage { get; set; }
    
    /// <summary>
    /// Start time of the span
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// End time of the span
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Duration of the span in milliseconds
    /// </summary>
    public double? DurationMs
    {
        get
        {
            if (EndTime.HasValue)
            {
                return (EndTime.Value - StartTime).TotalMilliseconds;
            }
            return null;
        }
    }
    
    /// <summary>
    /// Service name that generated this span
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
    
    /// <summary>
    /// HTTP method (if applicable)
    /// </summary>
    public string? HttpMethod { get; set; }
    
    /// <summary>
    /// HTTP URL (if applicable)
    /// </summary>
    public string? HttpUrl { get; set; }
    
    /// <summary>
    /// HTTP status code (if applicable)
    /// </summary>
    public int? HttpStatusCode { get; set; }
    
    /// <summary>
    /// Tags/attributes associated with this span (key-value pairs)
    /// </summary>
    public Dictionary<string, string> Tags { get; set; } = new();
    
    /// <summary>
    /// Events that occurred during the span (e.g., exceptions, logs)
    /// </summary>
    public List<SpanEvent> Events { get; set; } = new();
    
    /// <summary>
    /// Whether this span has an error
    /// </summary>
    public bool HasError => Status == SpanStatus.Error;
}
