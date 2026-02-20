using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Dealer;

/// <summary>
/// Published when a new dealer (company) registration is submitted.
/// NO PII — only IDs and metadata.
/// </summary>
public class DealerRegistrationRequestedEvent : EventBase
{
    public override string EventType => "dealer.registration.requested";

    public Guid DealerId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid? OwnerUserId { get; set; }
    public DateTime RequestedAt { get; set; }

    public static DealerRegistrationRequestedEvent Create(
        Guid dealerId, string companyName, Guid? ownerUserId)
    {
        return new DealerRegistrationRequestedEvent
        {
            DealerId = dealerId,
            CompanyName = companyName,
            OwnerUserId = ownerUserId,
            RequestedAt = DateTime.UtcNow
        };
    }
}
