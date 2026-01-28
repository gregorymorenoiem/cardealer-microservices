namespace BackgroundRemovalService.Domain.Enums;

/// <summary>
/// Formatos de salida soportados para imágenes procesadas
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// PNG con transparencia (recomendado)
    /// </summary>
    Png = 1,
    
    /// <summary>
    /// WebP con transparencia (mejor compresión)
    /// </summary>
    WebP = 2,
    
    /// <summary>
    /// JPEG con fondo blanco (sin transparencia)
    /// </summary>
    Jpeg = 3,
    
    /// <summary>
    /// Mantener formato original (si soporta transparencia)
    /// </summary>
    Auto = 4
}
