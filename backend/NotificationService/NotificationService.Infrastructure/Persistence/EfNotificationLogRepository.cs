using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces.Repositories;

namespace NotificationService.Infrastructure.Persistence;

public class EfNotificationLogRepository : INotificationLogRepository
{
    private readonly ApplicationDbContext _context;

    public EfNotificationLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NotificationLog>> GetByNotificationIdAsync(Guid notificationId)
    {
        return await _context.NotificationLogs
            .Where(l => l.NotificationId == notificationId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<NotificationLog>> GetByActionAsync(string action, DateTime startDate, DateTime endDate)
    {
        return await _context.NotificationLogs
            .Where(l => l.Action == action && l.Timestamp >= startDate && l.Timestamp <= endDate)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();
    }

    public async Task AddAsync(NotificationLog log)
    {
        await _context.NotificationLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<NotificationLog> logs)
    {
        await _context.NotificationLogs.AddRangeAsync(logs);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetLogCountByNotificationAsync(Guid notificationId)
    {
        return await _context.NotificationLogs
            .CountAsync(l => l.NotificationId == notificationId);
    }

    public async Task<int> CleanupOldLogsAsync(DateTime cutoffDate)
    {
        var oldLogs = await _context.NotificationLogs
            .Where(l => l.Timestamp < cutoffDate)
            .ToListAsync();

        _context.NotificationLogs.RemoveRange(oldLogs);
        return await _context.SaveChangesAsync();
    }
}