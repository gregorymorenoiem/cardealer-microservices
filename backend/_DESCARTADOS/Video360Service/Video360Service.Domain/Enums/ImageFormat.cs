namespace Video360Service.Domain.Enums;

/// <summary>
/// Formato de salida para las imágenes extraídas
/// </summary>
public enum ImageFormat
{
    /// <summary>
    /// JPEG - Mejor para fotos, menor tamaño
    /// </summary>
    Jpeg = 0,
    
    /// <summary>
    /// PNG - Sin pérdida, soporta transparencia
    /// </summary>
    Png = 1,
    
    /// <summary>
    /// WebP - Moderno, mejor compresión
    /// </summary>
    WebP = 2
}
