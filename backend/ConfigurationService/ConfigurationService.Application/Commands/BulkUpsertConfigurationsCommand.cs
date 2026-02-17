using ConfigurationService.Domain.Entities;
using MediatR;

namespace ConfigurationService.Application.Commands;

public record ConfigurationUpsertItem(
    string Key,
    string Value,
    string? Description = null
);

public record BulkUpsertConfigurationsCommand(
    string Environment,
    string UpdatedBy,
    List<ConfigurationUpsertItem> Items,
    string? ChangeReason = null
) : IRequest<IEnumerable<ConfigurationItem>>;
