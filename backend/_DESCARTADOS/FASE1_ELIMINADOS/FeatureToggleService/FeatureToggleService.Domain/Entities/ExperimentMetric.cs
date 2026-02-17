namespace FeatureToggleService.Domain.Entities;

/// <summary>
/// Represents a metric value recorded for an experiment
/// </summary>
public class ExperimentMetric
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Foreign key to ABExperiment</summary>
    public Guid ExperimentId { get; set; }

    /// <summary>Foreign key to ExperimentVariant</summary>
    public Guid VariantId { get; set; }

    /// <summary>Foreign key to ExperimentAssignment</summary>
    public Guid AssignmentId { get; set; }

    /// <summary>User identifier</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Metric key/name (e.g., "checkout_completed", "revenue", "time_on_page")</summary>
    public string MetricKey { get; set; } = string.Empty;

    /// <summary>Metric value</summary>
    public double Value { get; set; }

    /// <summary>Metric type (counter, gauge, timer, etc.)</summary>
    public string MetricType { get; set; } = "counter";

    /// <summary>Additional metadata (JSON)</summary>
    public string? Metadata { get; set; }

    /// <summary>Timestamp when metric was recorded</summary>
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ABExperiment? Experiment { get; set; }
    public ExperimentVariant? Variant { get; set; }
    public ExperimentAssignment? Assignment { get; set; }
}
