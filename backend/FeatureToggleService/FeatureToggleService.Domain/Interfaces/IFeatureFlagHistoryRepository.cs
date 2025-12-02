using FeatureToggleService.Domain.Entities;

namespace FeatureToggleService.Domain.Interfaces;

/// <summary>
/// Repository interface for FeatureFlagHistory entities
/// </summary>
public interface IFeatureFlagHistoryRepository
{
    Task<IEnumerable<FeatureFlagHistory>> GetByFlagIdAsync(Guid flagId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeatureFlagHistory>> GetByFlagIdAsync(Guid flagId, int limit, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeatureFlagHistory>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeatureFlagHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<FeatureFlagHistory> AddAsync(FeatureFlagHistory history, CancellationToken cancellationToken = default);
    Task<int> CountByFlagIdAsync(Guid flagId, CancellationToken cancellationToken = default);
}
