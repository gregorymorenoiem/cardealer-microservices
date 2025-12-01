using ConfigurationService.Domain.Entities;
using MediatR;

namespace ConfigurationService.Application.Commands;

public record UpdateConfigurationCommand(
    Guid Id,
    string Value,
    string UpdatedBy,
    string? ChangeReason = null
) : IRequest<ConfigurationItem>;
