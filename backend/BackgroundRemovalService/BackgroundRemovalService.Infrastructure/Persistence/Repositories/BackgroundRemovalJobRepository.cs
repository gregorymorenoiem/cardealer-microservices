using BackgroundRemovalService.Domain.Entities;
using BackgroundRemovalService.Domain.Enums;
using BackgroundRemovalService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackgroundRemovalService.Infrastructure.Persistence.Repositories;

public class BackgroundRemovalJobRepository : IBackgroundRemovalJobRepository
{
    private readonly BackgroundRemovalDbContext _context;

    public BackgroundRemovalJobRepository(BackgroundRemovalDbContext context)
    {
        _context = context;
    }

    public async Task<BackgroundRemovalJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BackgroundRemovalJobs
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<BackgroundRemovalJob?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        return await _context.BackgroundRemovalJobs
            .FirstOrDefaultAsync(j => j.CorrelationId == correlationId, cancellationToken);
    }

    public async Task<IEnumerable<BackgroundRemovalJob>> GetByUserIdAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        return await _context.BackgroundRemovalJobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BackgroundRemovalJob>> GetPendingJobsAsync(
        int limit = 100, 
        CancellationToken cancellationToken = default)
    {
        return await _context.BackgroundRemovalJobs
            .Where(j => j.Status == ProcessingStatus.Pending || j.Status == ProcessingStatus.Retrying)
            .OrderByDescending(j => j.Priority)
            .ThenBy(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BackgroundRemovalJob>> GetJobsByStatusAsync(
        ProcessingStatus status, 
        int limit = 100, 
        CancellationToken cancellationToken = default)
    {
        return await _context.BackgroundRemovalJobs
            .Where(j => j.Status == status)
            .OrderByDescending(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<BackgroundRemovalJob> CreateAsync(
        BackgroundRemovalJob job, 
        CancellationToken cancellationToken = default)
    {
        _context.BackgroundRemovalJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task<BackgroundRemovalJob> UpdateAsync(
        BackgroundRemovalJob job, 
        CancellationToken cancellationToken = default)
    {
        job.UpdatedAt = DateTime.UtcNow;
        _context.BackgroundRemovalJobs.Update(job);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await GetByIdAsync(id, cancellationToken);
        if (job != null)
        {
            _context.BackgroundRemovalJobs.Remove(job);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetCountByStatusAsync(ProcessingStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.BackgroundRemovalJobs
            .CountAsync(j => j.Status == status, cancellationToken);
    }

    public async Task<IEnumerable<BackgroundRemovalJob>> GetExpiredJobsAsync(
        DateTime expirationDate, 
        int limit = 100, 
        CancellationToken cancellationToken = default)
    {
        return await _context.BackgroundRemovalJobs
            .Where(j => j.ExpiresAt != null && j.ExpiresAt < expirationDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
