using AuditService.Domain.Common;

namespace AuditService.Domain.Entities;

/// <summary>
/// Represents an audited event consumed from RabbitMQ.
/// Stores all events from all microservices for audit trail purposes.
/// </summary>
public class AuditEvent : EntityBase
{
    /// <summary>
    /// Unique identifier from the original event.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Type of the event (e.g., "user.registered", "vehicle.created", "media.uploaded").
    /// This is the routing key from RabbitMQ.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Source microservice that published the event (e.g., "AuthService", "VehicleService").
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Full JSON payload of the event for complete audit trail.
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the event was originally created (from event metadata).
    /// </summary>
    public DateTime EventTimestamp { get; set; }

    /// <summary>
    /// Timestamp when this event was consumed and persisted to audit database.
    /// </summary>
    public DateTime ConsumedAt { get; set; }

    /// <summary>
    /// Optional correlation ID for tracing related events across services.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Optional user ID associated with the event (if applicable).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Additional metadata as JSON (e.g., request context, IP address, user agent).
    /// </summary>
    public string? Metadata { get; set; }
}
