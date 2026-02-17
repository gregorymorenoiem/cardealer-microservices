using FeatureToggleService.Domain.Enums;

namespace FeatureToggleService.Domain.Entities;

/// <summary>
/// Represents a feature flag configuration
/// </summary>
public class FeatureFlag
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Unique key identifier for the flag (e.g., "new-checkout-flow")</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Human-readable name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description of what the flag controls</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Current status of the flag</summary>
    public FlagStatus Status { get; set; } = FlagStatus.Draft;

    /// <summary>Whether the flag is currently enabled</summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>Environment where the flag applies</summary>
    public Enums.Environment Environment { get; set; } = Enums.Environment.All;

    /// <summary>Tags for categorization and filtering</summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>Percentage of users to enable the flag for (0-100)</summary>
    public int RolloutPercentage { get; set; } = 0;

    /// <summary>Target user IDs for explicit enablement</summary>
    public List<string> TargetUserIds { get; set; } = new();

    /// <summary>Target groups for explicit enablement</summary>
    public List<string> TargetGroups { get; set; } = new();

    /// <summary>JSON payload for complex flag values</summary>
    public string? JsonPayload { get; set; }

    /// <summary>Owner or team responsible for this flag</summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>Expiration date for temporary flags</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Whether this is a permanent flag (vs temporary)</summary>
    public bool IsPermanent { get; set; } = false;

    /// <summary>Kill switch triggered - emergency disable</summary>
    public bool KillSwitchTriggered { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? ModifiedBy { get; set; }

    // Navigation properties
    public RolloutStrategy? RolloutStrategy { get; set; }
    public List<FeatureFlagHistory> History { get; set; } = new();

    /// <summary>
    /// Check if the flag is expired
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    /// <summary>
    /// Enable the feature flag for all users
    /// </summary>
    public void Enable(string modifiedBy)
    {
        IsEnabled = true;
        Status = FlagStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Disable the feature flag for all users
    /// </summary>
    public void Disable(string modifiedBy)
    {
        IsEnabled = false;
        Status = FlagStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Archive the flag (soft delete)
    /// </summary>
    public void Archive(string modifiedBy)
    {
        IsEnabled = false;
        Status = FlagStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Restore an archived flag
    /// </summary>
    public void Restore(string modifiedBy)
    {
        Status = FlagStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Trigger kill switch - emergency disable
    /// </summary>
    public void TriggerKillSwitch(string modifiedBy, string reason)
    {
        KillSwitchTriggered = true;
        IsEnabled = false;
        Status = FlagStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Reset kill switch
    /// </summary>
    public void ResetKillSwitch(string modifiedBy)
    {
        KillSwitchTriggered = false;
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Set rollout percentage
    /// </summary>
    public void SetRolloutPercentage(int percentage, string modifiedBy)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100");

        RolloutPercentage = percentage;
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Add target users
    /// </summary>
    public void AddTargetUsers(List<string> userIds, string modifiedBy)
    {
        foreach (var userId in userIds)
        {
            if (!TargetUserIds.Contains(userId))
                TargetUserIds.Add(userId);
        }
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Remove target users
    /// </summary>
    public void RemoveTargetUsers(List<string> userIds, string modifiedBy)
    {
        TargetUserIds.RemoveAll(u => userIds.Contains(u));
        UpdatedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Check if the flag is active (not archived and not killed)
    /// </summary>
    public bool IsActive() => Status != FlagStatus.Archived && !KillSwitchTriggered && IsEnabled;
}

/// <summary>
/// Context for evaluating feature flags
/// </summary>
public class EvaluationContext
{
    public string? UserId { get; set; }
    public string? TenantId { get; set; }
    public string? Region { get; set; }
    public string? Environment { get; set; }
    public List<string> Groups { get; set; } = new();
    public Dictionary<string, string> Attributes { get; set; } = new();
}
