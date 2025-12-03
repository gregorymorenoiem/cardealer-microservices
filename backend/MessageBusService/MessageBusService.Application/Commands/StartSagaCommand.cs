using MediatR;
using MessageBusService.Domain.Entities;
using MessageBusService.Domain.Enums;

namespace MessageBusService.Application.Commands;

/// <summary>
/// Command to start a new saga
/// </summary>
public class StartSagaCommand : IRequest<Saga>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SagaType Type { get; set; } = SagaType.Orchestration;
    public string CorrelationId { get; set; } = string.Empty;
    public Dictionary<string, string>? Context { get; set; }
    public TimeSpan? Timeout { get; set; }
    public List<SagaStepDefinition> Steps { get; set; } = new();
}

/// <summary>
/// Defines a saga step
/// </summary>
public class SagaStepDefinition
{
    public string Name { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ActionPayload { get; set; } = string.Empty;
    public string? CompensationActionType { get; set; }
    public string? CompensationPayload { get; set; }
    public int MaxRetries { get; set; } = 3;
    public TimeSpan? Timeout { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
