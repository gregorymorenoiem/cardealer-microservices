namespace PaymentService.Infrastructure.Services.Settings;

/// <summary>
/// Configuración para el cliente del Banco Central de la República Dominicana
/// https://api.bancentral.gov.do/
/// </summary>
public class BancoCentralSettings
{
    /// <summary>
    /// URL base del API del BCRD
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.bancentral.gov.do";

    /// <summary>
    /// URL de la página web para scraping (fallback)
    /// </summary>
    public string WebScrapingUrl { get; set; } = "https://www.bancentral.gov.do/a/d/2532-tasas-de-cambio";

    /// <summary>
    /// API Key del BCRD (requiere registro previo)
    /// Registrarse en: https://api.bancentral.gov.do/
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Timeout para llamadas al API (segundos)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Tiempo de caché en Redis (horas)
    /// Las tasas del BCRD se actualizan diariamente
    /// </summary>
    public int CacheHours { get; set; } = 24;

    /// <summary>
    /// Hora del día para refrescar tasas (formato 24h)
    /// BCRD publica tasas aproximadamente a las 8:00 AM
    /// </summary>
    public int RefreshHour { get; set; } = 8;

    /// <summary>
    /// Minuto para refrescar tasas
    /// </summary>
    public int RefreshMinute { get; set; } = 30;

    /// <summary>
    /// Monedas a consultar
    /// </summary>
    public string[] SupportedCurrencies { get; set; } = { "USD", "EUR" };

    /// <summary>
    /// Habilitar fallback a web scraping
    /// </summary>
    public bool EnableWebScrapingFallback { get; set; } = true;

    /// <summary>
    /// Habilitar fallback a proveedor externo
    /// </summary>
    public bool EnableExternalProviderFallback { get; set; } = true;

    /// <summary>
    /// URL del proveedor externo (Fixer.io, ExchangeRate-API)
    /// </summary>
    public string ExternalProviderUrl { get; set; } = "https://api.exchangerate-api.com/v4/latest/USD";

    /// <summary>
    /// Número máximo de reintentos
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Delay entre reintentos (milisegundos)
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;
}
