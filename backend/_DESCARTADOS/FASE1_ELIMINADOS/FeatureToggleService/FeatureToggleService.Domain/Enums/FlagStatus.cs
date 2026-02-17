namespace FeatureToggleService.Domain.Enums;

/// <summary>
/// Status of a feature flag
/// </summary>
public enum FlagStatus
{
    /// <summary>Flag is in draft mode, not yet active</summary>
    Draft = 0,

    /// <summary>Flag is active and enabled</summary>
    Active = 1,

    /// <summary>Flag is inactive/disabled</summary>
    Inactive = 2,

    /// <summary>Flag uses rollout strategy for gradual enablement</summary>
    Rollout = 3,

    /// <summary>Flag is in A/B test mode</summary>
    ABTest = 4,

    /// <summary>Flag is archived (soft deleted)</summary>
    Archived = 5
}
