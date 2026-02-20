using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Billing;

/// <summary>
/// Evento emitido cuando un seller/dealer compra créditos de publicación.
/// </summary>
public class PublicationCreditsPurchasedEvent : EventBase
{
    public override string EventType => "billing.credits.purchased";

    public Guid UserId { get; set; }
    public Guid? DealerId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public int CreditsAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public string Currency { get; set; } = "DOP";
    public Guid PaymentTransactionId { get; set; }
    public DateTime PurchasedAt { get; set; }
}
