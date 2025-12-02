using SchedulerService.Domain.Entities;

namespace SchedulerService.Application.Interfaces;

/// <summary>
/// Repository interface for Job entity persistence
/// </summary>
public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Job>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Job>> GetActiveJobsAsync(CancellationToken cancellationToken = default);
    Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default);
    Task<Job> UpdateAsync(Job job, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
