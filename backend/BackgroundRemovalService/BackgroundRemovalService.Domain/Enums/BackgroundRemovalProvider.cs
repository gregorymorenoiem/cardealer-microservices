namespace BackgroundRemovalService.Domain.Enums;

/// <summary>
/// Proveedores de remoción de fondo disponibles.
/// Fácilmente extensible agregando nuevos valores.
/// ClipDrop es el proveedor por defecto (valor 0).
/// </summary>
public enum BackgroundRemovalProvider
{
    /// <summary>
    /// ClipDrop - API por defecto para remoción de fondos (Stability AI)
    /// https://clipdrop.co/apis/docs/remove-background
    /// Excelente calidad con precios competitivos.
    /// </summary>
    ClipDrop = 0,
    
    /// <summary>
    /// Remove.bg - API popular para remoción de fondos
    /// https://www.remove.bg/
    /// </summary>
    RemoveBg = 1,
    
    /// <summary>
    /// Photoroom - Alternativa con buenos resultados
    /// https://www.photoroom.com/
    /// </summary>
    Photoroom = 2,
    
    /// <summary>
    /// Clipping Magic - Otra alternativa profesional
    /// https://clippingmagic.com/
    /// </summary>
    ClippingMagic = 3,
    
    /// <summary>
    /// Slazzer - Servicio económico
    /// https://www.slazzer.com/
    /// </summary>
    Slazzer = 4,
    
    /// <summary>
    /// RemovalAI - API con ML avanzado
    /// https://removal.ai/
    /// </summary>
    RemovalAI = 5,
    
    /// <summary>
    /// Local - Procesamiento local con rembg (Python)
    /// Para evitar costos de API en desarrollo
    /// </summary>
    Local = 99
}
