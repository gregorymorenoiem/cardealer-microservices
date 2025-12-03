using MediatR;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Commands;

public class RetrySagaStepCommandHandler : IRequestHandler<RetrySagaStepCommand, Saga>
{
    private readonly ISagaOrchestrator _sagaOrchestrator;

    public RetrySagaStepCommandHandler(ISagaOrchestrator sagaOrchestrator)
    {
        _sagaOrchestrator = sagaOrchestrator;
    }

    public async Task<Saga> Handle(RetrySagaStepCommand request, CancellationToken cancellationToken)
    {
        return await _sagaOrchestrator.RetryStepAsync(request.SagaId, request.StepId, cancellationToken);
    }
}
