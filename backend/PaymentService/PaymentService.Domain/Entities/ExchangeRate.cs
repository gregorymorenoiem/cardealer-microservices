using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Tasa de cambio oficial del Banco Central de la República Dominicana
/// Requerido por DGII: Todas las transacciones en moneda extranjera deben 
/// convertirse a DOP usando la tasa oficial del BCRD para fines fiscales
/// Norma General 06-2018 DGII
/// </summary>
public class ExchangeRate
{
    /// <summary>
    /// ID único del registro de tasa
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Fecha de la tasa (sin hora - una tasa por día)
    /// El BCRD publica tasas diariamente
    /// </summary>
    public DateOnly RateDate { get; set; }

    /// <summary>
    /// Moneda origen (USD, EUR, etc.)
    /// ISO 4217 currency code
    /// </summary>
    public string SourceCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Moneda destino (siempre DOP para cumplimiento DGII)
    /// </summary>
    public string TargetCurrency { get; set; } = "DOP";

    /// <summary>
    /// Tasa de compra (banco compra divisas)
    /// Usada cuando el cliente paga en moneda extranjera
    /// </summary>
    public decimal BuyRate { get; set; }

    /// <summary>
    /// Tasa de venta (banco vende divisas)
    /// Usada para reembolsos en moneda extranjera
    /// </summary>
    public decimal SellRate { get; set; }

    /// <summary>
    /// Fuente de la tasa (para auditoría DGII)
    /// </summary>
    public ExchangeRateSource Source { get; set; } = ExchangeRateSource.BancoCentralApi;

    /// <summary>
    /// Identificador de la consulta en el BCRD (para auditoría)
    /// </summary>
    public string? BcrdReferenceId { get; set; }

    /// <summary>
    /// Fecha y hora de obtención de la tasa
    /// </summary>
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si esta es la tasa activa para la fecha
    /// Solo una tasa por moneda/fecha puede estar activa
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Metadata adicional del BCRD (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Fecha de creación del registro
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ==================== MÉTODOS DE CONVERSIÓN ====================

    /// <summary>
    /// Convierte un monto de la moneda origen a DOP usando la tasa de compra
    /// Usado para pagos entrantes en moneda extranjera
    /// </summary>
    /// <param name="amount">Monto en moneda extranjera</param>
    /// <returns>Monto equivalente en DOP</returns>
    public decimal ConvertToDop(decimal amount)
    {
        if (amount <= 0) return 0;
        return Math.Round(amount * BuyRate, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Convierte un monto de DOP a la moneda origen usando la tasa de venta
    /// Usado para reembolsos en moneda extranjera
    /// </summary>
    /// <param name="amountDop">Monto en DOP</param>
    /// <returns>Monto equivalente en moneda extranjera</returns>
    public decimal ConvertFromDop(decimal amountDop)
    {
        if (amountDop <= 0 || SellRate <= 0) return 0;
        return Math.Round(amountDop / SellRate, 2, MidpointRounding.AwayFromZero);
    }
}
