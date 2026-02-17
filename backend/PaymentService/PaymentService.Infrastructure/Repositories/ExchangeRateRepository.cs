using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Repositories;

/// <summary>
/// Repositorio para tasas de cambio del Banco Central RD
/// </summary>
public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly AzulDbContext _context;
    private readonly ILogger<ExchangeRateRepository> _logger;

    public ExchangeRateRepository(AzulDbContext context, ILogger<ExchangeRateRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ExchangeRate?> GetRateAsync(string currency, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _context.ExchangeRates
            .Where(r => r.SourceCurrency == currency.ToUpperInvariant() 
                     && r.RateDate == date 
                     && r.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ExchangeRate?> GetLatestRateAsync(string currency, CancellationToken cancellationToken = default)
    {
        return await _context.ExchangeRates
            .Where(r => r.SourceCurrency == currency.ToUpperInvariant() && r.IsActive)
            .OrderByDescending(r => r.RateDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetRatesInRangeAsync(
        string currency,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.ExchangeRates
            .Where(r => r.SourceCurrency == currency.ToUpperInvariant()
                     && r.RateDate >= startDate
                     && r.RateDate <= endDate
                     && r.IsActive)
            .OrderByDescending(r => r.RateDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExchangeRate> SaveRateAsync(ExchangeRate rate, CancellationToken cancellationToken = default)
    {
        // Normalizar moneda a mayúsculas
        rate.SourceCurrency = rate.SourceCurrency.ToUpperInvariant();
        rate.TargetCurrency = rate.TargetCurrency.ToUpperInvariant();

        // Desactivar tasa anterior para la misma moneda/fecha si existe
        var existingRate = await _context.ExchangeRates
            .Where(r => r.SourceCurrency == rate.SourceCurrency
                     && r.RateDate == rate.RateDate
                     && r.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingRate != null)
        {
            existingRate.IsActive = false;
            existingRate.UpdatedAt = DateTime.UtcNow;
            _logger.LogInformation(
                "Desactivando tasa anterior {Currency} para {Date}: {OldRate} → {NewRate}",
                rate.SourceCurrency, rate.RateDate, existingRate.BuyRate, rate.BuyRate);
        }

        rate.CreatedAt = DateTime.UtcNow;
        rate.UpdatedAt = DateTime.UtcNow;
        
        _context.ExchangeRates.Add(rate);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Tasa guardada: {Currency}/{Target} = {BuyRate}/{SellRate} para {Date} (Fuente: {Source})",
            rate.SourceCurrency, rate.TargetCurrency, rate.BuyRate, rate.SellRate, rate.RateDate, rate.Source);

        return rate;
    }

    public async Task SaveRatesAsync(IEnumerable<ExchangeRate> rates, CancellationToken cancellationToken = default)
    {
        foreach (var rate in rates)
        {
            await SaveRateAsync(rate, cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string currency, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _context.ExchangeRates
            .AnyAsync(r => r.SourceCurrency == currency.ToUpperInvariant()
                        && r.RateDate == date
                        && r.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetAvailableCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ExchangeRates
            .Where(r => r.IsActive)
            .Select(r => r.SourceCurrency)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }
}
