namespace StripePaymentService.Application.DTOs;

/// <summary>
/// DTO para crear un Payment Intent
/// </summary>
public class CreatePaymentIntentRequestDto
{
    /// <summary>
    /// Monto en centavos (para evitar decimales)
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Moneda (usd, dop, etc.)
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Descripción del pago
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// ID del Customer en Stripe (si existe)
    /// </summary>
    public string? StripeCustomerId { get; set; }

    /// <summary>
    /// Método de pago (si se preselecciona)
    /// </summary>
    public string? PaymentMethodId { get; set; }

    /// <summary>
    /// Metadata personalizada
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Si debe confirmar automáticamente
    /// </summary>
    public bool OffSession { get; set; }

    /// <summary>
    /// Indicador de riesgo para fraude
    /// </summary>
    public string? RiskLevel { get; set; }
}
