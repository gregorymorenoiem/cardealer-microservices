namespace CarDealer.Contracts.Abstractions;

/// <summary>
/// Base class for all domain events providing common properties.
/// </summary>
public abstract class EventBase : IEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// Automatically generated on creation.
    /// </summary>
    public Guid EventId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// UTC timestamp when the event occurred.
    /// Automatically set to current UTC time on creation.
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Type identifier for the event (e.g., "auth.user.registered").
    /// Must be implemented by derived classes.
    /// </summary>
    public abstract string EventType { get; }
}
