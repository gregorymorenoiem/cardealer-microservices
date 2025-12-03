using MediatR;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Queries;

public class GetSagaByIdQueryHandler : IRequestHandler<GetSagaByIdQuery, Saga?>
{
    private readonly ISagaRepository _sagaRepository;

    public GetSagaByIdQueryHandler(ISagaRepository sagaRepository)
    {
        _sagaRepository = sagaRepository;
    }

    public async Task<Saga?> Handle(GetSagaByIdQuery request, CancellationToken cancellationToken)
    {
        return await _sagaRepository.GetByIdAsync(request.SagaId, cancellationToken);
    }
}
