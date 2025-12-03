using MediatR;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;
using MessageBusService.Domain.Enums;

namespace MessageBusService.Application.Commands;

public class StartSagaCommandHandler : IRequestHandler<StartSagaCommand, Saga>
{
    private readonly ISagaRepository _sagaRepository;
    private readonly ISagaOrchestrator _sagaOrchestrator;

    public StartSagaCommandHandler(ISagaRepository sagaRepository, ISagaOrchestrator sagaOrchestrator)
    {
        _sagaRepository = sagaRepository;
        _sagaOrchestrator = sagaOrchestrator;
    }

    public async Task<Saga> Handle(StartSagaCommand request, CancellationToken cancellationToken)
    {
        // Create saga entity
        var saga = new Saga
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Status = SagaStatus.Created,
            CreatedAt = DateTime.UtcNow,
            CorrelationId = string.IsNullOrEmpty(request.CorrelationId)
                ? Guid.NewGuid().ToString()
                : request.CorrelationId,
            Context = request.Context,
            Timeout = request.Timeout,
            CurrentStepIndex = 0
        };

        // Add steps
        foreach (var stepDef in request.Steps)
        {
            var step = new SagaStep
            {
                Id = Guid.NewGuid(),
                Name = stepDef.Name,
                ServiceName = stepDef.ServiceName,
                ActionType = stepDef.ActionType,
                ActionPayload = stepDef.ActionPayload,
                CompensationActionType = stepDef.CompensationActionType,
                CompensationPayload = stepDef.CompensationPayload,
                Status = SagaStepStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                MaxRetries = stepDef.MaxRetries,
                Timeout = stepDef.Timeout,
                Metadata = stepDef.Metadata
            };
            saga.AddStep(step);
        }

        // Persist saga
        await _sagaRepository.CreateAsync(saga, cancellationToken);

        // Start saga orchestration
        return await _sagaOrchestrator.StartSagaAsync(saga, cancellationToken);
    }
}
