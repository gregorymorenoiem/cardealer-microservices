namespace TracingService.Domain.Entities;

/// <summary>
/// Statistics about distributed tracing
/// </summary>
public class TraceStatistics
{
    /// <summary>
    /// Total number of traces
    /// </summary>
    public int TotalTraces { get; set; }

    /// <summary>
    /// Total number of spans
    /// </summary>
    public int TotalSpans { get; set; }

    /// <summary>
    /// Number of traces with errors
    /// </summary>
    public int TracesWithErrors { get; set; }

    /// <summary>
    /// Average trace duration in milliseconds
    /// </summary>
    public double AverageDurationMs { get; set; }

    /// <summary>
    /// Median trace duration in milliseconds
    /// </summary>
    public double MedianDurationMs { get; set; }

    /// <summary>
    /// 95th percentile duration in milliseconds
    /// </summary>
    public double P95DurationMs { get; set; }

    /// <summary>
    /// 99th percentile duration in milliseconds
    /// </summary>
    public double P99DurationMs { get; set; }

    /// <summary>
    /// Slowest trace ID
    /// </summary>
    public string? SlowestTraceId { get; set; }

    /// <summary>
    /// Duration of the slowest trace in milliseconds
    /// </summary>
    public double? SlowestTraceDurationMs { get; set; }

    /// <summary>
    /// Most active service (service with most spans)
    /// </summary>
    public string? MostActiveService { get; set; }

    /// <summary>
    /// Number of spans for the most active service
    /// </summary>
    public int MostActiveServiceSpanCount { get; set; }

    /// <summary>
    /// Spans by service name
    /// </summary>
    public Dictionary<string, int> SpansByService { get; set; } = new();

    /// <summary>
    /// Errors by service name
    /// </summary>
    public Dictionary<string, int> ErrorsByService { get; set; } = new();

    /// <summary>
    /// Time range for these statistics
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time for these statistics
    /// </summary>
    public DateTime EndTime { get; set; }
}
