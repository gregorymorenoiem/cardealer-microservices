using MediatR;

namespace ConfigurationService.Application.Queries;

public record IsFeatureEnabledQuery(
    string Key,
    string? Environment = null,
    string? TenantId = null,
    string? UserId = null
) : IRequest<bool>;
