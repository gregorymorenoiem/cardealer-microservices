using SchedulerService.Domain.Entities;

namespace SchedulerService.Application.Interfaces;

/// <summary>
/// Repository interface for JobExecution entity persistence
/// </summary>
public interface IJobExecutionRepository
{
    Task<JobExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<JobExecution>> GetByJobIdAsync(Guid jobId, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<List<JobExecution>> GetRecentExecutionsAsync(int pageSize = 100, CancellationToken cancellationToken = default);
    Task<JobExecution> CreateAsync(JobExecution execution, CancellationToken cancellationToken = default);
    Task<JobExecution> UpdateAsync(JobExecution execution, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete old executions before the cutoff date
    /// </summary>
    Task<int> DeleteOldExecutionsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get execution statistics for a date range
    /// </summary>
    Task<ExecutionStatistics> GetStatisticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics for job executions
/// </summary>
public record ExecutionStatistics(
    int TotalExecutions,
    int SuccessCount,
    int FailedCount,
    int CancelledCount,
    double AverageDurationMs,
    Dictionary<string, int> ExecutionsByJob);
