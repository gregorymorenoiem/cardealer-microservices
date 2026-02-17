namespace BackupDRService.Core.Models;

/// <summary>
/// Health metrics for backup scheduler monitoring
/// </summary>
public class SchedulerHealthMetrics
{
    public DateTime CheckTime { get; set; } = DateTime.UtcNow;
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public SchedulerStats Stats { get; set; } = new();
    public List<ActiveSchedule> ActiveSchedules { get; set; } = new();
    public List<string> Issues { get; set; } = new();
}

public class SchedulerStats
{
    public int TotalSchedules { get; set; }
    public int EnabledSchedules { get; set; }
    public int DisabledSchedules { get; set; }
    public int SchedulesDueNext24Hours { get; set; }
    public int BackupsExecutedToday { get; set; }
    public int FailedBackupsToday { get; set; }
    public double SuccessRateToday { get; set; }
    public DateTime? LastBackupTime { get; set; }
    public DateTime? NextScheduledBackup { get; set; }
}

public class ActiveSchedule
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public DateTime? NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string? LastRunStatus { get; set; }
    public TimeSpan? TimeUntilNextRun { get; set; }
}
