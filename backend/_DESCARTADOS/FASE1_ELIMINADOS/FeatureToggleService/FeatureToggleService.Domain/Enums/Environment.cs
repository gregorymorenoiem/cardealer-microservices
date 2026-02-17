namespace FeatureToggleService.Domain.Enums;

/// <summary>
/// Environment where the feature flag applies
/// </summary>
public enum Environment
{
    /// <summary>All environments</summary>
    All = 0,

    /// <summary>Development environment only</summary>
    Development = 1,

    /// <summary>Staging/QA environment only</summary>
    Staging = 2,

    /// <summary>Production environment only</summary>
    Production = 3
}
