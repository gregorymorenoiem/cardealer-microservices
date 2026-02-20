namespace PaymentService.Domain.Entities;

/// <summary>
/// Factura generada tras un pago exitoso.
/// Cumple con requisitos DGII para NCF (Número de Comprobante Fiscal).
/// </summary>
public class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PaymentTransactionId { get; set; }
    public Guid UserId { get; set; }
    public Guid? DealerId { get; set; }

    // Invoice numbering (DGII-compliant)
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? Ncf { get; set; } // Número de Comprobante Fiscal (formato: B0100000001)

    // Amounts
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; } = 0.18m; // ITBIS 18%
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "DOP";

    // Exchange rate info (for multi-currency DGII compliance)
    public decimal? ExchangeRate { get; set; }
    public decimal? AmountInDop { get; set; }

    // Line items description
    public string Description { get; set; } = string.Empty;
    public string? LineItemsJson { get; set; } // JSON array of line items

    // Buyer info
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string? BuyerRnc { get; set; } // RNC o Cédula
    public string? BuyerAddress { get; set; }
    public string? BuyerPhone { get; set; }

    // Seller/Business info
    public string SellerName { get; set; } = "OKLA SRL";
    public string SellerRnc { get; set; } = string.Empty;
    public string SellerAddress { get; set; } = string.Empty;

    // Status
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    // PDF storage
    public string? PdfUrl { get; set; }
    public string? PdfStorageKey { get; set; }

    // Timestamps
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Audit
    public string? CorrelationId { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public PaymentTransaction? PaymentTransaction { get; set; }
}

public enum InvoiceStatus
{
    Draft = 0,
    Issued = 1,
    Sent = 2,
    Paid = 3,
    Cancelled = 4,
    Voided = 5
}
