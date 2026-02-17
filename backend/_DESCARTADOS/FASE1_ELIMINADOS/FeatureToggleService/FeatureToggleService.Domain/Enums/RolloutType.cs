namespace FeatureToggleService.Domain.Enums;

/// <summary>
/// Type of rollout strategy
/// </summary>
public enum RolloutType
{
    /// <summary>Percentage-based rollout</summary>
    Percentage = 0,

    /// <summary>User ID-based targeting</summary>
    UserIds = 1,

    /// <summary>Tenant/Organization-based targeting</summary>
    Tenants = 2,

    /// <summary>User group or role-based targeting</summary>
    Groups = 3,

    /// <summary>Geographic region-based targeting</summary>
    Regions = 4,

    /// <summary>Custom attribute-based targeting</summary>
    CustomAttributes = 5
}
