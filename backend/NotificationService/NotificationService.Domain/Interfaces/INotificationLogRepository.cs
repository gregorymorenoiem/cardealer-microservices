using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Interfaces.Repositories;

public interface INotificationLogRepository
{
    Task<IEnumerable<NotificationLog>> GetByNotificationIdAsync(Guid notificationId);
    Task<IEnumerable<NotificationLog>> GetByActionAsync(string action, DateTime startDate, DateTime endDate);
    Task AddAsync(NotificationLog log);
    Task AddRangeAsync(IEnumerable<NotificationLog> logs);
    Task<int> GetLogCountByNotificationAsync(Guid notificationId);
    Task<int> CleanupOldLogsAsync(DateTime cutoffDate);
}