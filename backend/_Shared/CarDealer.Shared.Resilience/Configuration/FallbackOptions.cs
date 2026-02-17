namespace CarDealer.Shared.Resilience.Configuration;

/// <summary>
/// Opciones de Fallback para respuestas degradadas
/// cuando un servicio downstream no está disponible
/// </summary>
public class FallbackOptions
{
    /// <summary>
    /// Habilitar fallback responses
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Devolver respuesta cacheada cuando el servicio falla
    /// </summary>
    public bool UseCachedResponse { get; set; } = true;

    /// <summary>
    /// TTL del cache de fallback (segundos)
    /// </summary>
    public int CacheTtlSeconds { get; set; } = 300;

    /// <summary>
    /// HTTP Status code a retornar en modo degradado
    /// </summary>
    public int DegradedStatusCode { get; set; } = 503;

    /// <summary>
    /// Mensaje de fallback por defecto
    /// </summary>
    public string DefaultMessage { get; set; } = "Service temporarily unavailable. Please try again later.";
}
