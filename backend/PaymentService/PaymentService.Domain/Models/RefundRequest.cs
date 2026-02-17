namespace PaymentService.Domain.Models;

/// <summary>
/// Solicitud de reembolso para procesar a través del gateway
/// </summary>
public class RefundRequest
{
    /// <summary>
    /// ID de la transacción original a reembolsar
    /// </summary>
    public string OriginalTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Monto a reembolsar (puede ser parcial)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Moneda del reembolso
    /// </summary>
    public string Currency { get; set; } = "DOP";

    /// <summary>
    /// Razón del reembolso
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Referencia externa opcional
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// Metadata adicional
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Resultado del reembolso
/// </summary>
public class RefundResult
{
    /// <summary>
    /// Indica si el reembolso fue exitoso
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// ID de la transacción de reembolso en el gateway
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Código de respuesta del gateway
    /// </summary>
    public string? ResponseCode { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Monto reembolsado
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Fecha/hora del reembolso
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Respuesta raw del gateway (para debugging)
    /// </summary>
    public string? RawResponse { get; set; }
}
