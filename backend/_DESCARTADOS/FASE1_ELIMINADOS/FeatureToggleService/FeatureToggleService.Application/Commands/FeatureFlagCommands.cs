using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using MediatR;
using Environment = FeatureToggleService.Domain.Enums.Environment;

namespace FeatureToggleService.Application.Commands;

/// <summary>
/// Command to create a new feature flag
/// </summary>
public record CreateFeatureFlagCommand(
    string Key,
    string Name,
    string Description,
    Environment Environment,
    List<string> Tags,
    string CreatedBy
) : IRequest<FeatureFlag>;

/// <summary>
/// Command to update a feature flag
/// </summary>
public record UpdateFeatureFlagCommand(
    Guid Id,
    string? Name = null,
    string? Description = null,
    List<string>? Tags = null,
    Environment? Environment = null,
    DateTime? ExpiresAt = null,
    string? ModifiedBy = null
) : IRequest<FeatureFlag?>;

/// <summary>
/// Command to delete a feature flag
/// </summary>
public record DeleteFeatureFlagCommand(Guid Id) : IRequest<bool>;

/// <summary>
/// Command to enable a feature flag
/// </summary>
public record EnableFeatureFlagCommand(Guid Id, string ModifiedBy) : IRequest<FeatureFlag?>;

/// <summary>
/// Command to disable a feature flag
/// </summary>
public record DisableFeatureFlagCommand(Guid Id, string ModifiedBy) : IRequest<FeatureFlag?>;

/// <summary>
/// Command to archive a feature flag
/// </summary>
public record ArchiveFeatureFlagCommand(Guid Id, string ModifiedBy) : IRequest<FeatureFlag?>;

/// <summary>
/// Command to restore an archived feature flag
/// </summary>
public record RestoreFeatureFlagCommand(Guid Id, string ModifiedBy) : IRequest<FeatureFlag?>;

/// <summary>
/// Command to trigger the kill switch on a feature flag
/// </summary>
public record TriggerKillSwitchCommand(Guid Id, string TriggeredBy, string Reason) : IRequest<FeatureFlag?>;

/// <summary>
/// Command to reset the kill switch on a feature flag
/// </summary>
public record ResetKillSwitchCommand(Guid Id, string ModifiedBy) : IRequest<FeatureFlag?>;

/// <summary>
/// Command to set rollout percentage for a feature flag
/// </summary>
public record SetRolloutPercentageCommand(Guid Id, int Percentage, string ModifiedBy) : IRequest<FeatureFlag?>;

/// <summary>
/// Command to add users to rollout targeting
/// </summary>
public record AddTargetUsersCommand(Guid Id, List<string> UserIds, string ModifiedBy) : IRequest<FeatureFlag?>;

/// <summary>
/// Command to remove users from rollout targeting
/// </summary>
public record RemoveTargetUsersCommand(Guid Id, List<string> UserIds, string ModifiedBy) : IRequest<FeatureFlag?>;
