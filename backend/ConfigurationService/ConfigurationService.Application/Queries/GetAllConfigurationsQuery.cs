using ConfigurationService.Domain.Entities;
using MediatR;

namespace ConfigurationService.Application.Queries;

public record GetAllConfigurationsQuery(
    string Environment,
    string? TenantId = null
) : IRequest<IEnumerable<ConfigurationItem>>;
