namespace Video360Service.Domain.Entities;

/// <summary>
/// Representa un frame extraído del video 360.
/// Cada Video360Job genera 6 frames para la vista interactiva.
/// </summary>
public class ExtractedFrame
{
    public ExtractedFrame()
    {
    }
    
    /// <summary>
    /// Constructor con parámetros principales
    /// </summary>
    public ExtractedFrame(
        Guid video360JobId,
        int frameIndex,
        int angleDegrees,
        string? angleLabel,
        string imageUrl,
        long fileSizeBytes,
        string contentType)
    {
        Video360JobId = video360JobId;
        FrameIndex = frameIndex;
        AngleDegrees = angleDegrees;
        AngleLabel = angleLabel;
        ImageUrl = imageUrl;
        FileSizeBytes = fileSizeBytes;
        ContentType = contentType;
    }
    
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// ID del job padre
    /// </summary>
    public Guid Video360JobId { get; set; }
    
    /// <summary>
    /// Referencia al job padre
    /// </summary>
    public Video360Job? Video360Job { get; set; }
    
    /// <summary>
    /// Índice del frame (0-5 para 6 frames)
    /// </summary>
    public int FrameIndex { get; set; }
    
    /// <summary>
    /// Ángulo en grados (0, 60, 120, 180, 240, 300)
    /// </summary>
    public int AngleDegrees { get; set; }
    
    /// <summary>
    /// Timestamp del frame en el video (segundos)
    /// </summary>
    public double TimestampSeconds { get; set; }
    
    /// <summary>
    /// URL de la imagen resultante (almacenada en S3/storage)
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// URL del thumbnail de la imagen
    /// </summary>
    public string? ThumbnailUrl { get; set; }
    
    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long FileSizeBytes { get; set; }
    
    /// <summary>
    /// Content type (image/jpeg, image/png, etc.)
    /// </summary>
    public string ContentType { get; set; } = "image/jpeg";
    
    /// <summary>
    /// Ancho de la imagen en píxeles
    /// </summary>
    public int Width { get; set; }
    
    /// <summary>
    /// Alto de la imagen en píxeles
    /// </summary>
    public int Height { get; set; }
    
    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Hash del archivo para verificación
    /// </summary>
    public string? FileHash { get; set; }
    
    /// <summary>
    /// Si la imagen ha sido optimizada
    /// </summary>
    public bool IsOptimized { get; set; } = false;
    
    /// <summary>
    /// Descripción o etiqueta del ángulo (ej: "Frente", "Lado Izquierdo")
    /// </summary>
    public string? AngleLabel { get; set; }
    
    /// <summary>
    /// Obtiene la etiqueta del ángulo basado en el índice
    /// </summary>
    public static string GetAngleLabelByIndex(int index) => index switch
    {
        0 => "Frente",
        1 => "Frente-Derecha",
        2 => "Trasera-Derecha",
        3 => "Trasera",
        4 => "Trasera-Izquierda",
        5 => "Frente-Izquierda",
        _ => $"Ángulo {index}"
    };
    
    /// <summary>
    /// Calcula el ángulo en grados basado en el índice (6 frames = 60° cada uno)
    /// </summary>
    public static int CalculateAngle(int frameIndex, int totalFrames = 6)
    {
        return (360 / totalFrames) * frameIndex;
    }
}
