using MediatR;
using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Commands;

/// <summary>
/// Command to compensate a failed saga
/// </summary>
public class CompensateSagaCommand : IRequest<Saga>
{
    public Guid SagaId { get; set; }
}
