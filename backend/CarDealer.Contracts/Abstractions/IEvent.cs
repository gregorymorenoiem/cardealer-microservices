namespace CarDealer.Contracts.Abstractions;

/// <summary>
/// Base interface for all domain events in the system.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// UTC timestamp when the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
    
    /// <summary>
    /// Type identifier for the event (e.g., "auth.user.registered").
    /// Used for routing and filtering in message broker.
    /// </summary>
    string EventType { get; }
}
