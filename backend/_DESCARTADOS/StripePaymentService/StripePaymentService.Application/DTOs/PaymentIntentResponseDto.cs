namespace StripePaymentService.Application.DTOs;

/// <summary>
/// DTO para respuesta de Payment Intent
/// </summary>
public class PaymentIntentResponseDto
{
    /// <summary>
    /// ID interno en OKLA
    /// </summary>
    public Guid PaymentIntentId { get; set; }

    /// <summary>
    /// ID del Payment Intent en Stripe
    /// </summary>
    public string StripePaymentIntentId { get; set; } = string.Empty;

    /// <summary>
    /// Estado del Payment Intent
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Monto
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Moneda
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Client Secret para confirmar en frontend
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Si requiere acción adicional
    /// </summary>
    public bool RequiresAction { get; set; }

    /// <summary>
    /// Método de pago
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Descripción
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Mensaje de error (si aplica)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Si fue exitoso
    /// </summary>
    public bool IsSuccessful => Status == "succeeded";

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
