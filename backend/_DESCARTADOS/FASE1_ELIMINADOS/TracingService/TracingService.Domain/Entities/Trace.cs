namespace TracingService.Domain.Entities;

/// <summary>
/// Represents a complete trace (collection of related spans)
/// A trace shows the end-to-end journey of a request through multiple services
/// </summary>
public class Trace
{
    /// <summary>
    /// Unique identifier for this trace (16-byte hex string)
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Root span of the trace (entry point)
    /// </summary>
    public Span? RootSpan { get; set; }

    /// <summary>
    /// All spans belonging to this trace
    /// </summary>
    public List<Span> Spans { get; set; } = new();

    /// <summary>
    /// Start time of the trace (start time of root span)
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time of the trace (latest end time among all spans)
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Total duration of the trace in milliseconds
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
    /// Number of spans in the trace
    /// </summary>
    public int SpanCount => Spans.Count;

    /// <summary>
    /// Number of services involved in the trace
    /// </summary>
    public int ServiceCount => Spans.Select(s => s.ServiceName).Distinct().Count();

    /// <summary>
    /// List of all services involved in the trace
    /// </summary>
    public List<string> ServicesInvolved => Spans.Select(s => s.ServiceName).Distinct().ToList();

    /// <summary>
    /// Whether any span in the trace has an error
    /// </summary>
    public bool HasError => Spans.Any(s => s.HasError);

    /// <summary>
    /// Number of errors in the trace
    /// </summary>
    public int ErrorCount => Spans.Count(s => s.HasError);
}
