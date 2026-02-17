using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;

namespace FeatureToggleService.Domain.Interfaces;

/// <summary>
/// Repository interface for FeatureFlag entities
/// </summary>
public interface IFeatureFlagRepository
{
    Task<FeatureFlag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FeatureFlag?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeatureFlag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FeatureFlag>> GetByEnvironmentAsync(Enums.Environment environment, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeatureFlag>> GetByStatusAsync(FlagStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeatureFlag>> GetByTagAsync(string tag, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeatureFlag>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FeatureFlag>> GetExpiredAsync(CancellationToken cancellationToken = default);
    Task<FeatureFlag> AddAsync(FeatureFlag flag, CancellationToken cancellationToken = default);
    Task<FeatureFlag> UpdateAsync(FeatureFlag flag, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
