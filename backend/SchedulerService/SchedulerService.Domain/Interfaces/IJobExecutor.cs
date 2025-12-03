using SchedulerService.Domain.Entities;
using SchedulerService.Domain.Models;

namespace SchedulerService.Domain.Interfaces;

/// <summary>
/// Interface for job executor implementations
/// </summary>
public interface IJobExecutor
{
    /// <summary>
    /// Gets the executor type identifier
    /// </summary>
    string ExecutorType { get; }

    /// <summary>
    /// Determines if this executor can handle the given job
    /// </summary>
    bool CanExecute(Job job);

    /// <summary>
    /// Executes the job with the provided context
    /// </summary>
    Task<ExecutionResult> ExecuteAsync(Job job, JobExecution execution, CancellationToken cancellationToken = default);
}
