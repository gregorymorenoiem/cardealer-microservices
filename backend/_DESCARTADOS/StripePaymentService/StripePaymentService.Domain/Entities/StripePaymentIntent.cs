namespace StripePaymentService.Domain.Entities;

/// <summary>
/// Representa una Payment Intent de Stripe
/// </summary>
public class StripePaymentIntent
{
    /// <summary>
    /// ID interno en OKLA
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del Payment Intent en Stripe
    /// </summary>
    public string StripePaymentIntentId { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Monto en centavos (para evitar decimales)
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Moneda (usd, dop, etc.)
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Estado del Payment Intent
    /// </summary>
    public string Status { get; set; } = "requires_payment_method";

    /// <summary>
    /// Método de pago seleccionado
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la transacción
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Razón de cancelación (si aplica)
    /// </summary>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Información del cliente
    /// </summary>
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

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
    /// Fecha de completación
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Si requiere acción adicional del usuario (3D Secure, etc.)
    /// </summary>
    public bool RequiresAction { get; set; }

    /// <summary>
    /// URL para completar la acción requerida
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Respuesta JSON completa de Stripe
    /// </summary>
    public string? RawStripeResponse { get; set; }
}
