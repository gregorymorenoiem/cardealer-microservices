using Microsoft.EntityFrameworkCore;
using IntegrationService.Domain.Entities;
using IntegrationService.Domain.Interfaces;
using IntegrationService.Infrastructure.Persistence;

namespace IntegrationService.Infrastructure.Repositories;

public class SyncJobRepository : ISyncJobRepository
{
    private readonly IntegrationDbContext _context;

    public SyncJobRepository(IntegrationDbContext context)
    {
        _context = context;
    }

    public async Task<SyncJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SyncJobs
            .Include(j => j.Integration)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<SyncJob>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SyncJobs.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SyncJob>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken cancellationToken = default)
    {
        return await _context.SyncJobs
            .Where(j => j.IntegrationId == integrationId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SyncJob>> GetByStatusAsync(SyncStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.SyncJobs.Where(j => j.Status == status).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SyncJob>> GetScheduledAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.SyncJobs
            .Where(j => j.Status == SyncStatus.Idle && j.ScheduledAt != null && j.ScheduledAt <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<SyncJob> AddAsync(SyncJob syncJob, CancellationToken cancellationToken = default)
    {
        await _context.SyncJobs.AddAsync(syncJob, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return syncJob;
    }

    public async Task UpdateAsync(SyncJob syncJob, CancellationToken cancellationToken = default)
    {
        _context.SyncJobs.Update(syncJob);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var syncJob = await GetByIdAsync(id, cancellationToken);
        if (syncJob != null)
        {
            _context.SyncJobs.Remove(syncJob);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SyncJobs.AnyAsync(j => j.Id == id, cancellationToken);
    }
}
