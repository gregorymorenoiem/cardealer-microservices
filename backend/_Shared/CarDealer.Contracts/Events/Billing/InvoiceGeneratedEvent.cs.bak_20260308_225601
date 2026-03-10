using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Billing;

/// <summary>
/// Evento emitido cuando una factura es generada y lista para envío.
/// </summary>
public class InvoiceGeneratedEvent : EventBase
{
    public override string EventType => "billing.invoice.generated";

    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid PaymentTransactionId { get; set; }
    public Guid UserId { get; set; }
    public Guid? DealerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "DOP";
    public string Description { get; set; } = string.Empty;
    public string? PdfUrl { get; set; }
    public DateTime IssuedAt { get; set; }
}
