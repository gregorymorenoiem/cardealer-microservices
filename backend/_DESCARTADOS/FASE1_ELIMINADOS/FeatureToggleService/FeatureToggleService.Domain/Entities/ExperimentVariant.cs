namespace FeatureToggleService.Domain.Entities;

/// <summary>
/// Represents a variant in an A/B test experiment
/// </summary>
public class ExperimentVariant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Foreign key to ABExperiment</summary>
    public Guid ExperimentId { get; set; }

    /// <summary>Unique key within the experiment (e.g., "control", "treatment-a")</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Human-readable name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description of the variant</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Whether this is the control variant</summary>
    public bool IsControl { get; set; } = false;

    /// <summary>Traffic weight percentage (0-100)</summary>
    public int Weight { get; set; } = 50;

    /// <summary>JSON payload with variant configuration</summary>
    public string? Payload { get; set; }

    /// <summary>Feature flag override value for this variant</summary>
    public bool? FeatureFlagValue { get; set; }

    /// <summary>CSS/styling changes (JSON)</summary>
    public string? StyleOverrides { get; set; }

    /// <summary>Custom parameters for the variant (JSON)</summary>
    public string? Parameters { get; set; }

    /// <summary>Screenshot or mockup URL</summary>
    public string? MockupUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ABExperiment? Experiment { get; set; }
    public List<ExperimentAssignment> Assignments { get; set; } = new();
    public List<ExperimentMetric> Metrics { get; set; } = new();

    /// <summary>
    /// Get participant count for this variant
    /// </summary>
    public int GetParticipantCount() => Assignments.Count;

    /// <summary>
    /// Get conversion count for this variant based on a specific metric
    /// </summary>
    public int GetConversionCount(string metricKey)
    {
        return Metrics.Count(m => m.MetricKey == metricKey && m.Value > 0);
    }

    /// <summary>
    /// Calculate conversion rate for a specific metric
    /// </summary>
    public double GetConversionRate(string metricKey)
    {
        var participantCount = GetParticipantCount();
        if (participantCount == 0) return 0;

        var conversionCount = GetConversionCount(metricKey);
        return (double)conversionCount / participantCount;
    }

    /// <summary>
    /// Calculate average metric value
    /// </summary>
    public double GetAverageMetricValue(string metricKey)
    {
        var relevantMetrics = Metrics.Where(m => m.MetricKey == metricKey).ToList();
        if (!relevantMetrics.Any()) return 0;

        return relevantMetrics.Average(m => m.Value);
    }
}
