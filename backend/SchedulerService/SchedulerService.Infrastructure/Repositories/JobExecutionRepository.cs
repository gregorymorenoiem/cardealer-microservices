using Microsoft.EntityFrameworkCore;
using SchedulerService.Application.Interfaces;
using SchedulerService.Domain.Entities;
using SchedulerService.Domain.Enums;
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

    public async Task<int> DeleteOldExecutionsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        var oldExecutions = await _context.JobExecutions
            .Where(e => e.CompletedAt.HasValue && e.CompletedAt.Value < cutoffDate)
            .ToListAsync(cancellationToken);

        if (oldExecutions.Count > 0)
        {
            _context.JobExecutions.RemoveRange(oldExecutions);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return oldExecutions.Count;
    }

    public async Task<ExecutionStatistics> GetStatisticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var executions = await _context.JobExecutions
            .Include(e => e.Job)
            .Where(e => e.ScheduledAt >= startDate && e.ScheduledAt <= endDate)
            .ToListAsync(cancellationToken);

        var successCount = executions.Count(e => e.Status == ExecutionStatus.Succeeded);
        var failedCount = executions.Count(e => e.Status == ExecutionStatus.Failed);
        var cancelledCount = executions.Count(e => e.Status == ExecutionStatus.Cancelled);
        var avgDuration = executions
            .Where(e => e.DurationMs.HasValue)
            .Select(e => e.DurationMs!.Value)
            .DefaultIfEmpty(0)
            .Average();

        var executionsByJob = executions
            .GroupBy(e => e.Job?.Name ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        return new ExecutionStatistics(
            TotalExecutions: executions.Count,
            SuccessCount: successCount,
            FailedCount: failedCount,
            CancelledCount: cancelledCount,
            AverageDurationMs: avgDuration,
            ExecutionsByJob: executionsByJob);
    }
}
