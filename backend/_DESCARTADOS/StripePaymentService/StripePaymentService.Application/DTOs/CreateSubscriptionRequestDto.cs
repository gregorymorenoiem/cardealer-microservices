namespace StripePaymentService.Application.DTOs;

/// <summary>
/// DTO para crear una suscripción
/// </summary>
public class CreateSubscriptionRequestDto
{
    /// <summary>
    /// ID del Customer en Stripe
    /// </summary>
    public string StripeCustomerId { get; set; } = string.Empty;

    /// <summary>
    /// ID del Price en Stripe
    /// </summary>
    public string StripePriceId { get; set; } = string.Empty;

    /// <summary>
    /// Descripción
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Metadata personalizada
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Days trial (días de prueba)
    /// </summary>
    public int? TrialDays { get; set; }

    /// <summary>
    /// Fecha de inicio
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Método de pago para usar
    /// </summary>
    public string? PaymentMethodId { get; set; }

    /// <summary>
    /// Si debe cobrar ahora mismo
    /// </summary>
    public bool BillingCycleAnchor { get; set; }
}
