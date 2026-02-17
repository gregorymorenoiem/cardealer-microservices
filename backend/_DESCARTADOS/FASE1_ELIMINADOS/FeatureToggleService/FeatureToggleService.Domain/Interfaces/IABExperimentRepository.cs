using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;

namespace FeatureToggleService.Domain.Interfaces;

/// <summary>
/// Repository interface for ABExperiment entities
/// </summary>
public interface IABExperimentRepository
{
    Task<ABExperiment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ABExperiment?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<ABExperiment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ABExperiment>> GetByStatusAsync(ExperimentStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<ABExperiment>> GetRunningAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ABExperiment>> GetByFeatureFlagAsync(Guid featureFlagId, CancellationToken cancellationToken = default);
    Task<ABExperiment> AddAsync(ABExperiment experiment, CancellationToken cancellationToken = default);
    Task<ABExperiment> UpdateAsync(ABExperiment experiment, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(ExperimentStatus status, CancellationToken cancellationToken = default);
}
