using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Seller;

/// <summary>
/// Event published when a buyer requests conversion to seller.
/// Does NOT include raw PII — only reference IDs.
/// </summary>
public class SellerConversionRequestedEvent : EventBase
{
    public override string EventType => "seller.conversion.requested";

    public Guid UserId { get; set; }
    public Guid ConversionId { get; set; }
    public Guid SellerProfileId { get; set; }
    public string Source { get; set; } = "conversion";
    public string PreviousAccountType { get; set; } = string.Empty;
    public Guid? KycProfileId { get; set; }
    public DateTime RequestedAt { get; set; }

    public static SellerConversionRequestedEvent Create(
        Guid userId,
        Guid conversionId,
        Guid sellerProfileId,
        string source,
        string previousAccountType,
        Guid? kycProfileId = null)
    {
        return new SellerConversionRequestedEvent
        {
            UserId = userId,
            ConversionId = conversionId,
            SellerProfileId = sellerProfileId,
            Source = source,
            PreviousAccountType = previousAccountType,
            KycProfileId = kycProfileId,
            RequestedAt = DateTime.UtcNow
        };
    }
}
