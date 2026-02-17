using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using MediatR;
using Environment = FeatureToggleService.Domain.Enums.Environment;

namespace FeatureToggleService.Application.Queries;

/// <summary>
/// Query to get a feature flag by ID
/// </summary>
public record GetFeatureFlagByIdQuery(Guid Id) : IRequest<FeatureFlag?>;

/// <summary>
/// Query to get a feature flag by key
/// </summary>
public record GetFeatureFlagByKeyQuery(string Key) : IRequest<FeatureFlag?>;

/// <summary>
/// Query to get all feature flags
/// </summary>
public record GetAllFeatureFlagsQuery() : IRequest<IEnumerable<FeatureFlag>>;

/// <summary>
/// Query to get feature flags by environment
/// </summary>
public record GetFeatureFlagsByEnvironmentQuery(Environment Environment) : IRequest<IEnumerable<FeatureFlag>>;

/// <summary>
/// Query to get feature flags by status
/// </summary>
public record GetFeatureFlagsByStatusQuery(FlagStatus Status) : IRequest<IEnumerable<FeatureFlag>>;

/// <summary>
/// Query to get feature flags by tag
/// </summary>
public record GetFeatureFlagsByTagQuery(string Tag) : IRequest<IEnumerable<FeatureFlag>>;

/// <summary>
/// Query to get active feature flags
/// </summary>
public record GetActiveFeatureFlagsQuery() : IRequest<IEnumerable<FeatureFlag>>;

/// <summary>
/// Query to get expired feature flags
/// </summary>
public record GetExpiredFeatureFlagsQuery() : IRequest<IEnumerable<FeatureFlag>>;

/// <summary>
/// Query to evaluate a feature flag for a context
/// </summary>
public record EvaluateFeatureFlagQuery(string FlagKey, EvaluationContext? Context = null) : IRequest<bool>;

/// <summary>
/// Query to evaluate multiple feature flags
/// </summary>
public record EvaluateMultipleFeatureFlagsQuery(List<string> FlagKeys, EvaluationContext? Context = null) : IRequest<Dictionary<string, bool>>;

/// <summary>
/// Query to get feature flag history
/// </summary>
public record GetFeatureFlagHistoryQuery(Guid FlagId) : IRequest<IEnumerable<FeatureFlagHistory>>;

/// <summary>
/// Query to get feature flag statistics
/// </summary>
public record GetFeatureFlagStatsQuery() : IRequest<FeatureFlagStats>;

/// <summary>
/// Feature flag statistics
/// </summary>
public class FeatureFlagStats
{
    public int TotalFlags { get; set; }
    public int ActiveFlags { get; set; }
    public int InactiveFlags { get; set; }
    public int DraftFlags { get; set; }
    public int ArchivedFlags { get; set; }
    public int ExpiredFlags { get; set; }
    public int KillSwitchActiveFlags { get; set; }
    public Dictionary<string, int> FlagsByEnvironment { get; set; } = new();
    public Dictionary<string, int> FlagsByTag { get; set; } = new();
}
