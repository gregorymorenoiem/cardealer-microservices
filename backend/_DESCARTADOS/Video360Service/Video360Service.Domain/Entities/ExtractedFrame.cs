namespace Video360Service.Domain.Entities;

/// <summary>
/// Representa un frame extraído del video 360
/// </summary>
public class ExtractedFrame
{
    public Guid Id { get; set; }
    public Guid Video360JobId { get; set; }
    
    /// <summary>
    /// Número de secuencia (1-6 típicamente)
    /// </summary>
    public int SequenceNumber { get; set; }
    
    /// <summary>
    /// Ángulo aproximado de la vista (0, 60, 120, 180, 240, 300 grados)
    /// </summary>
    public int AngleDegrees { get; set; }
    
    /// <summary>
    /// Nombre descriptivo de la vista
    /// </summary>
    public string ViewName { get; set; } = string.Empty;
    
    /// <summary>
    /// Número del frame en el video original
    /// </summary>
    public int SourceFrameNumber { get; set; }
    
    /// <summary>
    /// Timestamp del frame en el video (segundos)
    /// </summary>
    public double TimestampSeconds { get; set; }
    
    /// <summary>
    /// URL de la imagen extraída (después de subir a S3/storage)
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// URL de la miniatura
    /// </summary>
    public string? ThumbnailUrl { get; set; }
    
    /// <summary>
    /// Ancho de la imagen en píxeles
    /// </summary>
    public int Width { get; set; }
    
    /// <summary>
    /// Alto de la imagen en píxeles
    /// </summary>
    public int Height { get; set; }
    
    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long FileSizeBytes { get; set; }
    
    /// <summary>
    /// Formato de la imagen (jpg, png, webp)
    /// </summary>
    public string Format { get; set; } = "jpg";
    
    /// <summary>
    /// Puntuación de calidad del frame (0-100)
    /// </summary>
    public int? QualityScore { get; set; }
    
    /// <summary>
    /// Indica si este frame es el principal/destacado
    /// </summary>
    public bool IsPrimary { get; set; }
    
    /// <summary>
    /// Metadatos adicionales en JSON
    /// </summary>
    public string? MetadataJson { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navegación
    public Video360Job? Video360Job { get; set; }
    
    /// <summary>
    /// Nombres estándar para cada posición de vista
    /// </summary>
    public static readonly Dictionary<int, (string Name, int Angle)> StandardViews = new()
    {
        { 1, ("Frente", 0) },
        { 2, ("Frente-Derecha", 60) },
        { 3, ("Derecha", 120) },
        { 4, ("Atrás-Derecha", 180) },
        { 5, ("Atrás", 240) },
        { 6, ("Izquierda", 300) }
    };
    
    /// <summary>
    /// Crea un ExtractedFrame con valores estándar para una posición
    /// </summary>
    public static ExtractedFrame CreateForPosition(int position, Guid jobId)
    {
        if (!StandardViews.TryGetValue(position, out var view))
        {
            view = ($"Vista-{position}", (position - 1) * 60);
        }
        
        return new ExtractedFrame
        {
            Id = Guid.NewGuid(),
            Video360JobId = jobId,
            SequenceNumber = position,
            ViewName = view.Name,
            AngleDegrees = view.Angle,
            IsPrimary = position == 1 // El frente es el principal
        };
    }
}
