namespace StripePaymentService.Domain.Entities;

/// <summary>
/// Representa una Subscription en Stripe
/// </summary>
public class StripeSubscription
{
    /// <summary>
    /// ID interno en OKLA
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del Customer en OKLA
    /// </summary>
    public Guid StripeCustomerId { get; set; }

    /// <summary>
    /// Alias for StripeCustomerId (for EF Core navigation)
    /// </summary>
    public Guid CustomerId
    {
        get => StripeCustomerId;
        set => StripeCustomerId = value;
    }

    /// <summary>
    /// Relación con StripeCustomer
    /// </summary>
    public StripeCustomer? Customer { get; set; }

    /// <summary>
    /// ID de la Subscription en Stripe
    /// </summary>
    public string StripeSubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// ID del Price en Stripe
    /// </summary>
    public string StripePriceId { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la suscripción
    /// </summary>
    public string Status { get; set; } = "active";

    /// <summary>
    /// Monto a cobrar
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Moneda
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Intervalo de facturación
    /// </summary>
    public string BillingInterval { get; set; } = "month";

    /// <summary>
    /// Fecha de inicio
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Próxima fecha de facturación
    /// </summary>
    public DateTime NextBillingDate { get; set; }

    /// <summary>
    /// Fecha de finalización (si aplica)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Razón de cancelación
    /// </summary>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Fecha de cancelación
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Alias for CancelledAt (American spelling)
    /// </summary>
    public DateTime? CanceledAt
    {
        get => CancelledAt;
        set => CancelledAt = value;
    }

    /// <summary>
    /// Billing cycle anchor date
    /// </summary>
    public DateTime? BillingCycleAnchor { get; set; }

    /// <summary>
    /// Total de facturas pagadas
    /// </summary>
    public long TotalPaid { get; set; }

    /// <summary>
    /// Número de facturas
    /// </summary>
    public int InvoiceCount { get; set; }

    /// <summary>
    /// Metadata personalizada
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Invoices de la suscripción
    /// </summary>
    public List<StripeInvoice> Invoices { get; set; } = new();
}
