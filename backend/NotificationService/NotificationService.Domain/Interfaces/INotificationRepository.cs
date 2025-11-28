using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetByStatusAsync(NotificationStatus status);
    Task<IEnumerable<Notification>> GetByRecipientAsync(string recipient);
    Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type);
    Task<IEnumerable<Notification>> GetByProviderAsync(NotificationProvider provider);
    Task<IEnumerable<Notification>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Notification>> GetFailedNotificationsAsync(DateTime since);
    Task<IEnumerable<Notification>> GetPendingNotificationsAsync();
    Task AddAsync(Notification notification);
    Task AddRangeAsync(IEnumerable<Notification> notifications);
    Task UpdateAsync(Notification notification);
    Task UpdateRangeAsync(IEnumerable<Notification> notifications);
    Task DeleteAsync(Guid id);
    Task<int> GetCountByStatusAsync(NotificationStatus status);
    Task<int> GetCountByRecipientAsync(string recipient);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Notification>> GetNotificationsWithPaginationAsync(int pageNumber, int pageSize);
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<Notification>> GetRecentNotificationsAsync(int count = 50);
    Task<int> CleanupOldNotificationsAsync(DateTime cutoffDate);
}