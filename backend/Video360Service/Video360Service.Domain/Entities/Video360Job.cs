using Video360Service.Domain.Enums;

namespace Video360Service.Domain.Entities;

/// <summary>
/// Representa un trabajo de extracción de frames 360.
/// Un video se procesa y se extraen 6 imágenes equidistantes para crear una vista 360 interactiva.
/// </summary>
public class Video360Job
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// ID del usuario que solicitó el procesamiento
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// ID del vehículo asociado (para vincular con VehiclesSaleService)
    /// </summary>
    public Guid? VehicleId { get; set; }
    
    /// <summary>
    /// ID del tenant (para multi-tenancy)
    /// </summary>
    public string? TenantId { get; set; }
    
    /// <summary>
    /// URL o path del video original subido
    /// </summary>
    public string SourceVideoUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// URL del video almacenado después del upload
    /// </summary>
    public string? VideoUrl { get; set; }
    
    /// <summary>
    /// Nombre original del archivo de video
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Tamaño original del video en bytes
    /// </summary>
    public long OriginalFileSizeBytes { get; set; }
    
    /// <summary>
    /// Content type original (video/mp4, video/webm, etc.)
    /// </summary>
    public string OriginalContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Duración del video en segundos
    /// </summary>
    public double VideoDurationSeconds { get; set; }
    
    /// <summary>
    /// Resolución del video original (ej: 1920x1080)
    /// </summary>
    public string? VideoResolution { get; set; }
    
    /// <summary>
    /// Ancho del video
    /// </summary>
    public int? VideoWidth { get; set; }
    
    /// <summary>
    /// Alto del video
    /// </summary>
    public int? VideoHeight { get; set; }
    
    /// <summary>
    /// FPS del video original
    /// </summary>
    public int? VideoFps { get; set; }
    
    /// <summary>
    /// Proveedor utilizado para el procesamiento
    /// </summary>
    public Video360Provider Provider { get; set; } = Video360Provider.FfmpegApi;
    
    /// <summary>
    /// Proveedor preferido por el usuario
    /// </summary>
    public Video360Provider? PreferredProvider { get; set; }
    
    /// <summary>
    /// Proveedor de respaldo si el principal falla
    /// </summary>
    public Video360Provider? FallbackProvider { get; set; }
    
    /// <summary>
    /// Estado actual del procesamiento
    /// </summary>
    public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;
    
    /// <summary>
    /// Formato de salida para las imágenes
    /// </summary>
    public ImageFormat ImageFormat { get; set; } = ImageFormat.Jpeg;
    
    /// <summary>
    /// Alias para mantener compatibilidad
    /// </summary>
    public ImageFormat OutputFormat
    {
        get => ImageFormat;
        set => ImageFormat = value;
    }
    
    /// <summary>
    /// Calidad de las imágenes de salida
    /// </summary>
    public VideoQuality VideoQuality { get; set; } = VideoQuality.High;
    
    /// <summary>
    /// Alias para mantener compatibilidad
    /// </summary>
    public VideoQuality OutputQuality
    {
        get => VideoQuality;
        set => VideoQuality = value;
    }
    
    /// <summary>
    /// Número de frames a extraer (por defecto 6 para vista 360)
    /// </summary>
    public int FrameCount { get; set; } = 6;
    
    /// <summary>
    /// Las imágenes extraídas (relación 1:N)
    /// </summary>
    public List<ExtractedFrame> ExtractedFrames { get; set; } = new();
    
    /// <summary>
    /// Alias para Frames
    /// </summary>
    public List<ExtractedFrame> Frames => ExtractedFrames;
    
    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Código de error del proveedor
    /// </summary>
    public string? ErrorCode { get; set; }
    
    /// <summary>
    /// Número de intentos realizados
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Máximo de reintentos permitidos
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Tiempo de procesamiento en milisegundos
    /// </summary>
    public long? ProcessingTimeMs { get; set; }
    
    /// <summary>
    /// Costo del procesamiento en USD
    /// </summary>
    public decimal? CostUsd { get; set; }
    
    /// <summary>
    /// URL de callback cuando termine
    /// </summary>
    public string? CallbackUrl { get; set; }
    
    /// <summary>
    /// ID de correlación para tracking distribuido
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Prioridad del job (mayor = más prioritario)
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// Metadatos adicionales (JSON)
    /// </summary>
    public string? MetadataJson { get; set; }
    
    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha de inicio del procesamiento
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// Fecha de completado
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Fecha de expiración de los archivos
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Si el job fue procesado de forma síncrona
    /// </summary>
    public bool ProcessedSync { get; set; } = false;
    
    /// <summary>
    /// Verifica si el job puede ser reintentado
    /// </summary>
    public bool CanRetry => Status == ProcessingStatus.Failed && RetryCount < MaxRetries;
    
    /// <summary>
    /// Verifica si el job está completado exitosamente
    /// </summary>
    public bool IsCompleted => Status == ProcessingStatus.Completed;
    
    /// <summary>
    /// Verifica si el job está en progreso
    /// </summary>
    public bool IsInProgress => Status == ProcessingStatus.Uploading 
                               || Status == ProcessingStatus.Processing 
                               || Status == ProcessingStatus.Downloading;
    
    /// <summary>
    /// Marca el job como iniciado
    /// </summary>
    public void MarkAsStarted()
    {
        Status = ProcessingStatus.Processing;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marca el job como completado
    /// </summary>
    public void MarkAsCompleted(long processingTimeMs)
    {
        Status = ProcessingStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ProcessingTimeMs = processingTimeMs;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marca el job como fallido
    /// </summary>
    public void MarkAsFailed(string errorMessage, string? errorCode = null)
    {
        Status = ProcessingStatus.Failed;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Incrementa el contador de reintentos
    /// </summary>
    public void IncrementRetry()
    {
        RetryCount++;
        Status = ProcessingStatus.Retrying;
        ErrorMessage = null;
        ErrorCode = null;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Incrementa el contador de reintentos (alias)
    /// </summary>
    public void IncrementRetryCount() => IncrementRetry();
    
    /// <summary>
    /// Inicia el procesamiento del job
    /// </summary>
    public void StartProcessing()
    {
        Status = ProcessingStatus.Processing;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Actualiza la URL del video almacenado
    /// </summary>
    public void UpdateVideoUrl(string videoUrl)
    {
        VideoUrl = videoUrl;
        SourceVideoUrl = videoUrl;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Establece el proveedor a utilizar
    /// </summary>
    public void SetProvider(Video360Provider provider)
    {
        Provider = provider;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Completa el procesamiento con información adicional
    /// </summary>
    public void CompleteProcessing(
        long processingTimeMs,
        decimal? costUsd = null,
        double? durationSeconds = null,
        int? width = null,
        int? height = null)
    {
        Status = ProcessingStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ProcessingTimeMs = processingTimeMs;
        CostUsd = costUsd;
        
        if (durationSeconds.HasValue)
            VideoDurationSeconds = durationSeconds.Value;
        
        if (width.HasValue)
            VideoWidth = width;
        
        if (height.HasValue)
            VideoHeight = height;
        
        if (width.HasValue && height.HasValue)
            VideoResolution = $"{width}x{height}";
        
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Fecha de completado del procesamiento (alias)
    /// </summary>
    public DateTime? ProcessingCompletedAt => CompletedAt;
    
    /// <summary>
    /// Agrega un frame extraído al job
    /// </summary>
    public void AddFrame(ExtractedFrame frame)
    {
        frame.Video360JobId = Id;
        frame.Video360Job = this;
        ExtractedFrames.Add(frame);
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marca el job como fallido (sobrecarga)
    /// </summary>
    public void MarkFailed(string errorMessage, string? errorCode)
    {
        Status = ProcessingStatus.Failed;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        UpdatedAt = DateTime.UtcNow;
    }
}
