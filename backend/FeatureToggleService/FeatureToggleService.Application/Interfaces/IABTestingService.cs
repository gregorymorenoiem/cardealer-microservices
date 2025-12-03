using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;

namespace FeatureToggleService.Application.Interfaces;

/// <summary>
/// Service for managing A/B testing experiments
/// </summary>
public interface IABTestingService
{
    // Experiment management
    Task<ABExperiment> CreateExperimentAsync(ABExperiment experiment, CancellationToken cancellationToken = default);
    Task<ABExperiment?> GetExperimentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ABExperiment?> GetExperimentByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<List<ABExperiment>> GetAllExperimentsAsync(CancellationToken cancellationToken = default);
    Task<List<ABExperiment>> GetRunningExperimentsAsync(CancellationToken cancellationToken = default);
    Task<ABExperiment> UpdateExperimentAsync(ABExperiment experiment, CancellationToken cancellationToken = default);
    Task<IEnumerable<ABExperiment>> GetByStatusAsync(ExperimentStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<ABExperiment>> GetByFeatureFlagAsync(Guid featureFlagId, CancellationToken cancellationToken = default);

    // Experiment lifecycle
    Task<ABExperiment> StartExperimentAsync(Guid experimentId, string modifiedBy, CancellationToken cancellationToken = default);
    Task<ABExperiment> PauseExperimentAsync(Guid experimentId, string modifiedBy, CancellationToken cancellationToken = default);
    Task<ABExperiment> CompleteExperimentAsync(Guid experimentId, Guid? winningVariantId, string? notes, string modifiedBy, CancellationToken cancellationToken = default);
    Task<ABExperiment> CancelExperimentAsync(Guid experimentId, string reason, string modifiedBy, CancellationToken cancellationToken = default);

    // Variant assignment
    Task<ExperimentAssignment> AssignVariantAsync(Guid experimentId, string userId, string? sessionId = null, string? deviceId = null, Dictionary<string, object>? userAttributes = null, CancellationToken cancellationToken = default);
    Task<ExperimentAssignment?> GetAssignmentAsync(Guid experimentId, string userId, CancellationToken cancellationToken = default);
    Task<ExperimentVariant> ForceVariantAsync(Guid experimentId, string userId, string variantKey, string reason);

    // Exposure and conversion tracking
    Task TrackExposureAsync(Guid experimentId, string userId, CancellationToken cancellationToken = default);
    Task TrackConversionAsync(Guid experimentId, string userId, string? metricKey = null, CancellationToken cancellationToken = default);
    Task TrackMetricAsync(Guid experimentId, Guid variantId, string userId, string metricKey, double value, string metricType = "counter", string? metadata = null, CancellationToken cancellationToken = default);

    // Analysis
    Task<ExperimentAnalysis> AnalyzeExperimentAsync(Guid experimentId, CancellationToken cancellationToken = default);
    Task<ExperimentResult> GetVariantResultAsync(Guid experimentId, Guid variantId);
    Task<List<ExperimentResult>> CompareVariantsAsync(Guid experimentId);
    Task<bool> HasStatisticalSignificanceAsync(Guid experimentId, double confidenceLevel = 0.95);

    // Recommendations
    Task<Guid?> GetRecommendedWinnerAsync(Guid experimentId);
    Task<double> CalculateStatisticalPowerAsync(Guid experimentId);
    Task<int> EstimateDaysToSignificanceAsync(Guid experimentId);
}
