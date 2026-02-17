using System.Security.Cryptography;
using System.Text;
using FeatureToggleService.Application.Interfaces;
using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using FeatureToggleService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FeatureToggleService.Infrastructure.Services;

/// <summary>
/// Service for A/B testing experiments with statistical analysis
/// </summary>
public class ABTestingService : IABTestingService
{
    private readonly IABExperimentRepository _experimentRepository;
    private readonly IExperimentAssignmentRepository _assignmentRepository;
    private readonly IExperimentMetricRepository _metricRepository;
    private readonly ILogger<ABTestingService> _logger;

    public ABTestingService(
        IABExperimentRepository experimentRepository,
        IExperimentAssignmentRepository assignmentRepository,
        IExperimentMetricRepository metricRepository,
        ILogger<ABTestingService> logger)
    {
        _experimentRepository = experimentRepository;
        _assignmentRepository = assignmentRepository;
        _metricRepository = metricRepository;
        _logger = logger;
    }

    #region Experiment Management

    public async Task<ABExperiment> CreateExperimentAsync(ABExperiment experiment, CancellationToken cancellationToken = default)
    {
        // Validate
        if (string.IsNullOrEmpty(experiment.Key))
            throw new ArgumentException("Experiment key is required");

        if (string.IsNullOrEmpty(experiment.Name))
            throw new ArgumentException("Experiment name is required");

        // Check for duplicate key
        var existing = await GetExperimentByKeyAsync(experiment.Key, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException($"Experiment with key '{experiment.Key}' already exists");

        experiment.Status = ExperimentStatus.Draft;
        experiment.CreatedAt = DateTime.UtcNow;

        await _experimentRepository.AddAsync(experiment, cancellationToken);
        _logger.LogInformation("Created experiment {ExperimentKey} with ID {ExperimentId}",
            experiment.Key, experiment.Id);

        return experiment;
    }

    public async Task<ABExperiment?> GetExperimentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _experimentRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<ABExperiment?> GetExperimentByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _experimentRepository.GetByKeyAsync(key, cancellationToken);
    }

    public async Task<List<ABExperiment>> GetAllExperimentsAsync(CancellationToken cancellationToken = default)
    {
        return (await _experimentRepository.GetAllAsync(cancellationToken)).ToList();
    }

    public async Task<List<ABExperiment>> GetRunningExperimentsAsync(CancellationToken cancellationToken = default)
    {
        return (await _experimentRepository.GetRunningAsync(cancellationToken)).ToList();
    }

    public async Task<ABExperiment> UpdateExperimentAsync(ABExperiment experiment, CancellationToken cancellationToken = default)
    {
        experiment.UpdatedAt = DateTime.UtcNow;
        await _experimentRepository.UpdateAsync(experiment, cancellationToken);
        return experiment;
    }

    public async Task<IEnumerable<ABExperiment>> GetByStatusAsync(ExperimentStatus status, CancellationToken cancellationToken = default)
    {
        return await _experimentRepository.GetByStatusAsync(status, cancellationToken);
    }

    public async Task<IEnumerable<ABExperiment>> GetByFeatureFlagAsync(Guid featureFlagId, CancellationToken cancellationToken = default)
    {
        return await _experimentRepository.GetByFeatureFlagAsync(featureFlagId, cancellationToken);
    }

    #endregion

    #region Experiment Lifecycle

    public async Task<ABExperiment> StartExperimentAsync(Guid experimentId, string modifiedBy, CancellationToken cancellationToken = default)
    {
        var experiment = await GetExperimentAsync(experimentId, cancellationToken);
        if (experiment == null)
            throw new InvalidOperationException($"Experiment {experimentId} not found");

        experiment.Start(modifiedBy);
        await UpdateExperimentAsync(experiment, cancellationToken);

        _logger.LogInformation("Started experiment {ExperimentKey} by {ModifiedBy}",
            experiment.Key, modifiedBy);

        return experiment;
    }

    public async Task<ABExperiment> PauseExperimentAsync(Guid experimentId, string modifiedBy, CancellationToken cancellationToken = default)
    {
        var experiment = await GetExperimentAsync(experimentId, cancellationToken);
        if (experiment == null)
            throw new InvalidOperationException($"Experiment {experimentId} not found");

        experiment.Pause(modifiedBy);
        await UpdateExperimentAsync(experiment, cancellationToken);

        _logger.LogInformation("Paused experiment {ExperimentKey} by {ModifiedBy}",
            experiment.Key, modifiedBy);

        return experiment;
    }

    public async Task<ABExperiment> CompleteExperimentAsync(Guid experimentId, Guid? winningVariantId,
        string? notes, string modifiedBy, CancellationToken cancellationToken = default)
    {
        var experiment = await GetExperimentAsync(experimentId, cancellationToken);
        if (experiment == null)
            throw new InvalidOperationException($"Experiment {experimentId} not found");

        // Run final analysis to get confidence
        var analysis = await AnalyzeExperimentAsync(experimentId, cancellationToken);
        var winnerResult = analysis.VariantResults.FirstOrDefault(v => v.VariantId == winningVariantId);
        var confidence = winnerResult?.GetConfidencePercent() ?? 0;

        experiment.Complete(winningVariantId ?? Guid.Empty, confidence, notes ?? string.Empty, modifiedBy);
        await UpdateExperimentAsync(experiment, cancellationToken);

        _logger.LogInformation("Completed experiment {ExperimentKey} with winner variant {VariantId}, confidence {Confidence}%",
            experiment.Key, winningVariantId, confidence);

        return experiment;
    }

    public async Task<ABExperiment> CancelExperimentAsync(Guid experimentId, string reason, string modifiedBy, CancellationToken cancellationToken = default)
    {
        var experiment = await GetExperimentAsync(experimentId, cancellationToken);
        if (experiment == null)
            throw new InvalidOperationException($"Experiment {experimentId} not found");

        experiment.Cancel(reason, modifiedBy);
        await UpdateExperimentAsync(experiment, cancellationToken);

        _logger.LogWarning("Cancelled experiment {ExperimentKey}: {Reason}",
            experiment.Key, reason);

        return experiment;
    }

    #endregion

    #region Variant Assignment

    public async Task<ExperimentAssignment> AssignVariantAsync(Guid experimentId, string userId, string? sessionId = null, string? deviceId = null, Dictionary<string, object>? userAttributes = null, CancellationToken cancellationToken = default)
    {
        var experiment = await GetExperimentAsync(experimentId, cancellationToken);
        if (experiment == null)
            throw new InvalidOperationException($"Experiment {experimentId} not found");

        if (!experiment.IsRunning())
            throw new InvalidOperationException($"Experiment is not running");

        // Check if user already has an assignment (sticky bucketing)
        var existingAssignment = await GetAssignmentAsync(experiment.Id, userId, cancellationToken);
        if (existingAssignment != null && experiment.UseStickyBucketing)
        {
            return existingAssignment;
        }

        // Check traffic allocation
        if (experiment.TrafficAllocation < 100)
        {
            var trafficBucket = GetBucket(userId, experiment.Id.ToString(), 100);
            if (trafficBucket >= experiment.TrafficAllocation)
            {
                // User not in experiment traffic - assign to control
                var controlVariant = experiment.Variants.FirstOrDefault(v => v.IsControl);
                if (controlVariant == null)
                    throw new InvalidOperationException("No control variant found");

                var controlAssignment = new ExperimentAssignment
                {
                    ExperimentId = experiment.Id,
                    VariantId = controlVariant.Id,
                    UserId = userId,
                    SessionId = sessionId,
                    DeviceId = deviceId,
                    UserAttributes = System.Text.Json.JsonSerializer.Serialize(userAttributes ?? new Dictionary<string, object>()),
                    Bucket = trafficBucket,
                    AssignedAt = DateTime.UtcNow
                };

                await _assignmentRepository.AddAsync(controlAssignment, cancellationToken);
                return controlAssignment;
            }
        }

        // Assign variant based on weights
        var variant = SelectVariantByWeight(experiment, userId);

        // Create assignment record
        var assignment = new ExperimentAssignment
        {
            ExperimentId = experiment.Id,
            VariantId = variant.Id,
            UserId = userId,
            SessionId = sessionId,
            DeviceId = deviceId,
            UserAttributes = System.Text.Json.JsonSerializer.Serialize(userAttributes ?? new Dictionary<string, object>()),
            Bucket = GetBucket(userId, experiment.Id.ToString(), 100),
            AssignedAt = DateTime.UtcNow
        };

        await _assignmentRepository.AddAsync(assignment, cancellationToken);

        _logger.LogInformation("Assigned user {UserId} to variant {VariantKey} in experiment {ExperimentId}",
            userId, variant.Key, experiment.Id);

        return assignment;
    }

    public async Task<ExperimentAssignment?> GetAssignmentAsync(Guid experimentId, string userId, CancellationToken cancellationToken = default)
    {
        return await _assignmentRepository.GetByExperimentAndUserAsync(experimentId, userId, cancellationToken);
    }

    public async Task<ExperimentVariant> ForceVariantAsync(Guid experimentId, string userId,
        string variantKey, string reason)
    {
        var experiment = await GetExperimentAsync(experimentId);
        if (experiment == null)
            throw new InvalidOperationException($"Experiment {experimentId} not found");

        var variant = experiment.Variants.FirstOrDefault(v => v.Key == variantKey);
        if (variant == null)
            throw new InvalidOperationException($"Variant '{variantKey}' not found");

        // Remove existing assignment if any
        var existingAssignment = await GetAssignmentAsync(experimentId, userId);
        if (existingAssignment != null)
        {
            await _assignmentRepository.DeleteAsync(existingAssignment.Id);
        }

        // Create forced assignment
        var assignment = new ExperimentAssignment
        {
            ExperimentId = experimentId,
            VariantId = variant.Id,
            UserId = userId,
            IsForced = true,
            ForcedReason = reason,
            AssignedAt = DateTime.UtcNow,
            Bucket = -1 // Special bucket for forced assignments
        };

        await _assignmentRepository.AddAsync(assignment);

        _logger.LogInformation("Forced assignment of user {UserId} to variant {VariantKey}: {Reason}",
            userId, variantKey, reason);

        return variant;
    }

    private ExperimentVariant SelectVariantByWeight(ABExperiment experiment, string userId)
    {
        // Get bucket (0-99) for consistent assignment
        var bucket = GetBucket(userId, experiment.Id.ToString(), 100);

        // Distribute buckets according to weights
        var cumulativeWeight = 0;
        foreach (var variant in experiment.Variants.OrderBy(v => v.IsControl ? 0 : 1))
        {
            cumulativeWeight += variant.Weight;
            if (bucket < cumulativeWeight)
            {
                return variant;
            }
        }

        // Fallback to control or first variant
        return experiment.Variants.FirstOrDefault(v => v.IsControl)
            ?? experiment.Variants.First();
    }

    private int GetBucket(string userId, string seed, int numBuckets)
    {
        // Consistent hashing for stable bucket assignment
        var input = $"{seed}:{userId}";
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        var hashInt = BitConverter.ToInt32(hash, 0);
        return Math.Abs(hashInt % numBuckets);
    }

    #endregion

    #region Tracking

    public async Task TrackExposureAsync(Guid experimentId, string userId, CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentAsync(experimentId, userId, cancellationToken);
        if (assignment == null)
        {
            _logger.LogWarning("No assignment found for user {UserId} in experiment {ExperimentId}",
                userId, experimentId);
            return;
        }

        if (!assignment.IsExposed)
        {
            assignment.MarkAsExposed();
            await _assignmentRepository.UpdateAsync(assignment, cancellationToken);

            _logger.LogDebug("Tracked exposure for user {UserId} in experiment {ExperimentId}",
                userId, experimentId);
        }
    }

    public async Task TrackConversionAsync(Guid experimentId, string userId, string? metricKey = null, CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentAsync(experimentId, userId, cancellationToken);
        if (assignment == null)
        {
            _logger.LogWarning("No assignment found for user {UserId} in experiment {ExperimentId}",
                userId, experimentId);
            return;
        }

        if (!assignment.HasConverted)
        {
            assignment.MarkAsConverted();
            await _assignmentRepository.UpdateAsync(assignment, cancellationToken);
        }

        // Also track as a metric
        var actualMetricKey = metricKey ?? "conversion";
        await TrackMetricAsync(experimentId, assignment.VariantId, userId, actualMetricKey, 1, "counter", null, cancellationToken);

        _logger.LogInformation("Tracked conversion for user {UserId} in experiment {ExperimentId}",
            userId, experimentId);
    }

    public async Task TrackMetricAsync(Guid experimentId, Guid variantId, string userId, string metricKey,
        double value, string metricType = "counter", string? metadata = null, CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentAsync(experimentId, userId, cancellationToken);
        if (assignment == null)
        {
            _logger.LogWarning("No assignment found for user {UserId} in experiment {ExperimentId}",
                userId, experimentId);
            return;
        }

        var metric = new ExperimentMetric
        {
            ExperimentId = experimentId,
            VariantId = variantId,
            AssignmentId = assignment.Id,
            UserId = userId,
            MetricKey = metricKey,
            Value = value,
            MetricType = metricType,
            Metadata = metadata,
            RecordedAt = DateTime.UtcNow
        };

        await _metricRepository.AddAsync(metric, cancellationToken);

        _logger.LogDebug("Tracked metric {MetricKey}={Value} for user {UserId} in experiment {ExperimentId}",
            metricKey, value, userId, experimentId);
    }

    #endregion

    #region Analysis

    public async Task<ExperimentAnalysis> AnalyzeExperimentAsync(Guid experimentId, CancellationToken cancellationToken = default)
    {
        var experiment = await GetExperimentAsync(experimentId, cancellationToken);
        if (experiment == null)
            throw new InvalidOperationException($"Experiment {experimentId} not found");

        var assignments = (await _assignmentRepository.GetByExperimentAsync(experimentId, cancellationToken)).ToList();
        var metrics = (await _metricRepository.GetByExperimentAsync(experimentId, cancellationToken)).ToList();

        var analysis = new ExperimentAnalysis
        {
            ExperimentId = experimentId,
            ExperimentKey = experiment.Key,
            ExperimentName = experiment.Name,
            TotalParticipants = assignments.Count,
            TotalExposures = assignments.Count(a => a.IsExposed),
            DaysRunning = experiment.StartDate.HasValue
                ? (int)(DateTime.UtcNow - experiment.StartDate.Value).TotalDays
                : 0,
            HasReachedMinSampleSize = experiment.HasReachedMinSampleSize()
        };

        // Analyze each variant
        ExperimentResult? controlResult = null;
        foreach (var variant in experiment.Variants)
        {
            var result = await CalculateVariantResultAsync(experiment, variant, assignments, metrics);
            analysis.VariantResults.Add(result);

            if (variant.IsControl)
            {
                controlResult = result;
            }
        }

        // Compare variants with control
        if (controlResult != null)
        {
            foreach (var result in analysis.VariantResults.Where(r => !r.IsControl))
            {
                CalculateStatisticalSignificance(controlResult, result, experiment.SignificanceLevel);
            }
        }

        // Generate recommendations
        GenerateRecommendations(analysis, experiment);

        return analysis;
    }

    public async Task<ExperimentResult> GetVariantResultAsync(Guid experimentId, Guid variantId)
    {
        var experiment = await GetExperimentAsync(experimentId);
        if (experiment == null)
            throw new InvalidOperationException($"Experiment {experimentId} not found");

        var variant = experiment.Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant == null)
            throw new InvalidOperationException($"Variant {variantId} not found");

        var assignments = (await _assignmentRepository.GetByExperimentAsync(experimentId)).ToList();
        var metrics = (await _metricRepository.GetByExperimentAsync(experimentId)).ToList();

        return await CalculateVariantResultAsync(experiment, variant, assignments, metrics);
    }

    public async Task<List<ExperimentResult>> CompareVariantsAsync(Guid experimentId)
    {
        var analysis = await AnalyzeExperimentAsync(experimentId);
        return analysis.VariantResults;
    }

    public async Task<bool> HasStatisticalSignificanceAsync(Guid experimentId, double confidenceLevel = 0.95)
    {
        var analysis = await AnalyzeExperimentAsync(experimentId);
        return analysis.VariantResults
            .Any(v => !v.IsControl && v.IsStatisticallySignificant == true);
    }

    private Task<ExperimentResult> CalculateVariantResultAsync(
        ABExperiment experiment,
        ExperimentVariant variant,
        List<ExperimentAssignment> allAssignments,
        List<ExperimentMetric> allMetrics)
    {
        var variantAssignments = allAssignments.Where(a => a.VariantId == variant.Id).ToList();
        var variantMetrics = allMetrics.Where(m => m.VariantId == variant.Id).ToList();

        var totalAssignments = variantAssignments.Count;
        var totalExposures = variantAssignments.Count(a => a.IsExposed);
        var totalConversions = variantAssignments.Count(a => a.HasConverted);

        var conversionRate = totalExposures > 0 ? (double)totalConversions / totalExposures : 0;
        var standardError = CalculateStandardError(conversionRate, totalExposures);

        // 95% confidence interval (Z = 1.96)
        var zScore = 1.96;
        var marginOfError = zScore * standardError;

        var result = new ExperimentResult
        {
            VariantId = variant.Id,
            VariantKey = variant.Key,
            VariantName = variant.Name,
            IsControl = variant.IsControl,
            TotalAssignments = totalAssignments,
            TotalExposures = totalExposures,
            TotalConversions = totalConversions,
            ConversionRate = conversionRate,
            StandardError = standardError,
            ConfidenceIntervalLower = Math.Max(0, conversionRate - marginOfError),
            ConfidenceIntervalUpper = Math.Min(1, conversionRate + marginOfError)
        };

        // Calculate secondary metrics
        var primaryMetricKey = experiment.PrimaryMetric;
        if (!string.IsNullOrEmpty(primaryMetricKey))
        {
            var primaryMetrics = variantMetrics.Where(m => m.MetricKey == primaryMetricKey).ToList();
            if (primaryMetrics.Any())
            {
                result.SecondaryMetrics[primaryMetricKey] = primaryMetrics.Average(m => m.Value);
            }
        }

        // Revenue metrics
        var revenueMetrics = variantMetrics.Where(m => m.MetricKey == "revenue").ToList();
        if (revenueMetrics.Any())
        {
            result.TotalRevenue = revenueMetrics.Sum(m => m.Value);
            result.AverageRevenuePerUser = totalExposures > 0
                ? result.TotalRevenue / totalExposures
                : 0;
        }

        return Task.FromResult(result);
    }

    private void CalculateStatisticalSignificance(
        ExperimentResult control,
        ExperimentResult treatment,
        double significanceLevel)
    {
        // Calculate absolute difference
        treatment.AbsoluteDifference = treatment.ConversionRate - control.ConversionRate;

        // Calculate relative lift
        if (control.ConversionRate > 0)
        {
            treatment.RelativeLift = (treatment.ConversionRate - control.ConversionRate) / control.ConversionRate * 100;
        }

        // Z-test for proportions
        var pooledProportion = (control.TotalConversions + treatment.TotalConversions)
            / (double)(control.TotalExposures + treatment.TotalExposures);

        var standardError = Math.Sqrt(
            pooledProportion * (1 - pooledProportion) *
            (1.0 / control.TotalExposures + 1.0 / treatment.TotalExposures)
        );

        if (standardError > 0)
        {
            treatment.ZScore = (treatment.ConversionRate - control.ConversionRate) / standardError;
            treatment.PValue = CalculatePValue(treatment.ZScore.Value);
            treatment.IsStatisticallySignificant = treatment.PValue < (1 - significanceLevel);
        }
    }

    private double CalculateStandardError(double proportion, int sampleSize)
    {
        if (sampleSize == 0) return 0;
        return Math.Sqrt((proportion * (1 - proportion)) / sampleSize);
    }

    private double CalculatePValue(double zScore)
    {
        // Simplified p-value calculation using normal distribution
        // For production, use a proper statistics library
        var absZ = Math.Abs(zScore);

        // Approximation for standard normal CDF
        var pValue = 0.5 * Math.Exp(-0.717 * absZ - 0.416 * absZ * absZ);

        return pValue * 2; // Two-tailed test
    }

    private void GenerateRecommendations(ExperimentAnalysis analysis, ABExperiment experiment)
    {
        var warnings = new List<string>();
        var insights = new List<string>();

        // Check sample size
        if (!analysis.HasReachedMinSampleSize)
        {
            warnings.Add($"Sample size ({analysis.TotalParticipants}) has not reached minimum ({experiment.MinSampleSize})");
        }

        // Check for winner
        var bestVariant = analysis.GetBestVariant();
        if (bestVariant != null)
        {
            analysis.RecommendedWinnerId = bestVariant.VariantId;
            analysis.WinnerConfidence = bestVariant.GetConfidencePercent();

            insights.Add($"Variant '{bestVariant.VariantName}' shows {bestVariant.RelativeLift:F1}% improvement with {bestVariant.GetConfidencePercent():F1}% confidence");

            if (bestVariant.GetConfidencePercent() >= 95)
            {
                analysis.Recommendation = $"Strong winner detected. Consider graduating '{bestVariant.VariantName}' to 100% traffic.";
            }
            else
            {
                analysis.Recommendation = "Continue running experiment to increase confidence level.";
            }
        }
        else
        {
            // Check for no difference
            var control = analysis.VariantResults.FirstOrDefault(v => v.IsControl);
            if (control != null && analysis.DaysRunning >= 14)
            {
                var maxDifference = analysis.VariantResults
                    .Where(v => !v.IsControl)
                    .Max(v => Math.Abs(v.ConversionRate - control.ConversionRate));

                if (maxDifference < experiment.MinDetectableEffect)
                {
                    analysis.Recommendation = "No significant difference detected. Consider ending experiment.";
                    insights.Add("Variants perform similarly to control. No clear winner.");
                }
            }
            else
            {
                analysis.Recommendation = "Experiment needs more time or traffic to reach statistical significance.";
            }
        }

        // Check for data quality issues
        var exposureRate = analysis.TotalExposures / (double)Math.Max(1, analysis.TotalParticipants);
        if (exposureRate < 0.5)
        {
            warnings.Add($"Low exposure rate ({exposureRate:P0}). Many users assigned but not exposed.");
        }

        analysis.Warnings = warnings;
        analysis.Insights = insights;
    }

    #endregion

    #region Recommendations

    public async Task<Guid?> GetRecommendedWinnerAsync(Guid experimentId)
    {
        var analysis = await AnalyzeExperimentAsync(experimentId);
        return analysis.RecommendedWinnerId;
    }

    public async Task<double> CalculateStatisticalPowerAsync(Guid experimentId)
    {
        // Simplified power calculation
        // For production, use proper power analysis library
        var analysis = await AnalyzeExperimentAsync(experimentId);

        if (analysis.TotalExposures < 100)
            return 0.2;

        if (analysis.TotalExposures < 500)
            return 0.5;

        if (analysis.TotalExposures < 1000)
            return 0.7;

        return 0.8;
    }

    public async Task<int> EstimateDaysToSignificanceAsync(Guid experimentId)
    {
        var experiment = await GetExperimentAsync(experimentId);
        if (experiment == null)
            throw new InvalidOperationException($"Experiment {experimentId} not found");

        var analysis = await AnalyzeExperimentAsync(experimentId);

        if (analysis.HasClearWinner())
            return 0; // Already significant

        // Simple estimation based on current rate
        var dailyParticipants = analysis.DaysRunning > 0
            ? analysis.TotalParticipants / analysis.DaysRunning
            : 100; // Assume 100/day if just started

        var neededParticipants = experiment.MinSampleSize - analysis.TotalParticipants;
        var estimatedDays = neededParticipants / Math.Max(1, dailyParticipants);

        return Math.Max(0, (int)Math.Ceiling((double)estimatedDays));
    }

    #endregion
}
