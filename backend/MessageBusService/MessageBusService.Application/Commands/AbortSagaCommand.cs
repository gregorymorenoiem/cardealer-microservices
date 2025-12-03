using MediatR;
using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Commands;

/// <summary>
/// Command to abort a saga
/// </summary>
public class AbortSagaCommand : IRequest<Saga>
{
    public Guid SagaId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
