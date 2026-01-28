using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Interfaces;

/// <summary>
/// Repositorio para tasas de cambio del Banco Central RD
/// </summary>
public interface IExchangeRateRepository
{
    /// <summary>
    /// Obtiene la tasa de cambio activa para una moneda y fecha específica
    /// </summary>
    /// <param name="currency">Código de moneda (USD, EUR)</param>
    /// <param name="date">Fecha de la tasa</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tasa de cambio o null si no existe</returns>
    Task<ExchangeRate?> GetRateAsync(string currency, DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la tasa de cambio más reciente para una moneda
    /// </summary>
    /// <param name="currency">Código de moneda (USD, EUR)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tasa más reciente o null</returns>
    Task<ExchangeRate?> GetLatestRateAsync(string currency, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las tasas para una moneda en un rango de fechas
    /// </summary>
    /// <param name="currency">Código de moneda</param>
    /// <param name="startDate">Fecha inicio</param>
    /// <param name="endDate">Fecha fin</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de tasas</returns>
    Task<IReadOnlyList<ExchangeRate>> GetRatesInRangeAsync(
        string currency,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda una nueva tasa de cambio
    /// Desactiva automáticamente la tasa anterior para la misma moneda/fecha
    /// </summary>
    /// <param name="rate">Tasa a guardar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tasa guardada</returns>
    Task<ExchangeRate> SaveRateAsync(ExchangeRate rate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda múltiples tasas en una transacción
    /// </summary>
    /// <param name="rates">Tasas a guardar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task SaveRatesAsync(IEnumerable<ExchangeRate> rates, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe una tasa para una moneda y fecha
    /// </summary>
    Task<bool> ExistsAsync(string currency, DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las monedas disponibles con tasas activas
    /// </summary>
    Task<IReadOnlyList<string>> GetAvailableCurrenciesAsync(CancellationToken cancellationToken = default);
}
