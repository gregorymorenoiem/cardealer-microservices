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
}
