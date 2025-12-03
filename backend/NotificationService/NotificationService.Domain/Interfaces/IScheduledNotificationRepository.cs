using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Interfaces.Repositories;

public interface IScheduledNotificationRepository
{
    // Basic CRUD
    Task<ScheduledNotification?> GetByIdAsync(Guid id);
    Task<IEnumerable<ScheduledNotification>> GetAllAsync();
    Task<IEnumerable<ScheduledNotification>> GetByStatusAsync(ScheduledNotificationStatus status);
    Task AddAsync(ScheduledNotification scheduledNotification);
    Task AddRangeAsync(IEnumerable<ScheduledNotification> scheduledNotifications);
    Task UpdateAsync(ScheduledNotification scheduledNotification);
    Task DeleteAsync(Guid id);

    // Scheduling queries
    Task<IEnumerable<ScheduledNotification>> GetDueNotificationsAsync();
    Task<IEnumerable<ScheduledNotification>> GetDueNotificationsAsync(DateTime upTo);
    Task<IEnumerable<ScheduledNotification>> GetScheduledForDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<ScheduledNotification>> GetRecurringNotificationsAsync();
    Task<IEnumerable<ScheduledNotification>> GetByNotificationIdAsync(Guid notificationId);

    // Status queries
    Task<IEnumerable<ScheduledNotification>> GetPendingAsync();
    Task<IEnumerable<ScheduledNotification>> GetFailedAsync();
    Task<IEnumerable<ScheduledNotification>> GetExecutedAsync(DateTime since);

    // Pagination
    Task<IEnumerable<ScheduledNotification>> GetPagedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalCountAsync();
    Task<int> GetCountByStatusAsync(ScheduledNotificationStatus status);

    // Cleanup
    Task<int> CleanupCompletedAsync(DateTime cutoffDate);
    Task<int> CleanupCancelledAsync(DateTime cutoffDate);
}
