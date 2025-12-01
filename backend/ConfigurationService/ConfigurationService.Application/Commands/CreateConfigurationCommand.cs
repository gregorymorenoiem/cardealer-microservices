using ConfigurationService.Domain.Entities;
using MediatR;

namespace ConfigurationService.Application.Commands;

public record CreateConfigurationCommand(
    string Key,
    string Value,
    string Environment,
    string CreatedBy,
    string? Description = null,
    string? TenantId = null
) : IRequest<ConfigurationItem>;
