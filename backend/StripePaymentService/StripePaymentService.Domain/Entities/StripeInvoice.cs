namespace StripePaymentService.Domain.Entities;

/// <summary>
/// Representa una Invoice en Stripe
/// </summary>
public class StripeInvoice
{
    /// <summary>
    /// ID interno en OKLA
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID de la Suscripción
    /// </summary>
    public Guid StripeSubscriptionId { get; set; }

    /// <summary>
    /// Alias for StripeSubscriptionId (for EF Core navigation)
    /// </summary>
    public Guid SubscriptionId
    {
        get => StripeSubscriptionId;
        set => StripeSubscriptionId = value;
    }

    /// <summary>
    /// Relación con StripeSubscription
    /// </summary>
    public StripeSubscription? Subscription { get; set; }

    /// <summary>
    /// ID del Invoice en Stripe
    /// </summary>
    public string StripeInvoiceId { get; set; } = string.Empty;

    /// <summary>
    /// Número de factura
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Estado del Invoice
    /// </summary>
    public string Status { get; set; } = "draft";

    /// <summary>
    /// Monto total
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Moneda
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Moneda formateada (para display)
    /// </summary>
    public string FormattedAmount => $"{(Amount / 100.0):C}";

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de vencimiento
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Fecha de pago
    /// </summary>
    public DateTime? PaidDate { get; set; }

    /// <summary>
    /// URL de descarga del PDF
    /// </summary>
    public string? PdfUrl { get; set; }

    /// <summary>
    /// URL para pagar
    /// </summary>
    public string? HostedInvoiceUrl { get; set; }

    /// <summary>
    /// Descripción personalizada
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Metadata personalizada
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
