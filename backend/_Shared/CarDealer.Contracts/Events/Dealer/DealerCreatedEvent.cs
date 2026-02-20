using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Dealer;

/// <summary>
/// Published when an admin approves a dealer registration.
/// NO PII — only IDs and metadata.
/// </summary>
public class DealerCreatedEvent : EventBase
{
    public override string EventType => "dealer.created";

    public Guid DealerId { get; set; }
    public Guid? OwnerUserId { get; set; }
    public DateTime ApprovedAt { get; set; }

    public static DealerCreatedEvent Create(Guid dealerId, Guid? ownerUserId)
    {
        return new DealerCreatedEvent
        {
            DealerId = dealerId,
            OwnerUserId = ownerUserId,
            ApprovedAt = DateTime.UtcNow
        };
    }
}
