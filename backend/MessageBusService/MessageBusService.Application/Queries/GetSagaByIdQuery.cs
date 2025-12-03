using MediatR;
using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Queries;

/// <summary>
/// Query to get saga by ID
/// </summary>
public class GetSagaByIdQuery : IRequest<Saga?>
{
    public Guid SagaId { get; set; }
}
