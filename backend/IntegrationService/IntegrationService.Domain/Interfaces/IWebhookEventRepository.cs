using IntegrationService.Domain.Entities;

namespace IntegrationService.Domain.Interfaces;

public interface IWebhookEventRepository
{
    Task<WebhookEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WebhookEvent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<WebhookEvent>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WebhookEvent>> GetByStatusAsync(WebhookStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<WebhookEvent>> GetPendingRetryAsync(CancellationToken cancellationToken = default);
    Task<WebhookEvent> AddAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default);
    Task UpdateAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
