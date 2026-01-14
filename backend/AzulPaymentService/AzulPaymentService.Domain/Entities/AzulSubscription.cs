using AzulPaymentService.Domain.Enums;

namespace AzulPaymentService.Domain.Entities;

/// <summary>
/// Representa una suscripción recurrente procesada por AZUL
/// </summary>
public class AzulSubscription
{
    /// <summary>
    /// ID único de la suscripción en OKLA
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID de suscripción en AZUL (referencia externa)
    /// </summary>
    public string AzulSubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario suscriptor
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Monto de cada pago recurrente
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Moneda (DOP, USD, etc.)
    /// </summary>
    public string Currency { get; set; } = "DOP";

    /// <summary>
    /// Descripción de la suscripción
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Frecuencia de recurrencia
    /// </summary>
    public SubscriptionFrequency Frequency { get; set; }

    /// <summary>
    /// Fecha de inicio de la suscripción
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Fecha de finalización (null = indefinida)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Estado de la suscripción
    /// </summary>
    public string Status { get; set; } = "Active"; // Active, Paused, Cancelled, Failed

    /// <summary>
    /// Método de pago utilizado
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Token de la tarjeta para cobros recurrentes
    /// </summary>
    public string? CardToken { get; set; }

    /// <summary>
    /// Últimos 4 dígitos de la tarjeta
    /// </summary>
    public string? CardLastFour { get; set; }

    /// <summary>
    /// Email del suscriptor
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Teléfono del suscriptor
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// Número de cobros realizados
    /// </summary>
    public int ChargeCount { get; set; }

    /// <summary>
    /// Monto total cobrado
    /// </summary>
    public decimal TotalAmountCharged { get; set; }

    /// <summary>
    /// Fecha del próximo cobro
    /// </summary>
    public DateTime? NextChargeDate { get; set; }

    /// <summary>
    /// Fecha del último cobro exitoso
    /// </summary>
    public DateTime? LastChargeDate { get; set; }

    /// <summary>
    /// Timestamp de creación
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp de última actualización
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Razón de cancelación (si aplica)
    /// </summary>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Timestamp de cancelación
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Nombre del plan/servicio (ej: "Pro Dealer")
    /// </summary>
    public string? PlanName { get; set; }

    /// <summary>
    /// Referencia de la factura
    /// </summary>
    public string? InvoiceReference { get; set; }
}
