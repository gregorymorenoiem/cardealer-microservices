namespace AzulPaymentService.Application.DTOs;

/// <summary>
/// DTO para solicitud de reembolso
/// </summary>
public class RefundRequestDto
{
    /// <summary>
    /// ID interno de la transacción a reembolsar
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Monto a reembolsar (null = reembolso completo)
    /// </summary>
    public decimal? PartialAmount { get; set; }

    /// <summary>
    /// Razón del reembolso
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Notas adicionales
    /// </summary>
    public string? Notes { get; set; }
}
