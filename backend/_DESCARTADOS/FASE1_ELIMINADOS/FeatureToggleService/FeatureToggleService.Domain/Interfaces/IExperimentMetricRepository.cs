using FeatureToggleService.Domain.Entities;

namespace FeatureToggleService.Domain.Interfaces;

/// <summary>
/// Repository interface for ExperimentMetric entities
/// </summary>
public interface IExperimentMetricRepository
{
    Task<ExperimentMetric?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExperimentMetric>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ExperimentMetric>> GetByExperimentAsync(Guid experimentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExperimentMetric>> GetByVariantAsync(Guid variantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExperimentMetric>> GetByAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExperimentMetric>> GetByMetricKeyAsync(Guid experimentId, string metricKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExperimentMetric>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<ExperimentMetric> AddAsync(ExperimentMetric metric, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountByExperimentAsync(Guid experimentId, CancellationToken cancellationToken = default);
    Task<int> CountByVariantAsync(Guid variantId, CancellationToken cancellationToken = default);
    Task<double> SumValueByVariantAsync(Guid variantId, string metricKey, CancellationToken cancellationToken = default);
    Task<double> AverageValueByVariantAsync(Guid variantId, string metricKey, CancellationToken cancellationToken = default);
}
