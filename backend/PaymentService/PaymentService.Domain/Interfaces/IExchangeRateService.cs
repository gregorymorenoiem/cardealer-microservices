using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Interfaces;

/// <summary>
/// Servicio de conversión de moneda con cumplimiento DGII
/// Usa tasas oficiales del Banco Central de la República Dominicana
/// </summary>
public interface IExchangeRateService
{
    /// <summary>
    /// Obtiene la tasa de cambio actual para una moneda
    /// Primero busca en caché, luego en BD, y si es necesario consulta el BCRD
    /// </summary>
    /// <param name="currency">Código de moneda (USD, EUR)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tasa de cambio actual</returns>
    Task<ExchangeRate> GetCurrentRateAsync(string currency, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la tasa de cambio para una fecha específica
    /// </summary>
    /// <param name="currency">Código de moneda</param>
    /// <param name="date">Fecha de la tasa</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tasa de cambio</returns>
    Task<ExchangeRate> GetRateForDateAsync(string currency, DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Convierte un monto de moneda extranjera a DOP
    /// Registra la conversión para auditoría DGII
    /// </summary>
    /// <param name="amount">Monto en moneda extranjera</param>
    /// <param name="fromCurrency">Código de moneda origen</param>
    /// <param name="transactionId">ID de la transacción (opcional, para auditoría)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la conversión con detalles</returns>
    Task<ConversionResult> ConvertToDopAsync(
        decimal amount,
        string fromCurrency,
        Guid? transactionId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convierte un monto de DOP a moneda extranjera (para reembolsos)
    /// </summary>
    Task<ConversionResult> ConvertFromDopAsync(
        decimal amountDop,
        string toCurrency,
        Guid? transactionId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una cotización sin registrar (para mostrar precios)
    /// </summary>
    Task<ConversionResult> GetQuoteAsync(
        decimal amount,
        string fromCurrency,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza las tasas de cambio desde el Banco Central
    /// Llamado por el job diario
    /// </summary>
    Task<bool> RefreshRatesFromBcrdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si el servicio de tasas está disponible
    /// </summary>
    Task<bool> IsServiceHealthyAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado de una conversión de moneda
/// </summary>
public class ConversionResult
{
    /// <summary>
    /// Indica si la conversión fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje de error si no fue exitosa
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Monto original en moneda extranjera
    /// </summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>
    /// Moneda original
    /// </summary>
    public string OriginalCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Monto convertido a DOP
    /// </summary>
    public decimal ConvertedAmountDop { get; set; }

    /// <summary>
    /// Tasa de cambio aplicada
    /// </summary>
    public decimal AppliedRate { get; set; }

    /// <summary>
    /// Fecha de la tasa
    /// </summary>
    public DateOnly RateDate { get; set; }

    /// <summary>
    /// Fuente de la tasa
    /// </summary>
    public string RateSource { get; set; } = string.Empty;

    /// <summary>
    /// ITBIS (18%) sobre el monto en DOP
    /// </summary>
    public decimal ItbisDop { get; set; }

    /// <summary>
    /// Total con ITBIS incluido
    /// </summary>
    public decimal TotalWithItbisDop { get; set; }

    /// <summary>
    /// ID de la tasa de cambio usada (para referencia)
    /// </summary>
    public Guid ExchangeRateId { get; set; }

    /// <summary>
    /// ID del registro de conversión (si se guardó para auditoría)
    /// </summary>
    public Guid? ConversionRecordId { get; set; }

    /// <summary>
    /// Crea un resultado exitoso
    /// </summary>
    public static ConversionResult Successful(
        decimal originalAmount,
        string originalCurrency,
        ExchangeRate rate,
        CurrencyConversion? conversion = null)
    {
        return new ConversionResult
        {
            Success = true,
            OriginalAmount = originalAmount,
            OriginalCurrency = originalCurrency,
            ConvertedAmountDop = rate.ConvertToDop(originalAmount),
            AppliedRate = rate.BuyRate,
            RateDate = rate.RateDate,
            RateSource = rate.Source.ToString(),
            ItbisDop = conversion?.ItbisDop ?? 0,
            TotalWithItbisDop = conversion?.TotalWithItbisDop ?? rate.ConvertToDop(originalAmount),
            ExchangeRateId = rate.Id,
            ConversionRecordId = conversion?.Id
        };
    }

    /// <summary>
    /// Crea un resultado fallido
    /// </summary>
    public static ConversionResult Failed(string errorMessage)
    {
        return new ConversionResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
