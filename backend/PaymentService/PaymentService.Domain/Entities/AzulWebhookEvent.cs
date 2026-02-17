namespace PaymentService.Domain.Entities;

/// <summary>
/// Representa un evento de webhook recibido desde AZUL
/// </summary>
public class AzulWebhookEvent
{
    /// <summary>
    /// ID único del evento en OKLA
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tipo de evento recibido
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    // transaction.approved, transaction.declined, transaction.pending, 
    // subscription.created, subscription.charged, subscription.failed, etc.

    /// <summary>
    /// ID de la transacción relacionada (si aplica)
    /// </summary>
    public Guid? TransactionId { get; set; }

    /// <summary>
    /// ID de la suscripción relacionada (si aplica)
    /// </summary>
    public Guid? SubscriptionId { get; set; }

    /// <summary>
    /// ID del evento en AZUL
    /// </summary>
    public string? AzulEventId { get; set; }

    /// <summary>
    /// Payload completo del webhook (JSON)
    /// </summary>
    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>
    /// Firma/Signature de validación de AZUL
    /// </summary>
    public string? Signature { get; set; }

    /// <summary>
    /// Si el webhook fue validado correctamente
    /// </summary>
    public bool IsValidated { get; set; }

    /// <summary>
    /// Si el webhook fue procesado
    /// </summary>
    public bool IsProcessed { get; set; }

    /// <summary>
    /// Resultado del procesamiento
    /// </summary>
    public string? ProcessingResult { get; set; }

    /// <summary>
    /// Error durante procesamiento (si aplica)
    /// </summary>
    public string? ProcessingError { get; set; }

    /// <summary>
    /// Timestamp de recepción del webhook
    /// </summary>
    public DateTime ReceivedAt { get; set; }

    /// <summary>
    /// Timestamp de procesamiento
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// IP desde donde se envió el webhook
    /// </summary>
    public string? SenderIpAddress { get; set; }

    /// <summary>
    /// User-Agent del cliente que envía el webhook
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Número de intentos de procesamiento
    /// </summary>
    public int ProcessingAttempts { get; set; }

    /// <summary>
    /// Timestamp del último intento de procesamiento
    /// </summary>
    public DateTime? LastProcessingAttempt { get; set; }
}
