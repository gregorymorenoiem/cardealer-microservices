using MediatR;

namespace ConfigurationService.Application.Commands;

public record DeleteConfigurationCommand(Guid Id) : IRequest<bool>;
