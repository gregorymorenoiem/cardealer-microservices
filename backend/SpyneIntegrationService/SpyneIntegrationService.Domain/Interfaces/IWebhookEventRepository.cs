using SpyneIntegrationService.Domain.Entities;

namespace SpyneIntegrationService.Domain.Interfaces;

/// <summary>
/// Repository interface for webhook events
/// </summary>
public interface IWebhookEventRepository
{
    Task<SpyneWebhookEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SpyneWebhookEvent?> GetBySpyneJobIdAsync(string spyneJobId, CancellationToken cancellationToken = default);
    Task<List<SpyneWebhookEvent>> GetUnprocessedAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<SpyneWebhookEvent> AddAsync(SpyneWebhookEvent webhookEvent, CancellationToken cancellationToken = default);
    Task<SpyneWebhookEvent> UpdateAsync(SpyneWebhookEvent webhookEvent, CancellationToken cancellationToken = default);
}
