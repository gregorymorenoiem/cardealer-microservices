namespace Video360Service.Domain.Entities;

/// <summary>
/// Opciones de procesamiento para la extracción de frames
/// </summary>
public class ProcessingOptions
{
    /// <summary>
    /// Número de frames a extraer (default 6)
    /// </summary>
    public int FrameCount { get; set; } = 6;
    
    /// <summary>
    /// Ancho de salida de las imágenes (default 1920)
    /// </summary>
    public int OutputWidth { get; set; } = 1920;
    
    /// <summary>
    /// Alto de salida de las imágenes (default 1080)
    /// </summary>
    public int OutputHeight { get; set; } = 1080;
    
    /// <summary>
    /// Calidad de compresión JPEG (1-100, default 90)
    /// </summary>
    public int JpegQuality { get; set; } = 90;
    
    /// <summary>
    /// Formato de salida (jpg, png, webp)
    /// </summary>
    public string OutputFormat { get; set; } = "jpg";
    
    /// <summary>
    /// Generar miniaturas
    /// </summary>
    public bool GenerateThumbnails { get; set; } = true;
    
    /// <summary>
    /// Ancho de las miniaturas
    /// </summary>
    public int ThumbnailWidth { get; set; } = 400;
    
    /// <summary>
    /// Selección inteligente de frames (mejor calidad/enfoque)
    /// </summary>
    public bool SmartFrameSelection { get; set; } = true;
    
    /// <summary>
    /// Corregir brillo/contraste automáticamente
    /// </summary>
    public bool AutoCorrectExposure { get; set; } = true;
    
    /// <summary>
    /// Recortar al vehículo automáticamente
    /// </summary>
    public bool AutoCropToVehicle { get; set; } = false;
    
    /// <summary>
    /// Estabilizar el video antes de extraer
    /// </summary>
    public bool StabilizeVideo { get; set; } = false;
    
    /// <summary>
    /// Offset inicial en segundos (saltar intro)
    /// </summary>
    public double StartOffsetSeconds { get; set; } = 0;
    
    /// <summary>
    /// Offset final en segundos (saltar outro)
    /// </summary>
    public double EndOffsetSeconds { get; set; } = 0;
}
