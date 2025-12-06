using IntegrationService.Domain.Entities;

namespace IntegrationService.Domain.Interfaces;

public interface IIntegrationRepository
{
    Task<Integration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Integration>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Integration>> GetByTypeAsync(IntegrationType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Integration>> GetByStatusAsync(IntegrationStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Integration>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Integration> AddAsync(Integration integration, CancellationToken cancellationToken = default);
    Task UpdateAsync(Integration integration, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
