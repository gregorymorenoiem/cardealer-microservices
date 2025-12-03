namespace MessageBusService.Domain.Enums;

/// <summary>
/// Type of Saga pattern implementation
/// </summary>
public enum SagaType
{
    /// <summary>
    /// Orchestration-based saga (centralized coordinator)
    /// </summary>
    Orchestration = 0,

    /// <summary>
    /// Choreography-based saga (event-driven, decentralized)
    /// </summary>
    Choreography = 1
}
