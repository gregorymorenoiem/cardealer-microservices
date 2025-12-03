namespace FeatureToggleService.Domain.Entities;

/// <summary>
/// Represents a user's assignment to an experiment variant
/// </summary>
public class ExperimentAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Foreign key to ABExperiment</summary>
    public Guid ExperimentId { get; set; }

    /// <summary>Foreign key to ExperimentVariant</summary>
    public Guid VariantId { get; set; }

    /// <summary>User identifier</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Session identifier (for anonymous users)</summary>
    public string? SessionId { get; set; }

    /// <summary>Device identifier</summary>
    public string? DeviceId { get; set; }

    /// <summary>User agent string</summary>
    public string? UserAgent { get; set; }

    /// <summary>IP address (hashed for privacy)</summary>
    public string? IpAddressHash { get; set; }

    /// <summary>Geographic region</summary>
    public string? Region { get; set; }

    /// <summary>User attributes at time of assignment (JSON)</summary>
    public string? UserAttributes { get; set; }

    /// <summary>Whether the user was exposed to the variant (viewed the experience)</summary>
    public bool IsExposed { get; set; } = false;

    /// <summary>Timestamp when user was exposed</summary>
    public DateTime? ExposedAt { get; set; }

    /// <summary>Whether the user converted (completed primary goal)</summary>
    public bool HasConverted { get; set; } = false;

    /// <summary>Timestamp of conversion</summary>
    public DateTime? ConvertedAt { get; set; }

    /// <summary>Assignment date</summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Assignment bucket (for consistent hashing)</summary>
    public int Bucket { get; set; }

    /// <summary>Whether this is a forced assignment (override)</summary>
    public bool IsForced { get; set; } = false;

    /// <summary>Reason for forced assignment</summary>
    public string? ForcedReason { get; set; }

    // Navigation properties
    public ABExperiment? Experiment { get; set; }
    public ExperimentVariant? Variant { get; set; }
    public List<ExperimentMetric> Metrics { get; set; } = new();

    /// <summary>
    /// Mark user as exposed to the variant
    /// </summary>
    public void MarkAsExposed()
    {
        if (!IsExposed)
        {
            IsExposed = true;
            ExposedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Mark user as converted
    /// </summary>
    public void MarkAsConverted()
    {
        if (!HasConverted)
        {
            HasConverted = true;
            ConvertedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Get time to conversion
    /// </summary>
    public TimeSpan? GetTimeToConversion()
    {
        if (!HasConverted || !ExposedAt.HasValue || !ConvertedAt.HasValue)
            return null;

        return ConvertedAt.Value - ExposedAt.Value;
    }
}
