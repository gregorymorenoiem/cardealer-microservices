namespace AzulPaymentService.Domain.Enums;

/// <summary>
/// Estado de una transacción de pago
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transacción pendiente de procesamiento
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Transacción aprobada y completada
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Transacción rechazada
    /// </summary>
    Declined = 2,

    /// <summary>
    /// Transacción anulada
    /// </summary>
    Voided = 3,

    /// <summary>
    /// Transacción reembolsada
    /// </summary>
    Refunded = 4,

    /// <summary>
    /// Transacción en error
    /// </summary>
    Error = 5,

    /// <summary>
    /// Transacción autorizada pero no capturada
    /// </summary>
    Authorized = 6,

    /// <summary>
    /// Transacción capturada
    /// </summary>
    Captured = 7
}
