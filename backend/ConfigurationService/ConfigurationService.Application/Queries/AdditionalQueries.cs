using ConfigurationService.Domain.Entities;
using MediatR;

namespace ConfigurationService.Application.Queries;

public record GetAllFeatureFlagsQuery(
    string? Environment = null
) : IRequest<IEnumerable<FeatureFlag>>;

public record GetConfigurationsByCategoryQuery(
    string Category,
    string Environment,
    string? TenantId = null
) : IRequest<IEnumerable<ConfigurationItem>>;

public record GetConfigurationHistoryQuery(
    Guid ConfigurationItemId
) : IRequest<IEnumerable<ConfigurationHistory>>;
