using MediatR;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Queries;

public class GetSagasByStatusQueryHandler : IRequestHandler<GetSagasByStatusQuery, List<Saga>>
{
    private readonly ISagaRepository _sagaRepository;

    public GetSagasByStatusQueryHandler(ISagaRepository sagaRepository)
    {
        _sagaRepository = sagaRepository;
    }

    public async Task<List<Saga>> Handle(GetSagasByStatusQuery request, CancellationToken cancellationToken)
    {
        return await _sagaRepository.GetByStatusAsync(request.Status, cancellationToken);
    }
}
