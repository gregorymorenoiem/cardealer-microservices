namespace BackgroundRemovalService.Domain.Enums;

/// <summary>
/// Tamaños de imagen de salida
/// </summary>
public enum ImageSize
{
    /// <summary>
    /// Mantener tamaño original
    /// </summary>
    Original = 0,
    
    /// <summary>
    /// Preview pequeño (hasta 256px)
    /// </summary>
    Preview = 1,
    
    /// <summary>
    /// Pequeño (hasta 512px)
    /// </summary>
    Small = 2,
    
    /// <summary>
    /// Mediano (hasta 1024px)
    /// </summary>
    Medium = 3,
    
    /// <summary>
    /// Grande (hasta 2048px)
    /// </summary>
    Large = 4,
    
    /// <summary>
    /// Full HD (hasta 1920px)
    /// </summary>
    FullHD = 5,
    
    /// <summary>
    /// 4K (hasta 4096px)
    /// </summary>
    UltraHD = 6
}
