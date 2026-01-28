using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Representa una transacción de pago procesada por cualquier pasarela
/// Reemplaza AzulTransaction para soportar múltiples proveedores
/// </summary>
public class PaymentTransaction
{
    /// <summary>
    /// ID único de la transacción en OKLA
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID de transacción en la pasarela de pago (referencia externa)
    /// </summary>
    public string ExternalTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Pasarela de pago utilizada
    /// </summary>
    public PaymentGateway PaymentGateway { get; set; }

    /// <summary>
    /// ID del usuario que realiza la transacción
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Monto de la transacción (en la moneda especificada)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Moneda (DOP, USD, etc.)
    /// </summary>
    public string Currency { get; set; } = "DOP";

    /// <summary>
    /// Descripción de la transacción
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual de la transacción
    /// </summary>
    public TransactionStatus Status { get; set; }

    /// <summary>
    /// Método de pago utilizado (Tarjeta, ACH, Móvil, etc.)
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Últimos 4 dígitos de la tarjeta (si aplica)
    /// </summary>
    public string? CardLastFour { get; set; }

    /// <summary>
    /// Marca de la tarjeta (Visa, Mastercard, Amex, etc.)
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Tipo de transacción (Sale, Authorize, Capture, Void, Refund)
    /// </summary>
    public string TransactionType { get; set; } = "Sale";

    /// <summary>
    /// Código de respuesta de la pasarela
    /// </summary>
    public string? ResponseCode { get; set; }

    /// <summary>
    /// Mensaje de respuesta de la pasarela
    /// </summary>
    public string? ResponseMessage { get; set; }

    /// <summary>
    /// Token de la tarjeta (para recurrentes)
    /// </summary>
    public string? CardToken { get; set; }

    /// <summary>
    /// Número de aprobación de la pasarela
    /// </summary>
    public string? AuthorizationCode { get; set; }

    /// <summary>
    /// Comisión cobrada por la pasarela (en monto absoluto)
    /// </summary>
    public decimal? Commission { get; set; }

    /// <summary>
    /// Porcentaje de comisión aplicado
    /// </summary>
    public decimal? CommissionPercentage { get; set; }

    /// <summary>
    /// Monto neto recibido (Amount - Commission)
    /// </summary>
    public decimal? NetAmount { get; set; }

    /// <summary>
    /// Clave de idempotencia para evitar duplicados
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Metadata adicional de la transacción (JSON)
    /// Puede contener información específica de la pasarela
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Fecha y hora de creación de la transacción
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha y hora de procesamiento de la transacción
    /// </summary>
    public DateTime ProcessedAt { get; set; }

    /// <summary>
    /// Fecha y hora de última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID de la transacción original (para reembolsos)
    /// </summary>
    public Guid? RefundedFromTransactionId { get; set; }

    /// <summary>
    /// Referencia al usuario que procesó esta transacción
    /// </summary>
    public Guid? ProcessedByUserId { get; set; }

    /// <summary>
    /// Notas sobre la transacción (para auditoría)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Indicador de disputa o chargeback
    /// </summary>
    public bool IsDisputed { get; set; }

    /// <summary>
    /// Razón de disputa si aplica
    /// </summary>
    public string? DisputeReason { get; set; }
}
