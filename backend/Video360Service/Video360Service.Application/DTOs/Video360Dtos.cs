using Video360Service.Domain.Enums;

namespace Video360Service.Application.DTOs;

/// <summary>
/// DTO para crear un nuevo job de extracción de frames 360
/// </summary>
public record CreateVideo360JobRequest
{
    /// <summary>
    /// URL del video a procesar (alternativa a VideoBase64)
    /// </summary>
    public string? VideoUrl { get; init; }
    
    /// <summary>
    /// Video en Base64 (alternativa a VideoUrl)
    /// </summary>
    public string? VideoBase64 { get; init; }
    
    /// <summary>
    /// Nombre del archivo original
    /// </summary>
    public string? FileName { get; init; }
    
    /// <summary>
    /// ID del vehículo asociado (para vincular con VehiclesSaleService)
    /// </summary>
    public Guid? VehicleId { get; init; }
    
    /// <summary>
    /// Proveedor preferido (null = auto-select mejor disponible)
    /// </summary>
    public Video360Provider? PreferredProvider { get; init; }
    
    /// <summary>
    /// Número de frames a extraer (por defecto 6)
    /// </summary>
    public int FrameCount { get; init; } = 6;
    
    /// <summary>
    /// Formato de salida de las imágenes
    /// </summary>
    public ImageFormat OutputFormat { get; init; } = ImageFormat.Jpeg;
    
    /// <summary>
    /// Calidad de las imágenes de salida
    /// </summary>
    public VideoQuality OutputQuality { get; init; } = VideoQuality.High;
    
    /// <summary>
    /// Calidad de compresión (1-100)
    /// </summary>
    public int Quality { get; init; } = 90;
    
    /// <summary>
    /// Ancho de salida (null = proporcional al alto o original)
    /// </summary>
    public int? Width { get; init; }
    
    /// <summary>
    /// Alto de salida (null = proporcional al ancho o original)
    /// </summary>
    public int? Height { get; init; }
    
    /// <summary>
    /// Si debe generar thumbnails para cada frame
    /// </summary>
    public bool GenerateThumbnails { get; init; } = true;
    
    /// <summary>
    /// Si debe optimizar las imágenes resultantes
    /// </summary>
    public bool OptimizeImages { get; init; } = true;
    
    /// <summary>
    /// Timestamps específicos en segundos (null = equidistantes automáticos)
    /// </summary>
    public double[]? SpecificTimestamps { get; init; }
    
    /// <summary>
    /// URL de callback cuando termine el procesamiento
    /// </summary>
    public string? CallbackUrl { get; init; }
    
    /// <summary>
    /// ID de correlación para tracking distribuido
    /// </summary>
    public string? CorrelationId { get; init; }
    
    /// <summary>
    /// Prioridad del job (mayor = más prioritario)
    /// </summary>
    public int Priority { get; init; } = 0;
    
    /// <summary>
    /// Metadatos adicionales
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
    
    /// <summary>
    /// Si debe procesarse de forma síncrona (esperar resultado)
    /// </summary>
    public bool ProcessSync { get; init; } = true;
}

/// <summary>
/// DTO de respuesta para un job de extracción 360
/// </summary>
public record Video360JobResponse
{
    public Guid JobId { get; init; }
    public Guid? VehicleId { get; init; }
    public ProcessingStatus Status { get; init; }
    public string StatusText => Status.ToString();
    public Video360Provider Provider { get; init; }
    public string ProviderName => Provider.ToString();
    public string? SourceVideoUrl { get; init; }
    public double? VideoDurationSeconds { get; init; }
    public string? VideoResolution { get; init; }
    public List<ExtractedFrameResponse> Frames { get; init; } = new();
    public int TotalFrames => Frames.Count;
    public long? ProcessingTimeMs { get; init; }
    public decimal? CostUsd { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// DTO de respuesta para un frame extraído
/// </summary>
public record ExtractedFrameResponse
{
    public Guid FrameId { get; init; }
    public int Index { get; init; }
    public int AngleDegrees { get; init; }
    public string AngleLabel { get; init; } = string.Empty;
    public double TimestampSeconds { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public string? ImageBase64 { get; init; }
    public long FileSizeBytes { get; init; }
    public string ContentType { get; init; } = "image/jpeg";
    public int Width { get; init; }
    public int Height { get; init; }
}

/// <summary>
/// DTO para listar jobs con paginación
/// </summary>
public record Video360JobListResponse
{
    public IEnumerable<Video360JobResponse> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// DTO para información de proveedor
/// </summary>
public record ProviderInfoResponse
{
    public Video360Provider Provider { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public bool IsAvailable { get; init; }
    public decimal CostPerVideoUsd { get; init; }
    public int? RemainingCredits { get; init; }
    public int DailyLimit { get; init; }
    public int DailyUsageCount { get; init; }
    public string[] SupportedFormats { get; init; } = [];
    public int MaxVideoSizeMb { get; init; }
    public int MaxVideoDurationSeconds { get; init; }
}

/// <summary>
/// DTO para estadísticas de uso
/// </summary>
public record UsageStatsResponse
{
    public int TotalJobsProcessed { get; init; }
    public int SuccessfulJobs { get; init; }
    public int FailedJobs { get; init; }
    public decimal TotalCostUsd { get; init; }
    public long TotalProcessingTimeMs { get; init; }
    public int TotalFramesExtracted { get; init; }
    public Dictionary<string, int> JobsByProvider { get; init; } = new();
    public string BillingPeriod { get; init; } = string.Empty;
}

/// <summary>
/// DTO para respuesta de vista 360 completa de un vehículo
/// </summary>
public record Vehicle360ViewResponse
{
    public Guid VehicleId { get; init; }
    public Guid JobId { get; init; }
    public ProcessingStatus Status { get; init; }
    public bool IsReady => Status == ProcessingStatus.Completed;
    public List<Frame360Data> Frames { get; init; } = new();
    public int TotalFrames => Frames.Count;
    public DateTime? CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// Datos simplificados de frame para vista 360
/// </summary>
public record Frame360Data
{
    public int Index { get; init; }
    public int Angle { get; init; }
    public string Label { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string? Thumbnail { get; init; }
}
