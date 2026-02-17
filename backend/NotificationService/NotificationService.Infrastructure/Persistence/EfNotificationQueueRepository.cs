using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces.Repositories;

namespace NotificationService.Infrastructure.Persistence;

public class EfNotificationQueueRepository : INotificationQueueRepository
{
    private readonly ApplicationDbContext _context;

    public EfNotificationQueueRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationQueue?> GetNextPendingAsync()
    {
        return await _context.NotificationQueues
            .Include(q => q.Notification)
            .Where(q => q.Status == Domain.Enums.QueueStatus.Pending)
            .OrderBy(q => q.QueuedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<NotificationQueue>> GetPendingAsync()
    {
        return await _context.NotificationQueues
            .Include(q => q.Notification)
            .Where(q => q.Status == Domain.Enums.QueueStatus.Pending)
            .OrderBy(q => q.QueuedAt)
            .Take(100) // Process in batches to prevent OOM
            .ToListAsync();
    }

    public async Task<IEnumerable<NotificationQueue>> GetRetryQueueAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.NotificationQueues
            .Include(q => q.Notification)
            .Where(q => q.Status == Domain.Enums.QueueStatus.Retry && q.NextRetryAt <= now)
            .OrderBy(q => q.NextRetryAt)
            .Take(100) // Process in batches
            .ToListAsync();
    }

    public async Task AddAsync(NotificationQueue queue)
    {
        await _context.NotificationQueues.AddAsync(queue);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<NotificationQueue> queues)
    {
        await _context.NotificationQueues.AddRangeAsync(queues);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(NotificationQueue queue)
    {
        _context.NotificationQueues.Update(queue);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<NotificationQueue> queues)
    {
        _context.NotificationQueues.UpdateRange(queues);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _context.NotificationQueues
            .AsNoTracking()
            .CountAsync(q => q.Status == Domain.Enums.QueueStatus.Pending);
    }

    public async Task<int> CleanupProcessedAsync(DateTime cutoffDate)
    {
        // Performance: Use ExecuteDeleteAsync instead of loading entities into memory
        return await _context.NotificationQueues
            .Where(q => q.ProcessedAt.HasValue && q.ProcessedAt.Value < cutoffDate)
            .ExecuteDeleteAsync();
    }
}