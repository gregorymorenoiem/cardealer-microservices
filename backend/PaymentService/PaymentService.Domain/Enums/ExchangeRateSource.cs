namespace PaymentService.Domain.Enums;

/// <summary>
/// Fuente de la tasa de cambio
/// Para cumplimiento DGII, la fuente oficial debe ser el Banco Central (BCRD)
/// </summary>
public enum ExchangeRateSource
{
    /// <summary>
    /// API oficial del Banco Central de la República Dominicana
    /// Fuente preferida y requerida por DGII
    /// </summary>
    BancoCentralApi = 1,

    /// <summary>
    /// Scraping del sitio web del BCRD
    /// Usado como fallback si el API no está disponible
    /// </summary>
    BancoCentralWebScrape = 2,

    /// <summary>
    /// Tasa del día anterior (caché)
    /// Usado como último recurso
    /// </summary>
    CachedPreviousDay = 3,

    /// <summary>
    /// Tasa ingresada manualmente
    /// Solo permitido en casos excepcionales con auditoría
    /// </summary>
    ManualEntry = 4,

    /// <summary>
    /// Proveedor secundario (backup externo)
    /// Ej: Fixer.io, ExchangeRate-API
    /// </summary>
    ExternalProvider = 5
}
