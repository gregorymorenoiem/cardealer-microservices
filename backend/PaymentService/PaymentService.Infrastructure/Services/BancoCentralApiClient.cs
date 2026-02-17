using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Services.Settings;

namespace PaymentService.Infrastructure.Services;

/// <summary>
/// Cliente HTTP para el API del Banco Central de la República Dominicana
/// Fuente oficial de tasas de cambio requerida por DGII
/// https://api.bancentral.gov.do/
/// </summary>
public class BancoCentralApiClient
{
    private readonly HttpClient _httpClient;
    private readonly BancoCentralSettings _settings;
    private readonly ILogger<BancoCentralApiClient> _logger;

    public BancoCentralApiClient(
        HttpClient httpClient,
        IOptions<BancoCentralSettings> settings,
        ILogger<BancoCentralApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        // Configurar HttpClient
        _httpClient.BaseAddress = new Uri(_settings.ApiBaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        
        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        }
    }

    /// <summary>
    /// Obtiene las tasas de cambio del día desde el API del BCRD
    /// </summary>
    /// <returns>Lista de tasas de cambio</returns>
    public async Task<List<ExchangeRate>> GetTodayRatesAsync(CancellationToken cancellationToken = default)
    {
        var rates = new List<ExchangeRate>();
        var today = DateOnly.FromDateTime(DateTime.Now); // Hora local RD

        foreach (var currency in _settings.SupportedCurrencies)
        {
            try
            {
                var rate = await GetRateFromApiAsync(currency, today, cancellationToken);
                if (rate != null)
                {
                    rates.Add(rate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error obteniendo tasa {Currency} del BCRD API", currency);
            }
        }

        // Si no obtuvimos tasas del API, intentar fallbacks
        if (rates.Count == 0)
        {
            _logger.LogWarning("No se obtuvieron tasas del API BCRD, intentando fallbacks...");
            rates = await TryFallbacksAsync(today, cancellationToken);
        }

        return rates;
    }

    /// <summary>
    /// Obtiene la tasa de una moneda específica del API
    /// </summary>
    private async Task<ExchangeRate?> GetRateFromApiAsync(string currency, DateOnly date, CancellationToken cancellationToken)
    {
        try
        {
            // Endpoint del BCRD para tasas de cambio
            // Nota: El endpoint real puede variar - verificar documentación del BCRD
            var endpoint = $"/tasas/v1/{currency.ToLower()}?fecha={date:yyyy-MM-dd}";
            
            _logger.LogDebug("Consultando BCRD: {Endpoint}", endpoint);

            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "BCRD API respondió con {StatusCode} para {Currency}", 
                    response.StatusCode, currency);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var bcrdResponse = JsonSerializer.Deserialize<BcrdRateResponse>(content);

            if (bcrdResponse?.Data == null)
            {
                _logger.LogWarning("Respuesta vacía del BCRD para {Currency}", currency);
                return null;
            }

            return new ExchangeRate
            {
                Id = Guid.NewGuid(),
                RateDate = date,
                SourceCurrency = currency.ToUpperInvariant(),
                TargetCurrency = "DOP",
                BuyRate = bcrdResponse.Data.BuyRate,
                SellRate = bcrdResponse.Data.SellRate,
                Source = ExchangeRateSource.BancoCentralApi,
                BcrdReferenceId = bcrdResponse.Data.ReferenceId,
                FetchedAt = DateTime.UtcNow,
                IsActive = true,
                Metadata = content
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión con BCRD API para {Currency}", currency);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parseando respuesta BCRD para {Currency}", currency);
            return null;
        }
    }

    /// <summary>
    /// Intenta obtener tasas de fuentes alternativas
    /// </summary>
    private async Task<List<ExchangeRate>> TryFallbacksAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var rates = new List<ExchangeRate>();

        // Fallback 1: Web scraping del BCRD
        if (_settings.EnableWebScrapingFallback)
        {
            try
            {
                var scrapedRates = await ScrapeRatesFromWebAsync(date, cancellationToken);
                if (scrapedRates.Count > 0)
                {
                    _logger.LogInformation("Tasas obtenidas via web scraping: {Count}", scrapedRates.Count);
                    return scrapedRates;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error en web scraping del BCRD");
            }
        }

        // Fallback 2: Proveedor externo
        if (_settings.EnableExternalProviderFallback)
        {
            try
            {
                var externalRates = await GetRatesFromExternalProviderAsync(date, cancellationToken);
                if (externalRates.Count > 0)
                {
                    _logger.LogInformation("Tasas obtenidas de proveedor externo: {Count}", externalRates.Count);
                    return externalRates;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error obteniendo tasas de proveedor externo");
            }
        }

        return rates;
    }

    /// <summary>
    /// Scraping de la página web del BCRD (fallback)
    /// </summary>
    private async Task<List<ExchangeRate>> ScrapeRatesFromWebAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var rates = new List<ExchangeRate>();

        try
        {
            using var client = new HttpClient();
            var html = await client.GetStringAsync(_settings.WebScrapingUrl, cancellationToken);

            // Parsear HTML para extraer tasas
            // NOTA: Este parsing es básico y debe ajustarse según la estructura real del sitio
            // En producción, usar una librería como HtmlAgilityPack o AngleSharp

            // Ejemplo de tasas hardcodeadas para desarrollo (remover en producción)
            // El scraping real extraería valores del HTML
            var usdMatch = ExtractRateFromHtml(html, "USD");
            var eurMatch = ExtractRateFromHtml(html, "EUR");

            if (usdMatch.HasValue)
            {
                rates.Add(new ExchangeRate
                {
                    Id = Guid.NewGuid(),
                    RateDate = date,
                    SourceCurrency = "USD",
                    TargetCurrency = "DOP",
                    BuyRate = usdMatch.Value.buy,
                    SellRate = usdMatch.Value.sell,
                    Source = ExchangeRateSource.BancoCentralWebScrape,
                    FetchedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            if (eurMatch.HasValue)
            {
                rates.Add(new ExchangeRate
                {
                    Id = Guid.NewGuid(),
                    RateDate = date,
                    SourceCurrency = "EUR",
                    TargetCurrency = "DOP",
                    BuyRate = eurMatch.Value.buy,
                    SellRate = eurMatch.Value.sell,
                    Source = ExchangeRateSource.BancoCentralWebScrape,
                    FetchedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en scraping del sitio BCRD");
        }

        return rates;
    }

    /// <summary>
    /// Extrae tasa de cambio del HTML (implementación básica)
    /// </summary>
    private (decimal buy, decimal sell)? ExtractRateFromHtml(string html, string currency)
    {
        // TODO: Implementar parsing real del HTML del BCRD
        // Por ahora retorna null para forzar uso del API
        // En producción: usar HtmlAgilityPack o regex
        
        _logger.LogDebug("Intentando extraer {Currency} del HTML (longitud: {Length})", currency, html.Length);
        return null;
    }

    /// <summary>
    /// Obtiene tasas de un proveedor externo (último fallback)
    /// </summary>
    private async Task<List<ExchangeRate>> GetRatesFromExternalProviderAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var rates = new List<ExchangeRate>();

        try
        {
            using var client = new HttpClient();
            // Usar proveedor que tenga DOP (ExchangeRate-API soporta DOP)
            var response = await client.GetFromJsonAsync<ExternalRateResponse>(
                $"https://api.exchangerate-api.com/v4/latest/DOP", 
                cancellationToken);

            if (response?.Rates == null) return rates;

            // USD/DOP (invertir porque la API da DOP como base)
            if (response.Rates.TryGetValue("USD", out var usdRate) && usdRate > 0)
            {
                var buyRate = Math.Round(1 / usdRate, 4);
                rates.Add(new ExchangeRate
                {
                    Id = Guid.NewGuid(),
                    RateDate = date,
                    SourceCurrency = "USD",
                    TargetCurrency = "DOP",
                    BuyRate = buyRate,
                    SellRate = buyRate * 1.01m, // Spread del 1%
                    Source = ExchangeRateSource.ExternalProvider,
                    FetchedAt = DateTime.UtcNow,
                    IsActive = true,
                    Metadata = JsonSerializer.Serialize(response)
                });
            }

            // EUR/DOP
            if (response.Rates.TryGetValue("EUR", out var eurRate) && eurRate > 0)
            {
                var buyRate = Math.Round(1 / eurRate, 4);
                rates.Add(new ExchangeRate
                {
                    Id = Guid.NewGuid(),
                    RateDate = date,
                    SourceCurrency = "EUR",
                    TargetCurrency = "DOP",
                    BuyRate = buyRate,
                    SellRate = buyRate * 1.01m,
                    Source = ExchangeRateSource.ExternalProvider,
                    FetchedAt = DateTime.UtcNow,
                    IsActive = true,
                    Metadata = JsonSerializer.Serialize(response)
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo tasas de proveedor externo");
        }

        return rates;
    }

    /// <summary>
    /// Verifica si el API del BCRD está disponible
    /// </summary>
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

// ==================== DTOs para respuestas del API ====================

/// <summary>
/// Respuesta del API del Banco Central
/// </summary>
public class BcrdRateResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public BcrdRateData? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// Datos de tasa del BCRD
/// </summary>
public class BcrdRateData
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("buy_rate")]
    public decimal BuyRate { get; set; }

    [JsonPropertyName("sell_rate")]
    public decimal SellRate { get; set; }

    [JsonPropertyName("reference_id")]
    public string? ReferenceId { get; set; }
}

/// <summary>
/// Respuesta de proveedor externo (exchangerate-api.com)
/// </summary>
public class ExternalRateResponse
{
    [JsonPropertyName("base")]
    public string Base { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("rates")]
    public Dictionary<string, decimal> Rates { get; set; } = new();
}
