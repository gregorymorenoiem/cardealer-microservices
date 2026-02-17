namespace StripePaymentService.Application.DTOs;

/// <summary>
/// DTO para webhook event de Stripe
/// </summary>
public class StripeWebhookEventDto
{
    /// <summary>
    /// ID del evento en Stripe
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de evento
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp del evento
    /// </summary>
    public long EventTimestamp { get; set; }

    /// <summary>
    /// Versión del API usado
    /// </summary>
    public string? ApiVersion { get; set; }

    /// <summary>
    /// Modo (test o live)
    /// </summary>
    public string? Mode { get; set; }

    /// <summary>
    /// ID del objeto relacionado
    /// </summary>
    public string? ObjectId { get; set; }

    /// <summary>
    /// Tipo del objeto
    /// </summary>
    public string? ObjectType { get; set; }

    /// <summary>
    /// Payload completo
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Si fue validado
    /// </summary>
    public bool IsValidated { get; set; }
}
