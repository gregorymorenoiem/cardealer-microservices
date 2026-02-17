using FeatureToggleService.Domain.Enums;

namespace FeatureToggleService.Domain.Entities;

/// <summary>
/// Represents a rollout strategy for gradual feature enablement
/// </summary>
public class RolloutStrategy
{
    public Guid Id { get; set; }

    /// <summary>Foreign key to FeatureFlag</summary>
    public Guid FeatureFlagId { get; set; }

    /// <summary>Type of rollout strategy</summary>
    public RolloutType Type { get; set; } = RolloutType.Percentage;

    /// <summary>Percentage of users to enable (0-100)</summary>
    public int Percentage { get; set; } = 0;

    /// <summary>List of specific user IDs to include</summary>
    public List<string> TargetUserIds { get; set; } = new();

    /// <summary>List of tenant/organization IDs to include</summary>
    public List<string> TargetTenants { get; set; } = new();

    /// <summary>List of user groups/roles to include</summary>
    public List<string> TargetGroups { get; set; } = new();

    /// <summary>List of regions to include</summary>
    public List<string> TargetRegions { get; set; } = new();

    /// <summary>Custom attribute rules in JSON format</summary>
    public string? CustomRules { get; set; }

    /// <summary>Start date for scheduled rollout</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>End date for scheduled rollout (when reaching 100%)</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Whether to include/exclude beta users</summary>
    public bool IncludeBetaUsers { get; set; } = true;

    /// <summary>Whether to include/exclude internal users</summary>
    public bool IncludeInternalUsers { get; set; } = true;

    /// <summary>Seed for consistent hashing (optional)</summary>
    public string? HashSeed { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public FeatureFlag? FeatureFlag { get; set; }

    /// <summary>
    /// Update the rollout percentage
    /// </summary>
    public void SetPercentage(int percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100");

        Percentage = percentage;
        Type = RolloutType.Percentage;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add users to targeting
    /// </summary>
    public void AddTargetUsers(IEnumerable<string> userIds)
    {
        foreach (var userId in userIds)
        {
            if (!TargetUserIds.Contains(userId))
                TargetUserIds.Add(userId);
        }
        Type = RolloutType.UserIds;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove users from targeting
    /// </summary>
    public void RemoveTargetUsers(IEnumerable<string> userIds)
    {
        foreach (var userId in userIds)
        {
            TargetUserIds.Remove(userId);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Evaluate if a context matches this rollout strategy
    /// </summary>
    public bool Evaluate(EvaluationContext context)
    {
        return Type switch
        {
            RolloutType.Percentage => EvaluatePercentage(context),
            RolloutType.UserIds => EvaluateUserIds(context),
            RolloutType.Tenants => EvaluateTenants(context),
            RolloutType.Groups => EvaluateGroups(context),
            RolloutType.Regions => EvaluateRegions(context),
            RolloutType.CustomAttributes => EvaluateCustomAttributes(context),
            _ => false
        };
    }

    private bool EvaluatePercentage(EvaluationContext context)
    {
        if (Percentage == 0) return false;
        if (Percentage == 100) return true;

        // Use user ID for consistent hashing
        if (string.IsNullOrEmpty(context.UserId))
            return false;

        var hashInput = HashSeed ?? FeatureFlagId.ToString();
        var combinedHash = $"{hashInput}:{context.UserId}".GetHashCode();
        var bucket = Math.Abs(combinedHash % 100);

        return bucket < Percentage;
    }

    private bool EvaluateUserIds(EvaluationContext context)
    {
        if (string.IsNullOrEmpty(context.UserId))
            return false;

        return TargetUserIds.Contains(context.UserId);
    }

    private bool EvaluateTenants(EvaluationContext context)
    {
        if (string.IsNullOrEmpty(context.TenantId))
            return false;

        return TargetTenants.Contains(context.TenantId);
    }

    private bool EvaluateGroups(EvaluationContext context)
    {
        if (context.Groups == null || context.Groups.Count == 0)
            return false;

        return context.Groups.Any(g => TargetGroups.Contains(g));
    }

    private bool EvaluateRegions(EvaluationContext context)
    {
        if (string.IsNullOrEmpty(context.Region))
            return false;

        return TargetRegions.Contains(context.Region);
    }

    private bool EvaluateCustomAttributes(EvaluationContext context)
    {
        // Simplified evaluation - in production, use a proper rule engine
        if (string.IsNullOrEmpty(CustomRules) || context.Attributes == null)
            return false;

        // For now, just check if any custom rules are defined
        return !string.IsNullOrEmpty(CustomRules);
    }
}
