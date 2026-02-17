using ConfigurationService.Domain.Entities;
using MediatR;

namespace ConfigurationService.Application.Commands;

public record UpdateFeatureFlagCommand(
    Guid Id,
    bool IsEnabled,
    int? RolloutPercentage = null
) : IRequest<FeatureFlag>;

public record DeleteFeatureFlagCommand(Guid Id) : IRequest<bool>;
