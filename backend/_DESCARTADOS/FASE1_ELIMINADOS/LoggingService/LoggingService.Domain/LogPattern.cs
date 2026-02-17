namespace LoggingService.Domain;

/// <summary>
/// Represents a detected pattern in log entries
/// </summary>
public class LogPattern
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PatternType Type { get; set; }
    public int OccurrenceCount { get; set; }
    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public List<string> AffectedServices { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    public bool IsRecurring() => OccurrenceCount > 1;

    public TimeSpan GetPatternDuration() => LastSeen - FirstSeen;

    public double GetFrequencyPerHour()
    {
        var duration = GetPatternDuration();
        if (duration.TotalHours == 0) return OccurrenceCount;
        return OccurrenceCount / duration.TotalHours;
    }
}

/// <summary>
/// Types of patterns that can be detected
/// </summary>
public enum PatternType
{
    ErrorSpike,           // Sudden increase in error rate
    RecurringError,       // Same error appearing repeatedly
    PerformanceDegradation, // Slow response times
    ResourceExhaustion,   // Memory/CPU/Disk issues
    SecurityThreat,       // Potential security issues
    ConfigurationIssue,   // Configuration problems
    DependencyFailure,    // External service failures
    Custom               // User-defined patterns
}
