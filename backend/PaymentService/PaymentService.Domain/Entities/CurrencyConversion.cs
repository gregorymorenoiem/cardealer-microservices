using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Registro de conversión de moneda para una transacción específica
/// Requerido por DGII para auditoría fiscal - Norma General 06-2018
/// Cada transacción en moneda extranjera debe tener un registro de conversión
/// </summary>
public class CurrencyConversion
{
    /// <summary>
    /// ID único de la conversión
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID de la transacción de pago asociada
    /// </summary>
    public Guid PaymentTransactionId { get; set; }

    /// <summary>
    /// ID de la tasa de cambio utilizada
    /// </summary>
    public Guid ExchangeRateId { get; set; }

    /// <summary>
    /// Moneda original de la transacción (USD, EUR, etc.)
    /// </summary>
    public string OriginalCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Monto original en moneda extranjera
    /// </summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>
    /// Monto convertido a DOP
    /// </summary>
    public decimal ConvertedAmountDop { get; set; }

    /// <summary>
    /// Tasa de cambio aplicada (copia para auditoría)
    /// </summary>
    public decimal AppliedRate { get; set; }

    /// <summary>
    /// Fecha de la tasa de cambio usada
    /// </summary>
    public DateOnly RateDate { get; set; }

    /// <summary>
    /// Fuente de la tasa (para auditoría)
    /// </summary>
    public ExchangeRateSource RateSource { get; set; }

    /// <summary>
    /// Tipo de conversión
    /// </summary>
    public ConversionType ConversionType { get; set; }

    /// <summary>
    /// ITBIS (18%) calculado sobre el monto en DOP
    /// Requerido por DGII
    /// </summary>
    public decimal ItbisDop { get; set; }

    /// <summary>
    /// Monto total con ITBIS en DOP
    /// </summary>
    public decimal TotalWithItbisDop { get; set; }

    /// <summary>
    /// NCF asociado (si ya fue generado)
    /// </summary>
    public string? Ncf { get; set; }

    /// <summary>
    /// Fecha de emisión del NCF
    /// </summary>
    public DateTime? NcfIssuedAt { get; set; }

    /// <summary>
    /// Notas adicionales (para auditoría)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Referencia a la tasa de cambio
    /// </summary>
    public virtual ExchangeRate? ExchangeRate { get; set; }

    // ==================== MÉTODOS DE CÁLCULO ====================

    /// <summary>
    /// Calcula el ITBIS (18%) sobre el monto en DOP
    /// </summary>
    public void CalculateItbis()
    {
        const decimal ITBIS_RATE = 0.18m;
        ItbisDop = Math.Round(ConvertedAmountDop * ITBIS_RATE, 2, MidpointRounding.AwayFromZero);
        TotalWithItbisDop = ConvertedAmountDop + ItbisDop;
    }

    /// <summary>
    /// Crea un registro de conversión a partir de una transacción y tasa
    /// </summary>
    public static CurrencyConversion Create(
        Guid transactionId,
        ExchangeRate rate,
        decimal originalAmount,
        string originalCurrency,
        ConversionType conversionType)
    {
        var conversion = new CurrencyConversion
        {
            PaymentTransactionId = transactionId,
            ExchangeRateId = rate.Id,
            OriginalCurrency = originalCurrency,
            OriginalAmount = originalAmount,
            AppliedRate = rate.BuyRate,
            RateDate = rate.RateDate,
            RateSource = rate.Source,
            ConversionType = conversionType
        };

        // Calcular conversión según tipo
        conversion.ConvertedAmountDop = conversionType == ConversionType.Purchase
            ? rate.ConvertToDop(originalAmount)
            : rate.ConvertFromDop(originalAmount);

        // Calcular ITBIS
        conversion.CalculateItbis();

        return conversion;
    }
}

/// <summary>
/// Tipo de conversión de moneda
/// </summary>
public enum ConversionType
{
    /// <summary>
    /// Compra: Cliente paga en USD/EUR → Convertir a DOP
    /// </summary>
    Purchase = 1,

    /// <summary>
    /// Reembolso: Devolver en USD/EUR desde DOP
    /// </summary>
    Refund = 2,

    /// <summary>
    /// Cotización: Solo consulta, sin transacción
    /// </summary>
    Quote = 3
}
