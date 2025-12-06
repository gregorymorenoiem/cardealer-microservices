using IntegrationService.Domain.Entities;

namespace IntegrationService.Domain.Interfaces;

public interface ISyncJobRepository
{
    Task<SyncJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SyncJob>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SyncJob>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SyncJob>> GetByStatusAsync(SyncStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<SyncJob>> GetScheduledAsync(CancellationToken cancellationToken = default);
    Task<SyncJob> AddAsync(SyncJob syncJob, CancellationToken cancellationToken = default);
    Task UpdateAsync(SyncJob syncJob, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
