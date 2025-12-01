using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Audit;

/// <summary>
/// Event published when an audit log entry is created.
/// </summary>
public class AuditLogCreatedEvent : EventBase
{
    public override string EventType => "audit.log.created";

    public Guid AuditId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? Changes { get; set; }
}
