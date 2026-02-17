using InvoicingService.Domain.Entities;

namespace InvoicingService.Domain.Interfaces;

public interface ICfdiConfigurationRepository
{
    Task<CfdiConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CfdiConfiguration?> GetByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task<CfdiConfiguration?> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsForDealerAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task AddAsync(CfdiConfiguration configuration, CancellationToken cancellationToken = default);
    Task UpdateAsync(CfdiConfiguration configuration, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
