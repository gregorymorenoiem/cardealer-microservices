using SchedulerService.Domain.Enums;

namespace SchedulerService.Domain.Entities;

/// <summary>
/// Represents a scheduled job configuration
/// </summary>
public class Job
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty; // Fully qualified type name
    public string CronExpression { get; set; } = string.Empty;
    public JobStatus Status { get; set; } = JobStatus.Enabled;
    public int RetryCount { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 300; // 5 minutes default
    public Dictionary<string, string> Parameters { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastExecutionAt { get; set; }
    public DateTime? NextExecutionAt { get; set; }

    // Navigation property
    public List<JobExecution> Executions { get; set; } = new();

    public void Enable()
    {
        Status = JobStatus.Enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        Status = JobStatus.Disabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pause()
    {
        Status = JobStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastExecution(DateTime executionTime)
    {
        LastExecutionAt = executionTime;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNextExecution(DateTime? nextTime)
    {
        NextExecutionAt = nextTime;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExecutable() => Status == JobStatus.Enabled;
}
