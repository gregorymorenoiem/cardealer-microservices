using Microsoft.EntityFrameworkCore;
using SchedulerService.Application.Interfaces;
using SchedulerService.Domain.Entities;
using SchedulerService.Domain.Enums;
using SchedulerService.Infrastructure.Data;

namespace SchedulerService.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly SchedulerDbContext _context;

    public JobRepository(SchedulerDbContext context)
    {
        _context = context;
    }

    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Include(j => j.Executions.OrderByDescending(e => e.ScheduledAt).Take(10))
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<List<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .OrderBy(j => j.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Job>> GetActiveJobsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Where(j => j.Status == JobStatus.Enabled)
            .OrderBy(j => j.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default)
    {
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task<Job> UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _context.Jobs.FindAsync(new object[] { id }, cancellationToken);
        if (job != null)
        {
            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
