using Microsoft.EntityFrameworkCore;
using SchedulerService.Application.Interfaces;
using SchedulerService.Domain.Entities;
using SchedulerService.Infrastructure.Data;

namespace SchedulerService.Infrastructure.Repositories;

public class JobExecutionRepository : IJobExecutionRepository
{
    private readonly SchedulerDbContext _context;

    public JobExecutionRepository(SchedulerDbContext context)
    {
        _context = context;
    }

    public async Task<JobExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.JobExecutions
            .Include(e => e.Job)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<JobExecution>> GetByJobIdAsync(Guid jobId, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        return await _context.JobExecutions
            .Where(e => e.JobId == jobId)
            .OrderByDescending(e => e.ScheduledAt)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<JobExecution>> GetRecentExecutionsAsync(int pageSize = 100, CancellationToken cancellationToken = default)
    {
        return await _context.JobExecutions
            .Include(e => e.Job)
            .OrderByDescending(e => e.ScheduledAt)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<JobExecution> CreateAsync(JobExecution execution, CancellationToken cancellationToken = default)
    {
        _context.JobExecutions.Add(execution);
        await _context.SaveChangesAsync(cancellationToken);
        return execution;
    }

    public async Task<JobExecution> UpdateAsync(JobExecution execution, CancellationToken cancellationToken = default)
    {
        _context.JobExecutions.Update(execution);
        await _context.SaveChangesAsync(cancellationToken);
        return execution;
    }
}
