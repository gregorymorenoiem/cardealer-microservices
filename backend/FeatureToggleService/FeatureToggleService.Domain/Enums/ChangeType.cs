namespace FeatureToggleService.Domain.Enums;

/// <summary>
/// Type of change made to a feature flag
/// </summary>
public enum ChangeType
{
    /// <summary>Flag was created</summary>
    Created = 0,

    /// <summary>Flag was updated</summary>
    Updated = 1,

    /// <summary>Flag was enabled</summary>
    Enabled = 2,

    /// <summary>Flag was disabled</summary>
    Disabled = 3,

    /// <summary>Rollout percentage was changed</summary>
    RolloutChange = 4,

    /// <summary>Flag was archived</summary>
    Archived = 5,

    /// <summary>Flag was restored from archive</summary>
    Restored = 6,

    /// <summary>Targeting rules were modified</summary>
    TargetingChange = 7,

    /// <summary>Kill switch was triggered</summary>
    KillSwitch = 8
}
