using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Services.Settings;
using System.Text.Json;

namespace PaymentService.Infrastructure.Services;

/// <summary>
/// Servicio de tasas de cambio con cumplimiento DGII
/// Usa tasas oficiales del Banco Central de la República Dominicana
/// Implementa caché para optimizar rendimiento
/// </summary>
public class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _rateRepository;
    private readonly ICurrencyConversionRepository _conversionRepository;
    private readonly BancoCentralApiClient _bcrdClient;
    private readonly IDistributedCache? _cache;
    private readonly BancoCentralSettings _settings;
    private readonly ILogger<ExchangeRateService> _logger;

    private const string CACHE_KEY_PREFIX = "exchange_rate";
    private const decimal ITBIS_RATE = 0.18m;

    public ExchangeRateService(
        IExchangeRateRepository rateRepository,
        ICurrencyConversionRepository conversionRepository,
        BancoCentralApiClient bcrdClient,
        IOptions<BancoCentralSettings> settings,
        ILogger<ExchangeRateService> logger,
        IDistributedCache? cache = null)
    {
        _rateRepository = rateRepository;
        _conversionRepository = conversionRepository;
        _bcrdClient = bcrdClient;
        _settings = settings.Value;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ExchangeRate> GetCurrentRateAsync(string currency, CancellationToken cancellationToken = default)
    {
        currency = currency.ToUpperInvariant();
        var today = DateOnly.FromDateTime(DateTime.Now); // Hora local RD

        // 1. Intentar obtener de caché
        var cachedRate = await GetFromCacheAsync(currency, today);
        if (cachedRate != null)
        {
            _logger.LogDebug("Tasa {Currency} obtenida de caché", currency);
            return cachedRate;
        }

        // 2. Intentar obtener de base de datos
        var dbRate = await _rateRepository.GetRateAsync(currency, today, cancellationToken);
        if (dbRate != null)
        {
            await SetCacheAsync(dbRate);
            _logger.LogDebug("Tasa {Currency} obtenida de BD", currency);
            return dbRate;
        }

        // 3. Consultar al Banco Central
        var bcrdRates = await _bcrdClient.GetTodayRatesAsync(cancellationToken);
        var rate = bcrdRates.FirstOrDefault(r => r.SourceCurrency == currency);

        if (rate != null)
        {
            // Guardar en BD y caché
            await _rateRepository.SaveRateAsync(rate, cancellationToken);
            await SetCacheAsync(rate);
            _logger.LogInformation("Tasa {Currency} obtenida del BCRD: {Rate}", currency, rate.BuyRate);
            return rate;
        }

        // 4. Fallback: usar última tasa conocida
        var latestRate = await _rateRepository.GetLatestRateAsync(currency, cancellationToken);
        if (latestRate != null)
        {
            _logger.LogWarning(
                "Usando tasa histórica de {Date} para {Currency} (no se pudo obtener tasa actual)",
                latestRate.RateDate, currency);
            
            // Marcar como caché de día anterior
            latestRate.Source = ExchangeRateSource.CachedPreviousDay;
            return latestRate;
        }

        // 5. Error: no hay tasa disponible
        throw new InvalidOperationException(
            $"No se pudo obtener tasa de cambio para {currency}. " +
            "El Banco Central no está disponible y no hay tasas históricas en la base de datos.");
    }

    public async Task<ExchangeRate> GetRateForDateAsync(string currency, DateOnly date, CancellationToken cancellationToken = default)
    {
        currency = currency.ToUpperInvariant();

        // Primero buscar en BD (las tasas históricas deberían estar ahí)
        var rate = await _rateRepository.GetRateAsync(currency, date, cancellationToken);
        if (rate != null)
        {
            return rate;
        }

        // Si es fecha futura, usar la tasa actual
        if (date > DateOnly.FromDateTime(DateTime.Now))
        {
            return await GetCurrentRateAsync(currency, cancellationToken);
        }

        // Si es fecha pasada sin tasa, buscar la más cercana
        var nearestRate = await _rateRepository.GetLatestRateAsync(currency, cancellationToken);
        if (nearestRate != null && nearestRate.RateDate <= date)
        {
            _logger.LogWarning(
                "No hay tasa para {Currency} en {Date}, usando tasa de {RateDate}",
                currency, date, nearestRate.RateDate);
            return nearestRate;
        }

        throw new InvalidOperationException($"No se encontró tasa de cambio para {currency} en {date}");
    }

    public async Task<ConversionResult> ConvertToDopAsync(
        decimal amount,
        string fromCurrency,
        Guid? transactionId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            fromCurrency = fromCurrency.ToUpperInvariant();

            // Si ya es DOP, no hay conversión
            if (fromCurrency == "DOP")
            {
                return new ConversionResult
                {
                    Success = true,
                    OriginalAmount = amount,
                    OriginalCurrency = "DOP",
                    ConvertedAmountDop = amount,
                    AppliedRate = 1.0m,
                    RateDate = DateOnly.FromDateTime(DateTime.Now),
                    RateSource = "NoConversion",
                    ItbisDop = Math.Round(amount * ITBIS_RATE, 2),
                    TotalWithItbisDop = Math.Round(amount * (1 + ITBIS_RATE), 2)
                };
            }

            // Obtener tasa actual
            var rate = await GetCurrentRateAsync(fromCurrency, cancellationToken);

            // Crear registro de conversión si hay transactionId
            CurrencyConversion? conversion = null;
            if (transactionId.HasValue)
            {
                conversion = CurrencyConversion.Create(
                    transactionId.Value,
                    rate,
                    amount,
                    fromCurrency,
                    ConversionType.Purchase);

                await _conversionRepository.SaveAsync(conversion, cancellationToken);
            }

            var result = ConversionResult.Successful(amount, fromCurrency, rate, conversion);
            
            // Calcular ITBIS si no hay conversión guardada
            if (conversion == null)
            {
                result.ItbisDop = Math.Round(result.ConvertedAmountDop * ITBIS_RATE, 2);
                result.TotalWithItbisDop = result.ConvertedAmountDop + result.ItbisDop;
            }

            _logger.LogInformation(
                "Conversión: {Amount} {Currency} → {AmountDop} DOP (Tasa: {Rate}, ITBIS: {Itbis})",
                amount, fromCurrency, result.ConvertedAmountDop, rate.BuyRate, result.ItbisDop);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error convirtiendo {Amount} {Currency} a DOP", amount, fromCurrency);
            return ConversionResult.Failed($"Error en conversión: {ex.Message}");
        }
    }

    public async Task<ConversionResult> ConvertFromDopAsync(
        decimal amountDop,
        string toCurrency,
        Guid? transactionId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            toCurrency = toCurrency.ToUpperInvariant();

            if (toCurrency == "DOP")
            {
                return new ConversionResult
                {
                    Success = true,
                    OriginalAmount = amountDop,
                    OriginalCurrency = "DOP",
                    ConvertedAmountDop = amountDop,
                    AppliedRate = 1.0m,
                    RateDate = DateOnly.FromDateTime(DateTime.Now),
                    RateSource = "NoConversion"
                };
            }

            var rate = await GetCurrentRateAsync(toCurrency, cancellationToken);
            var convertedAmount = rate.ConvertFromDop(amountDop);

            CurrencyConversion? conversion = null;
            if (transactionId.HasValue)
            {
                conversion = CurrencyConversion.Create(
                    transactionId.Value,
                    rate,
                    amountDop,
                    toCurrency,
                    ConversionType.Refund);

                conversion.ConvertedAmountDop = amountDop; // En refund, el DOP es el origen
                await _conversionRepository.SaveAsync(conversion, cancellationToken);
            }

            return new ConversionResult
            {
                Success = true,
                OriginalAmount = convertedAmount,
                OriginalCurrency = toCurrency,
                ConvertedAmountDop = amountDop,
                AppliedRate = rate.SellRate,
                RateDate = rate.RateDate,
                RateSource = rate.Source.ToString(),
                ExchangeRateId = rate.Id,
                ConversionRecordId = conversion?.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error convirtiendo {AmountDop} DOP a {Currency}", amountDop, toCurrency);
            return ConversionResult.Failed($"Error en conversión: {ex.Message}");
        }
    }

    public async Task<ConversionResult> GetQuoteAsync(
        decimal amount,
        string fromCurrency,
        CancellationToken cancellationToken = default)
    {
        // Solo consulta, no registra
        return await ConvertToDopAsync(amount, fromCurrency, null, cancellationToken);
    }

    public async Task<bool> RefreshRatesFromBcrdAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando actualización de tasas desde BCRD...");

            var rates = await _bcrdClient.GetTodayRatesAsync(cancellationToken);

            if (rates.Count == 0)
            {
                _logger.LogWarning("No se obtuvieron tasas del BCRD");
                return false;
            }

            foreach (var rate in rates)
            {
                await _rateRepository.SaveRateAsync(rate, cancellationToken);
                await SetCacheAsync(rate);
            }

            _logger.LogInformation(
                "Tasas actualizadas exitosamente: {Currencies}",
                string.Join(", ", rates.Select(r => $"{r.SourceCurrency}={r.BuyRate}")));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando tasas desde BCRD");
            return false;
        }
    }

    public async Task<bool> IsServiceHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar que podemos obtener tasas
            var currencies = await _rateRepository.GetAvailableCurrenciesAsync(cancellationToken);
            
            // Debe haber al menos USD disponible
            if (!currencies.Contains("USD"))
            {
                _logger.LogWarning("No hay tasa USD disponible - servicio degradado");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando salud del servicio de tasas");
            return false;
        }
    }

    // ==================== CACHE HELPERS ====================

    private async Task<ExchangeRate?> GetFromCacheAsync(string currency, DateOnly date)
    {
        if (_cache == null) return null;

        try
        {
            var key = $"{CACHE_KEY_PREFIX}:{currency}:{date:yyyy-MM-dd}";
            var cached = await _cache.GetStringAsync(key);
            
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<ExchangeRate>(cached);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error leyendo caché para {Currency}", currency);
        }

        return null;
    }

    private async Task SetCacheAsync(ExchangeRate rate)
    {
        if (_cache == null) return;

        try
        {
            var key = $"{CACHE_KEY_PREFIX}:{rate.SourceCurrency}:{rate.RateDate:yyyy-MM-dd}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(_settings.CacheHours)
            };

            await _cache.SetStringAsync(key, JsonSerializer.Serialize(rate), options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error guardando en caché {Currency}", rate.SourceCurrency);
        }
    }
}
