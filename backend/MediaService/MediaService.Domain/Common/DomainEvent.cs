using MediatR;

namespace MediaService.Domain.Common;

/// <summary>
/// Base class for domain events
/// </summary>
public abstract class DomainEvent : INotification
{
    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public string EventId { get; protected set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// User ID who triggered the event
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Service that raised the event
    /// </summary>
    public string ServiceName { get; set; } = "MediaService";

    /// <summary>
    /// Additional event data
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    protected DomainEvent()
    {
    }

    protected DomainEvent(string serviceName)
    {
        ServiceName = serviceName;
    }
}