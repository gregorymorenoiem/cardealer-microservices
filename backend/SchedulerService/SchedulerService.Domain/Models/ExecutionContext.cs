namespace SchedulerService.Domain.Models;

/// <summary>
/// Context information for job execution
/// </summary>
public class ExecutionContext
{
    public Guid JobId { get; set; }
    public Guid ExecutionId { get; set; }
    public string JobType { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
    public int AttemptNumber { get; set; }
    public int MaxRetries { get; set; }
    public int TimeoutSeconds { get; set; }
    public CancellationToken CancellationToken { get; set; }
}
