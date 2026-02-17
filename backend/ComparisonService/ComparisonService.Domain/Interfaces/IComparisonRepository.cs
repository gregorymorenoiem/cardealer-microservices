using ComparisonService.Domain.Entities;

namespace ComparisonService.Domain.Interfaces;

public interface IComparisonRepository
{
    Task<VehicleComparison?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VehicleComparison?> GetByShareTokenAsync(string shareToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<VehicleComparison>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<VehicleComparison> CreateAsync(VehicleComparison comparison, CancellationToken cancellationToken = default);
    Task UpdateAsync(VehicleComparison comparison, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
