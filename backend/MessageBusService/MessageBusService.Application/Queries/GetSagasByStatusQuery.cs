using MediatR;
using MessageBusService.Domain.Entities;
using MessageBusService.Domain.Enums;

namespace MessageBusService.Application.Queries;

/// <summary>
/// Query to get sagas by status
/// </summary>
public class GetSagasByStatusQuery : IRequest<List<Saga>>
{
    public SagaStatus Status { get; set; }
}
