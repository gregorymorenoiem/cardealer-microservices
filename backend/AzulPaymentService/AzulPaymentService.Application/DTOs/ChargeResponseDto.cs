namespace AzulPaymentService.Application.DTOs;

/// <summary>
/// DTO para respuesta de cobro/transacción
/// </summary>
public class ChargeResponseDto
{
    /// <summary>
    /// ID interno de la transacción en OKLA
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// ID de la transacción en AZUL
    /// </summary>
    public string AzulTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la transacción
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Código de respuesta de AZUL
    /// </summary>
    public string? ResponseCode { get; set; }

    /// <summary>
    /// Mensaje de respuesta de AZUL
    /// </summary>
    public string? ResponseMessage { get; set; }

    /// <summary>
    /// Código de autorización (si aprobado)
    /// </summary>
    public string? AuthorizationCode { get; set; }

    /// <summary>
    /// Token de la tarjeta para futuras transacciones
    /// </summary>
    public string? CardToken { get; set; }

    /// <summary>
    /// Últimos 4 dígitos de la tarjeta
    /// </summary>
    public string? CardLast4 { get; set; }

    /// <summary>
    /// Marca de la tarjeta (Visa, Mastercard, etc.)
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Monto procesado
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Moneda
    /// </summary>
    public string Currency { get; set; } = "DOP";

    /// <summary>
    /// Fecha/hora de la transacción
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Si fue exitosa
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Razón del fallo (si aplica)
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Respuesta JSON completa de AZUL (para debugging)
    /// </summary>
    public object? RawAzulResponse { get; set; }
}
