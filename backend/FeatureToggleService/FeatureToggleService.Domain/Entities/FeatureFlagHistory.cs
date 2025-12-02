using FeatureToggleService.Domain.Enums;

namespace FeatureToggleService.Domain.Entities;

/// <summary>
/// Represents a historical change to a feature flag
/// </summary>
public class FeatureFlagHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Foreign key to FeatureFlag</summary>
    public Guid FeatureFlagId { get; set; }

    /// <summary>Type of change made</summary>
    public ChangeType ChangeType { get; set; }

    /// <summary>Previous value (as string for flexibility)</summary>
    public string? PreviousValue { get; set; }

    /// <summary>New value (as string for flexibility)</summary>
    public string? NewValue { get; set; }

    /// <summary>Description of the change</summary>
    public string ChangeDescription { get; set; } = string.Empty;

    /// <summary>User who made the change</summary>
    public string ChangedBy { get; set; } = string.Empty;

    /// <summary>Timestamp of the change</summary>
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>IP address of the user who made the change</summary>
    public string? IpAddress { get; set; }

    /// <summary>Reason or ticket number for the change</summary>
    public string? Reason { get; set; }

    // Navigation property
    public FeatureFlag? FeatureFlag { get; set; }
}
