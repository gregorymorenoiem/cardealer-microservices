namespace PaymentService.Application.DTOs;

/// <summary>
/// DTO para webhook event de AZUL
/// </summary>
public class WebhookEventDto
{
    /// <summary>
    /// Tipo de evento (transaction.approved, subscription.charged, etc.)
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// ID del evento en AZUL
    /// </summary>
    public string AzulEventId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp del evento
    /// </summary>
    public DateTime EventTimestamp { get; set; }

    /// <summary>
    /// ID de la transacción relacionada (si aplica)
    /// </summary>
    public Guid? TransactionId { get; set; }

    /// <summary>
    /// ID de la suscripción relacionada (si aplica)
    /// </summary>
    public Guid? SubscriptionId { get; set; }

    /// <summary>
    /// ID externo de transacción en AZUL (si aplica)
    /// </summary>
    public string? AzulTransactionId { get; set; }

    /// <summary>
    /// ID externo de suscripción en AZUL (si aplica)
    /// </summary>
    public string? AzulSubscriptionId { get; set; }

    /// <summary>
    /// Payload completo del webhook
    /// </summary>
    public object? Payload { get; set; }

    /// <summary>
    /// Firma de validación
    /// </summary>
    public string? Signature { get; set; }

    /// <summary>
    /// Si fue validada la firma
    /// </summary>
    public bool IsSignatureValid { get; set; }

    /// <summary>
    /// IP del servidor que envió el webhook
    /// </summary>
    public string? SenderIpAddress { get; set; }

    /// <summary>
    /// User-Agent del request
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Timestamp de recepción
    /// </summary>
    public DateTime ReceivedAt { get; set; }
}
