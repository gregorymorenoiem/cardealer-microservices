namespace AzulPaymentService.Application.DTOs;

/// <summary>
/// DTO para respuesta de suscripción
/// </summary>
public class SubscriptionResponseDto
{
    /// <summary>
    /// ID interno de la suscripción en OKLA
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// ID de la suscripción en AZUL
    /// </summary>
    public string AzulSubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario suscriptor
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Monto periódico
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Moneda
    /// </summary>
    public string Currency { get; set; } = "DOP";

    /// <summary>
    /// Frecuencia (Daily, Weekly, Monthly, etc.)
    /// </summary>
    public string Frequency { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la suscripción (Active, Paused, Cancelled, etc.)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de inicio
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Fecha de finalización
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Próxima fecha de cobro
    /// </summary>
    public DateTime? NextChargeDate { get; set; }

    /// <summary>
    /// Última fecha de cobro exitoso
    /// </summary>
    public DateTime? LastChargeDate { get; set; }

    /// <summary>
    /// Total de cargos realizados
    /// </summary>
    public int ChargeCount { get; set; }

    /// <summary>
    /// Monto total cargado
    /// </summary>
    public decimal TotalAmountCharged { get; set; }

    /// <summary>
    /// Últimos 4 dígitos de la tarjeta
    /// </summary>
    public string? CardLast4 { get; set; }

    /// <summary>
    /// Marca de la tarjeta
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Nombre del plan/producto
    /// </summary>
    public string? PlanName { get; set; }

    /// <summary>
    /// Si fue exitosa la creación
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Razón del fallo (si aplica)
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Respuesta JSON completa de AZUL
    /// </summary>
    public object? RawAzulResponse { get; set; }
}
