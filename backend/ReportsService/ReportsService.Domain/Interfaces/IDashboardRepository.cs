using ReportsService.Domain.Entities;

namespace ReportsService.Domain.Interfaces;

public interface IDashboardRepository
{
    Task<Dashboard?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Dashboard?> GetByIdWithWidgetsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Dashboard>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Dashboard>> GetByTypeAsync(DashboardType type, CancellationToken cancellationToken = default);
    Task<Dashboard?> GetDefaultAsync(CancellationToken cancellationToken = default);
    Task<Dashboard> AddAsync(Dashboard dashboard, CancellationToken cancellationToken = default);
    Task UpdateAsync(Dashboard dashboard, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
