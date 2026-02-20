namespace Video360Service.Domain.Enums;

/// <summary>
/// Calidad/resolución de las imágenes de salida
/// </summary>
public enum VideoQuality
{
    /// <summary>
    /// Baja resolución (800x600) - Para thumbnails
    /// </summary>
    Low = 0,
    
    /// <summary>
    /// Resolución media (1280x960) - Balance
    /// </summary>
    Medium = 1,
    
    /// <summary>
    /// Alta resolución (1920x1440) - Recomendado
    /// </summary>
    High = 2,
    
    /// <summary>
    /// Resolución original del video
    /// </summary>
    Original = 3,
    
    /// <summary>
    /// Ultra alta resolución (3840x2880) - 4K
    /// </summary>
    Ultra = 4
}
