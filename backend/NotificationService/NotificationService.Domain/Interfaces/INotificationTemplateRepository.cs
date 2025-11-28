using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Interfaces.Repositories;

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> GetByIdAsync(Guid id);
    Task<NotificationTemplate?> GetByNameAsync(string name);
    Task<IEnumerable<NotificationTemplate>> GetByTypeAsync(NotificationType type);
    Task<IEnumerable<NotificationTemplate>> GetActiveTemplatesAsync();
    Task AddAsync(NotificationTemplate template);
    Task UpdateAsync(NotificationTemplate template);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string name);
}