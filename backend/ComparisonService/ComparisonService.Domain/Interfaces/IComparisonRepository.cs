using ComparisonService.Domain.Entities;

namespace ComparisonService.Domain.Interfaces;

public interface IComparisonRepository
{
    Task<Comparison?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Comparison?> GetByShareTokenAsync(string shareToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comparison>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Comparison> CreateAsync(Comparison comparison, CancellationToken cancellationToken = default);
    Task UpdateAsync(Comparison comparison, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
