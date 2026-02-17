using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Interfaces.Repositories;

namespace NotificationService.Infrastructure.Persistence;

public class EfNotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public EfNotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await _context.Notifications.FindAsync(id);
    }

    public async Task<IEnumerable<Notification>> GetByStatusAsync(NotificationStatus status)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.Status == status)
            .OrderByDescending(n => n.CreatedAt)
            .Take(500)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByRecipientAsync(string recipient)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.Recipient == recipient)
            .OrderByDescending(n => n.CreatedAt)
            .Take(500)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.Type == type)
            .OrderByDescending(n => n.CreatedAt)
            .Take(500)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByProviderAsync(NotificationProvider provider)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.Provider == provider)
            .OrderByDescending(n => n.CreatedAt)
            .Take(500)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.CreatedAt >= startDate && n.CreatedAt <= endDate)
            .OrderByDescending(n => n.CreatedAt)
            .Take(1000)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetFailedNotificationsAsync(DateTime since)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.Status == NotificationStatus.Failed && n.CreatedAt >= since)
            .OrderByDescending(n => n.CreatedAt)
            .Take(500)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetPendingNotificationsAsync()
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.Status == NotificationStatus.Pending)
            .OrderBy(n => n.CreatedAt)
            .Take(500)
            .ToListAsync();
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Notification> notifications)
    {
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<Notification> notifications)
    {
        _context.Notifications.UpdateRange(notifications);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var notification = await GetByIdAsync(id);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetCountByStatusAsync(NotificationStatus status)
    {
        return await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.Status == status);
    }

    public async Task<int> GetCountByRecipientAsync(string recipient)
    {
        return await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.Recipient == recipient);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Notifications
            .AsNoTracking()
            .AnyAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Notification>> GetNotificationsWithPaginationAsync(int pageNumber, int pageSize)
    {
        return await _context.Notifications
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Notifications.AsNoTracking().CountAsync();
    }

    public async Task<IEnumerable<Notification>> GetRecentNotificationsAsync(int count = 50)
    {
        return await _context.Notifications
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> CleanupOldNotificationsAsync(DateTime cutoffDate)
    {
        // Performance: Use ExecuteDeleteAsync instead of loading entities into memory
        return await _context.Notifications
            .Where(n => n.CreatedAt < cutoffDate)
            .ExecuteDeleteAsync();
    }
}