using ConfigurationService.Domain.Entities;
using MediatR;

namespace ConfigurationService.Application.Queries;

public record GetConfigurationQuery(
    string Key,
    string Environment,
    string? TenantId = null
) : IRequest<ConfigurationItem?>;
