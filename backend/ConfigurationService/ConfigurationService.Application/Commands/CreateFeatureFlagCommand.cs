using ConfigurationService.Domain.Entities;
using MediatR;

namespace ConfigurationService.Application.Commands;

public record CreateFeatureFlagCommand(
    string Name,
    string Key,
    bool IsEnabled,
    string? Description = null,
    string? Environment = null,
    string? TenantId = null,
    int RolloutPercentage = 100,
    string? CreatedBy = null
) : IRequest<FeatureFlag>;
