namespace LoggingService.Domain;

/// <summary>
/// Represents an anomaly detected in log data
/// </summary>
public class LogAnomaly
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AnomalyType Type { get; set; }
    public AnomalySeverity Severity { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public string ServiceName { get; set; } = string.Empty;
    public string? AffectedComponent { get; set; }
    public double Confidence { get; set; } // 0-100
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> RelatedLogIds { get; set; } = new();
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public bool IsHighSeverity() => Severity >= AnomalySeverity.High;

    public bool IsStale(TimeSpan threshold) => DateTime.UtcNow - DetectedAt > threshold;

    public TimeSpan GetAge() => DateTime.UtcNow - DetectedAt;
}

/// <summary>
/// Types of anomalies that can be detected
/// </summary>
public enum AnomalyType
{
    ErrorRateSpike,        // Sudden increase in error rate
    ResponseTimeSpike,     // Sudden increase in response time
    TrafficSpike,          // Unusual traffic pattern
    UnusualErrorPattern,   // New or unusual errors
    ServiceUnavailable,    // Service not responding
    ResourceAnomaly,       // Unusual resource usage
    SecurityAnomaly,       // Potential security issue
    DataAnomaly           // Unusual data patterns
}

/// <summary>
/// Severity levels for anomalies
/// </summary>
public enum AnomalySeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
