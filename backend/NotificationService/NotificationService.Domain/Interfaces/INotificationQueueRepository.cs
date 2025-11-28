using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Interfaces.Repositories;

public interface INotificationQueueRepository
{
    Task<NotificationQueue?> GetNextPendingAsync();
    Task<IEnumerable<NotificationQueue>> GetPendingAsync();
    Task<IEnumerable<NotificationQueue>> GetRetryQueueAsync();
    Task AddAsync(NotificationQueue queue);
    Task AddRangeAsync(IEnumerable<NotificationQueue> queues);
    Task UpdateAsync(NotificationQueue queue);
    Task UpdateRangeAsync(IEnumerable<NotificationQueue> queues);
    Task<int> GetPendingCountAsync();
    Task<int> CleanupProcessedAsync(DateTime cutoffDate);
}