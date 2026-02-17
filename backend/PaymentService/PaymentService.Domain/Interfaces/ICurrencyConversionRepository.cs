using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Interfaces;

/// <summary>
/// Repositorio para registros de conversión de moneda (auditoría DGII)
/// </summary>
public interface ICurrencyConversionRepository
{
    /// <summary>
    /// Obtiene una conversión por ID
    /// </summary>
    Task<CurrencyConversion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la conversión asociada a una transacción de pago
    /// </summary>
    Task<CurrencyConversion?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las conversiones en un rango de fechas (para reportes DGII)
    /// </summary>
    Task<IReadOnlyList<CurrencyConversion>> GetConversionsInRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda un registro de conversión
    /// </summary>
    Task<CurrencyConversion> SaveAsync(CurrencyConversion conversion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza el NCF de una conversión (cuando se emite la factura)
    /// </summary>
    Task UpdateNcfAsync(Guid id, string ncf, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el total convertido por moneda en un período (para reportes)
    /// </summary>
    Task<Dictionary<string, decimal>> GetTotalsByCurrencyAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}
