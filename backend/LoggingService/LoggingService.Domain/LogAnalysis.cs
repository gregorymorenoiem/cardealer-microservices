namespace LoggingService.Domain;

/// <summary>
/// Represents the result of log analysis
/// </summary>
public class LogAnalysis
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public LogStatistics Statistics { get; set; } = new();
    public List<LogPattern> DetectedPatterns { get; set; } = new();
    public List<LogAnomaly> DetectedAnomalies { get; set; } = new();
    public List<Recommendation> Recommendations { get; set; } = new();
    public Dictionary<string, ServiceHealthMetrics> ServiceHealth { get; set; } = new();
    public AnalysisSummary Summary { get; set; } = new();

    public bool HasCriticalIssues() =>
        DetectedAnomalies.Any(a => a.IsHighSeverity()) ||
        Statistics.GetErrorRate() > 10;

    public int GetTotalAnomalies() => DetectedAnomalies.Count;

    public int GetTotalPatterns() => DetectedPatterns.Count;

    public List<string> GetAffectedServices() =>
        DetectedAnomalies.Select(a => a.ServiceName)
            .Union(DetectedPatterns.SelectMany(p => p.AffectedServices))
            .Distinct()
            .ToList();
}

/// <summary>
/// Health metrics for a specific service
/// </summary>
public class ServiceHealthMetrics
{
    public string ServiceName { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public double ErrorRate { get; set; }
    public double AverageResponseTime { get; set; }
    public int RequestCount { get; set; }
    public int ErrorCount { get; set; }
    public DateTime LastHealthy { get; set; }
    public List<string> CurrentIssues { get; set; } = new();
    public double AvailabilityPercentage { get; set; }

    public bool IsHealthy() => Status == HealthStatus.Healthy;

    public bool IsCritical() => Status == HealthStatus.Critical;
}

/// <summary>
/// Health status of a service
/// </summary>
public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy,
    Critical,
    Unknown
}

/// <summary>
/// Summary of the analysis
/// </summary>
public class AnalysisSummary
{
    public int TotalLogsAnalyzed { get; set; }
    public int CriticalIssuesFound { get; set; }
    public int WarningsFound { get; set; }
    public int PatternsDetected { get; set; }
    public int AnomaliesDetected { get; set; }
    public double OverallSystemHealth { get; set; } // 0-100
    public List<string> TopIssues { get; set; } = new();
    public Dictionary<string, int> IssuesByCategory { get; set; } = new();
}

/// <summary>
/// Recommendation based on analysis
/// </summary>
public class Recommendation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationType Type { get; set; }
    public RecommendationPriority Priority { get; set; }
    public string? AffectedService { get; set; }
    public List<string> ActionItems { get; set; } = new();
    public string? DocumentationLink { get; set; }
}

/// <summary>
/// Types of recommendations
/// </summary>
public enum RecommendationType
{
    Performance,
    Scalability,
    Reliability,
    Security,
    Configuration,
    Monitoring,
    BestPractice
}

/// <summary>
/// Priority levels for recommendations
/// </summary>
public enum RecommendationPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
