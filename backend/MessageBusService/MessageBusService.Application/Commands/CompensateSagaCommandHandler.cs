using MediatR;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Commands;

public class CompensateSagaCommandHandler : IRequestHandler<CompensateSagaCommand, Saga>
{
    private readonly ISagaOrchestrator _sagaOrchestrator;

    public CompensateSagaCommandHandler(ISagaOrchestrator sagaOrchestrator)
    {
        _sagaOrchestrator = sagaOrchestrator;
    }

    public async Task<Saga> Handle(CompensateSagaCommand request, CancellationToken cancellationToken)
    {
        return await _sagaOrchestrator.CompensateSagaAsync(request.SagaId, cancellationToken);
    }
}
