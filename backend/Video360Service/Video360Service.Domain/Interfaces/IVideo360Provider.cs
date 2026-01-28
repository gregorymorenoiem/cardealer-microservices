using Video360Service.Domain.Enums;

namespace Video360Service.Domain.Interfaces;

/// <summary>
/// Interfaz del Strategy Pattern para proveedores de extracción de frames 360.
/// Cada proveedor (FFmpeg-API, ApyHub, Cloudinary, etc.) implementa esta interfaz.
/// </summary>
public interface IVideo360Provider
{
    /// <summary>
    /// Identificador del proveedor
    /// </summary>
    Video360Provider ProviderType { get; }
    
    /// <summary>
    /// Nombre descriptivo del proveedor
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// Costo por video procesado en USD
    /// </summary>
    decimal CostPerVideoUsd { get; }
    
    /// <summary>
    /// Verifica si el proveedor está configurado y disponible
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Extrae frames de un video para crear vista 360
    /// </summary>
    /// <param name="videoBytes">Bytes del video original</param>
    /// <param name="options">Opciones de procesamiento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado con los frames extraídos</returns>
    Task<Video360ExtractionResult> ExtractFramesAsync(
        byte[] videoBytes, 
        Video360ExtractionOptions options,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Extrae frames de un video desde URL
    /// </summary>
    /// <param name="videoUrl">URL del video</param>
    /// <param name="options">Opciones de procesamiento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado con los frames extraídos</returns>
    Task<Video360ExtractionResult> ExtractFramesFromUrlAsync(
        string videoUrl, 
        Video360ExtractionOptions options,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene información de la cuenta (créditos, límites)
    /// </summary>
    Task<ProviderAccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Opciones para la extracción de frames 360
/// </summary>
public record Video360ExtractionOptions
{
    /// <summary>
    /// Número de frames a extraer (por defecto 6)
    /// </summary>
    public int FrameCount { get; init; } = 6;
    
    /// <summary>
    /// Formato de salida de las imágenes
    /// </summary>
    public ImageFormat OutputFormat { get; init; } = ImageFormat.Jpeg;
    
    /// <summary>
    /// Calidad de las imágenes (1-100)
    /// </summary>
    public int Quality { get; init; } = 90;
    
    /// <summary>
    /// Ancho de salida (null = proporcional)
    /// </summary>
    public int? Width { get; init; }
    
    /// <summary>
    /// Alto de salida (null = proporcional)
    /// </summary>
    public int? Height { get; init; }
    
    /// <summary>
    /// Calidad de video de salida
    /// </summary>
    public VideoQuality VideoQuality { get; init; } = VideoQuality.High;
    
    /// <summary>
    /// Si debe generar thumbnails
    /// </summary>
    public bool GenerateThumbnails { get; init; } = true;
    
    /// <summary>
    /// Ancho del thumbnail
    /// </summary>
    public int ThumbnailWidth { get; init; } = 200;
    
    /// <summary>
    /// Si debe optimizar las imágenes
    /// </summary>
    public bool OptimizeImages { get; init; } = true;
    
    /// <summary>
    /// Timestamps específicos a extraer (si null, se calculan equidistantes)
    /// </summary>
    public double[]? SpecificTimestamps { get; init; }
    
    /// <summary>
    /// Nombre base para los archivos de salida
    /// </summary>
    public string? OutputBaseName { get; init; }
}

/// <summary>
/// Resultado de la extracción de frames
/// </summary>
public record Video360ExtractionResult
{
    /// <summary>
    /// Si la extracción fue exitosa
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Código de error del proveedor
    /// </summary>
    public string? ErrorCode { get; init; }
    
    /// <summary>
    /// Los frames extraídos
    /// </summary>
    public IReadOnlyList<ExtractedFrameData> Frames { get; init; } = [];
    
    /// <summary>
    /// Tiempo de procesamiento en ms
    /// </summary>
    public long ProcessingTimeMs { get; init; }
    
    /// <summary>
    /// Costo del procesamiento
    /// </summary>
    public decimal CostUsd { get; init; }
    
    /// <summary>
    /// Metadatos del video procesado
    /// </summary>
    public VideoMetadata? VideoInfo { get; init; }
    
    /// <summary>
    /// ID de la operación en el proveedor (para tracking)
    /// </summary>
    public string? ProviderOperationId { get; init; }
}

/// <summary>
/// Datos de un frame extraído
/// </summary>
public record ExtractedFrameData
{
    /// <summary>
    /// Índice del frame (0-5)
    /// </summary>
    public int Index { get; init; }
    
    /// <summary>
    /// Ángulo en grados
    /// </summary>
    public int AngleDegrees { get; init; }
    
    /// <summary>
    /// Timestamp en segundos
    /// </summary>
    public double TimestampSeconds { get; init; }
    
    /// <summary>
    /// Bytes de la imagen
    /// </summary>
    public byte[] ImageBytes { get; init; } = [];
    
    /// <summary>
    /// Bytes del thumbnail (si se generó)
    /// </summary>
    public byte[]? ThumbnailBytes { get; init; }
    
    /// <summary>
    /// Content type de la imagen
    /// </summary>
    public string ContentType { get; init; } = "image/jpeg";
    
    /// <summary>
    /// Ancho de la imagen
    /// </summary>
    public int Width { get; init; }
    
    /// <summary>
    /// Alto de la imagen
    /// </summary>
    public int Height { get; init; }
    
    /// <summary>
    /// Etiqueta del ángulo
    /// </summary>
    public string? AngleLabel { get; init; }
    
    /// <summary>
    /// Calcula el ángulo para un frame dado el índice y el total de frames
    /// </summary>
    public static int CalculateAngle(int frameIndex, int totalFrames)
    {
        if (totalFrames <= 0) return 0;
        return (int)(360.0 * frameIndex / totalFrames);
    }
    
    /// <summary>
    /// Obtiene la etiqueta del ángulo
    /// </summary>
    public static string GetAngleLabel(int angleDegrees)
    {
        return angleDegrees switch
        {
            0 => "Front",
            45 => "Front-Right",
            60 => "Front-Right",
            90 => "Right",
            120 => "Back-Right",
            135 => "Back-Right",
            180 => "Back",
            225 => "Back-Left",
            240 => "Back-Left",
            270 => "Left",
            300 => "Front-Left",
            315 => "Front-Left",
            _ => $"{angleDegrees}°"
        };
    }
}

/// <summary>
/// Metadatos del video
/// </summary>
public record VideoMetadata
{
    /// <summary>
    /// Duración en segundos
    /// </summary>
    public double DurationSeconds { get; init; }
    
    /// <summary>
    /// Ancho del video
    /// </summary>
    public int Width { get; init; }
    
    /// <summary>
    /// Alto del video
    /// </summary>
    public int Height { get; init; }
    
    /// <summary>
    /// FPS del video
    /// </summary>
    public int Fps { get; init; }
    
    /// <summary>
    /// Codec del video
    /// </summary>
    public string? Codec { get; init; }
    
    /// <summary>
    /// Bitrate del video
    /// </summary>
    public long? BitrateKbps { get; init; }
}

/// <summary>
/// Información de la cuenta del proveedor
/// </summary>
public record ProviderAccountInfo
{
    /// <summary>
    /// Si la cuenta está activa
    /// </summary>
    public bool IsActive { get; init; }
    
    /// <summary>
    /// Créditos o procesamiento restante
    /// </summary>
    public int? RemainingCredits { get; init; }
    
    /// <summary>
    /// Límite de la cuenta
    /// </summary>
    public int? CreditLimit { get; init; }
    
    /// <summary>
    /// Fecha de reset de créditos
    /// </summary>
    public DateTime? CreditResetDate { get; init; }
    
    /// <summary>
    /// Plan actual
    /// </summary>
    public string? CurrentPlan { get; init; }
    
    /// <summary>
    /// Mensaje de error si hay problemas
    /// </summary>
    public string? ErrorMessage { get; init; }
}
