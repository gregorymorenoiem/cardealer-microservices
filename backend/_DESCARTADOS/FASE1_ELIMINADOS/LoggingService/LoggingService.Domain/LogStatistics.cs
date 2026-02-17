namespace LoggingService.Domain;

public class LogStatistics
{
    public int TotalLogs { get; set; }
    public int TraceCount { get; set; }
    public int DebugCount { get; set; }
    public int InformationCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
    public int CriticalCount { get; set; }
    public Dictionary<string, int> LogsByService { get; set; } = new();
    public DateTime? OldestLog { get; set; }
    public DateTime? NewestLog { get; set; }

    public double GetErrorRate()
    {
        if (TotalLogs == 0) return 0;
        return (double)(ErrorCount + CriticalCount) / TotalLogs * 100;
    }

    public string GetMostActiveService()
    {
        if (LogsByService.Count == 0) return "None";
        return LogsByService.OrderByDescending(x => x.Value).First().Key;
    }

    public TimeSpan? GetLogSpan()
    {
        if (!OldestLog.HasValue || !NewestLog.HasValue) return null;
        return NewestLog.Value - OldestLog.Value;
    }
}
