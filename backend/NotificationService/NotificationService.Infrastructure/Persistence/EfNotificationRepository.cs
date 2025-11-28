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
            .Where(n => n.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByRecipientAsync(string recipient)
    {
        return await _context.Notifications
            .Where(n => n.Recipient == recipient)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type)
    {
        return await _context.Notifications
            .Where(n => n.Type == type)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByProviderAsync(NotificationProvider provider)
    {
        return await _context.Notifications
            .Where(n => n.Provider == provider)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Notifications
            .Where(n => n.CreatedAt >= startDate && n.CreatedAt <= endDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetFailedNotificationsAsync(DateTime since)
    {
        return await _context.Notifications
            .Where(n => n.Status == NotificationStatus.Failed && n.CreatedAt >= since)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetPendingNotificationsAsync()
    {
        return await _context.Notifications
            .Where(n => n.Status == NotificationStatus.Pending)
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
            .CountAsync(n => n.Status == status);
    }

    public async Task<int> GetCountByRecipientAsync(string recipient)
    {
        return await _context.Notifications
            .CountAsync(n => n.Recipient == recipient);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Notifications
            .AnyAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Notification>> GetNotificationsWithPaginationAsync(int pageNumber, int pageSize)
    {
        return await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Notifications.CountAsync();
    }

    public async Task<IEnumerable<Notification>> GetRecentNotificationsAsync(int count = 50)
    {
        return await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> CleanupOldNotificationsAsync(DateTime cutoffDate)
    {
        var oldNotifications = await _context.Notifications
            .Where(n => n.CreatedAt < cutoffDate)
            .ToListAsync();

        _context.Notifications.RemoveRange(oldNotifications);
        return await _context.SaveChangesAsync();
    }
}