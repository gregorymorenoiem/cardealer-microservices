using MediatR;
using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Commands;

/// <summary>
/// Command to retry a failed saga step
/// </summary>
public class RetrySagaStepCommand : IRequest<Saga>
{
    public Guid SagaId { get; set; }
    public Guid StepId { get; set; }
}
