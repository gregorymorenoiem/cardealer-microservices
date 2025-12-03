using FeatureToggleService.Domain.Enums;

namespace FeatureToggleService.Domain.Entities;

/// <summary>
/// Represents an A/B test experiment
/// </summary>
public class ABExperiment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Unique key identifier for the experiment</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Human-readable name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Hypothesis being tested</summary>
    public string Hypothesis { get; set; } = string.Empty;

    /// <summary>Detailed description</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Current status of the experiment</summary>
    public ExperimentStatus Status { get; set; } = ExperimentStatus.Draft;

    /// <summary>Feature flag associated with this experiment</summary>
    public Guid? FeatureFlagId { get; set; }

    /// <summary>Percentage of traffic allocated to the experiment (0-100)</summary>
    public int TrafficAllocation { get; set; } = 100;

    /// <summary>Owner or team responsible</summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>Tags for categorization</summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>Start date of the experiment</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>End date of the experiment</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Timestamp when experiment was started</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>Timestamp when experiment was completed</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Minimum sample size required</summary>
    public int MinSampleSize { get; set; } = 1000;

    /// <summary>Target statistical significance level (e.g., 0.95 for 95%)</summary>
    public double SignificanceLevel { get; set; } = 0.95;

    /// <summary>Minimum detectable effect size (e.g., 0.05 for 5%)</summary>
    public double MinDetectableEffect { get; set; } = 0.05;

    /// <summary>Primary metric to optimize</summary>
    public string PrimaryMetric { get; set; } = string.Empty;

    /// <summary>Secondary metrics to track</summary>
    public List<string> SecondaryMetrics { get; set; } = new();

    /// <summary>Whether to use sticky bucketing (user always gets same variant)</summary>
    public bool UseStickyBucketing { get; set; } = true;

    /// <summary>Target audience filters (JSON)</summary>
    public string? TargetAudience { get; set; }

    /// <summary>Exclusion rules (JSON)</summary>
    public string? ExclusionRules { get; set; }

    /// <summary>Segmentation rules for targeting (JSON Dictionary)</summary>
    public Dictionary<string, string> SegmentationRules { get; set; } = new();

    /// <summary>Reason for cancellation</summary>
    public string? CancelReason { get; set; }

    /// <summary>Notes about completion and results</summary>
    public string? CompletionNotes { get; set; }

    /// <summary>ID of the winning variant (after analysis)</summary>
    public Guid? WinningVariantId { get; set; }

    /// <summary>Confidence level of the winner selection</summary>
    public double? WinnerConfidence { get; set; }

    /// <summary>Notes about why experiment was stopped or results</summary>
    public string? ConclusionNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? ModifiedBy { get; set; }

    // Navigation properties
    public FeatureFlag? FeatureFlag { get; set; }
    public List<ExperimentVariant> Variants { get; set; } = new();
    public List<ExperimentAssignment> Assignments { get; set; } = new();
    public List<ExperimentMetric> Metrics { get; set; } = new();

    /// <summary>
    /// Check if experiment is currently running
    /// </summary>
    public bool IsRunning()
    {
        if (Status != ExperimentStatus.Running) return false;
        if (!StartDate.HasValue) return false;

        var now = DateTime.UtcNow;
        if (now < StartDate.Value) return false;
        if (EndDate.HasValue && now > EndDate.Value) return false;

        return true;
    }

    /// <summary>
    /// Start the experiment
    /// </summary>
    public void Start(string modifiedBy)
    {
        if (Status != ExperimentStatus.Draft && Status != ExperimentStatus.Paused)
            throw new InvalidOperationException($"Cannot start experiment in {Status} status");

        if (Variants.Count < 2)
            throw new InvalidOperationException("Experiment must have at least 2 variants");

        var totalWeight = Variants.Sum(v => v.Weight);
        if (totalWeight != 100)
            throw new InvalidOperationException("Variant weights must sum to 100%");

        Status = ExperimentStatus.Running;
        StartDate ??= DateTime.UtcNow;
        StartedAt ??= DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Pause the experiment
    /// </summary>
    public void Pause(string modifiedBy)
    {
        if (Status != ExperimentStatus.Running)
            throw new InvalidOperationException($"Cannot pause experiment in {Status} status");

        Status = ExperimentStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Complete the experiment
    /// </summary>
    public void Complete(Guid winningVariantId, double confidence, string notes, string modifiedBy)
    {
        if (Status != ExperimentStatus.Running && Status != ExperimentStatus.Paused)
            throw new InvalidOperationException($"Cannot complete experiment in {Status} status");

        if (!Variants.Any(v => v.Id == winningVariantId))
            throw new ArgumentException("Winning variant must be part of this experiment");

        Status = ExperimentStatus.Completed;
        EndDate = DateTime.UtcNow;
        CompletedAt = DateTime.UtcNow;
        WinningVariantId = winningVariantId;
        WinnerConfidence = confidence;
        ConclusionNotes = notes;
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Cancel the experiment
    /// </summary>
    public void Cancel(string reason, string modifiedBy)
    {
        Status = ExperimentStatus.Cancelled;
        EndDate = DateTime.UtcNow;
        ConclusionNotes = $"Cancelled: {reason}";
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Add a variant to the experiment
    /// </summary>
    public void AddVariant(ExperimentVariant variant)
    {
        if (Status != ExperimentStatus.Draft)
            throw new InvalidOperationException("Cannot add variants to a running experiment");

        if (Variants.Any(v => v.Key == variant.Key))
            throw new ArgumentException($"Variant with key '{variant.Key}' already exists");

        Variants.Add(variant);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Get total participants count
    /// </summary>
    public int GetTotalParticipants() => Assignments.Count;

    /// <summary>
    /// Check if experiment has reached minimum sample size
    /// </summary>
    public bool HasReachedMinSampleSize() => GetTotalParticipants() >= MinSampleSize;
}
