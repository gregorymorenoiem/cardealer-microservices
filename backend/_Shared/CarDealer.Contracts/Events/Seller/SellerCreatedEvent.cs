using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Seller;

/// <summary>
/// Event published when a seller account is fully created and active.
/// Does NOT include raw PII — only reference IDs.
/// </summary>
public class SellerCreatedEvent : EventBase
{
    public override string EventType => "seller.created";

    public Guid UserId { get; set; }
    public Guid SellerProfileId { get; set; }
    public string Source { get; set; } = string.Empty;
    public string AccountType { get; set; } = "Seller";
    public DateTime CreatedAt { get; set; }

    public static SellerCreatedEvent Create(
        Guid userId,
        Guid sellerProfileId,
        string source)
    {
        return new SellerCreatedEvent
        {
            UserId = userId,
            SellerProfileId = sellerProfileId,
            Source = source,
            CreatedAt = DateTime.UtcNow
        };
    }
}
