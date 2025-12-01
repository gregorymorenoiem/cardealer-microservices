using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Audit;

/// <summary>
/// Event published when a compliance-related event is recorded.
/// </summary>
public class ComplianceEventRecordedEvent : EventBase
{
    public override string EventType => "audit.compliance.recorded";

    public Guid ComplianceId { get; set; }
    public string ComplianceType { get; set; } = string.Empty; // GDPR, SOC2, ISO27001
    public string EventDescription { get; set; } = string.Empty;
    public Guid RelatedUserId { get; set; }
    public DateTime RecordedAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
