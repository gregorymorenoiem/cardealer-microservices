using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Repositories;

/// <summary>
/// Repositorio para registros de conversión de moneda (auditoría DGII)
/// </summary>
public class CurrencyConversionRepository : ICurrencyConversionRepository
{
    private readonly AzulDbContext _context;
    private readonly ILogger<CurrencyConversionRepository> _logger;

    public CurrencyConversionRepository(AzulDbContext context, ILogger<CurrencyConversionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CurrencyConversion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyConversions
            .Include(c => c.ExchangeRate)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CurrencyConversion?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyConversions
            .Include(c => c.ExchangeRate)
            .FirstOrDefaultAsync(c => c.PaymentTransactionId == transactionId, cancellationToken);
    }

    public async Task<IReadOnlyList<CurrencyConversion>> GetConversionsInRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyConversions
            .Include(c => c.ExchangeRate)
            .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CurrencyConversion> SaveAsync(CurrencyConversion conversion, CancellationToken cancellationToken = default)
    {
        conversion.OriginalCurrency = conversion.OriginalCurrency.ToUpperInvariant();
        
        _context.CurrencyConversions.Add(conversion);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Conversión registrada: {OriginalAmount} {Currency} → {ConvertedDop} DOP (Tasa: {Rate}) - TX: {TxId}",
            conversion.OriginalAmount,
            conversion.OriginalCurrency,
            conversion.ConvertedAmountDop,
            conversion.AppliedRate,
            conversion.PaymentTransactionId);

        return conversion;
    }

    public async Task UpdateNcfAsync(Guid id, string ncf, CancellationToken cancellationToken = default)
    {
        var conversion = await _context.CurrencyConversions.FindAsync(new object[] { id }, cancellationToken);
        if (conversion != null)
        {
            conversion.Ncf = ncf;
            conversion.NcfIssuedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("NCF {Ncf} asignado a conversión {ConversionId}", ncf, id);
        }
    }

    public async Task<Dictionary<string, decimal>> GetTotalsByCurrencyAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyConversions
            .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
            .GroupBy(c => c.OriginalCurrency)
            .Select(g => new { Currency = g.Key, Total = g.Sum(c => c.OriginalAmount) })
            .ToDictionaryAsync(x => x.Currency, x => x.Total, cancellationToken);
    }
}
