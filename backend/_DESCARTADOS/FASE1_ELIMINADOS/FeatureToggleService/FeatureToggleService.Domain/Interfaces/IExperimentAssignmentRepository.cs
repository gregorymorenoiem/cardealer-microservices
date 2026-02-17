using FeatureToggleService.Domain.Entities;

namespace FeatureToggleService.Domain.Interfaces;

/// <summary>
/// Repository interface for ExperimentAssignment entities
/// </summary>
public interface IExperimentAssignmentRepository
{
    Task<ExperimentAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ExperimentAssignment?> GetByExperimentAndUserAsync(Guid experimentId, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExperimentAssignment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ExperimentAssignment>> GetByExperimentAsync(Guid experimentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExperimentAssignment>> GetByVariantAsync(Guid variantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExperimentAssignment>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExperimentAssignment>> GetExposedAsync(Guid experimentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExperimentAssignment>> GetConvertedAsync(Guid experimentId, CancellationToken cancellationToken = default);
    Task<ExperimentAssignment> AddAsync(ExperimentAssignment assignment, CancellationToken cancellationToken = default);
    Task<ExperimentAssignment> UpdateAsync(ExperimentAssignment assignment, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountByExperimentAsync(Guid experimentId, CancellationToken cancellationToken = default);
    Task<int> CountByVariantAsync(Guid variantId, CancellationToken cancellationToken = default);
}
