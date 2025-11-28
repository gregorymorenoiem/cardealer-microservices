namespace AuditService.Application.DTOs;

public class AuditStatsDto
{
    public int TotalLogs { get; set; }
    public int SuccessfulLogs { get; set; }
    public int FailedLogs { get; set; }
    public int SystemLogs { get; set; }
    public int UserLogs { get; set; }
    public int AnonymousLogs { get; set; }
    public double SuccessRate { get; set; }
    public DateTime? FirstLogDate { get; set; }
    public DateTime? LastLogDate { get; set; }

    // Statistics by categories
    public Dictionary<string, int> LogsBySeverity { get; set; } = new();
    public Dictionary<string, int> LogsByService { get; set; } = new();
    public Dictionary<string, int> LogsByAction { get; set; } = new();
    public Dictionary<string, int> LogsByResource { get; set; } = new();

    // Time-based statistics
    public Dictionary<DateTime, int> DailyCounts { get; set; } = new();
    public Dictionary<string, int> HourlyAverages { get; set; } = new();

    // Performance metrics
    public double AverageDurationMs { get; set; }
    public long MaxDurationMs { get; set; }
    public long MinDurationMs { get; set; }

    // Top lists
    public List<ActionFrequencyDto> TopActions { get; set; } = new();
    public List<UserActivityDto> TopUsers { get; set; } = new();
    public List<ServiceActivityDto> TopServices { get; set; } = new();

    // Error analysis
    public Dictionary<string, int> CommonErrors { get; set; } = new();
    public int TotalErrorsLast24h { get; set; }
    public double ErrorRateTrend { get; set; }
}

public class ActionFrequencyDto
{
    public string Action { get; set; } = string.Empty;
    public int Count { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double SuccessRate => Count > 0 ? (SuccessCount * 100.0) / Count : 0;
    public double AverageDurationMs { get; set; }
}

public class UserActivityDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public int TotalActions { get; set; }
    public int SuccessfulActions { get; set; }
    public int FailedActions { get; set; }
    public double SuccessRate => TotalActions > 0 ? (SuccessfulActions * 100.0) / TotalActions : 0;
    public DateTime FirstActivity { get; set; }
    public DateTime LastActivity { get; set; }
    public List<string> MostFrequentActions { get; set; } = new();
    public double AverageActionsPerDay { get; set; }
}

public class ServiceActivityDto
{
    public string ServiceName { get; set; } = string.Empty;
    public int TotalLogs { get; set; }
    public int ErrorCount { get; set; }
    public double ErrorRate => TotalLogs > 0 ? (ErrorCount * 100.0) / TotalLogs : 0;
    public double AverageDurationMs { get; set; }
    public DateTime LastActivity { get; set; }
}