using MediatR;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Commands;

public class AbortSagaCommandHandler : IRequestHandler<AbortSagaCommand, Saga>
{
    private readonly ISagaOrchestrator _sagaOrchestrator;

    public AbortSagaCommandHandler(ISagaOrchestrator sagaOrchestrator)
    {
        _sagaOrchestrator = sagaOrchestrator;
    }

    public async Task<Saga> Handle(AbortSagaCommand request, CancellationToken cancellationToken)
    {
        return await _sagaOrchestrator.AbortSagaAsync(request.SagaId, request.Reason, cancellationToken);
    }
}
