using Video360Service.Domain.Enums;

namespace Video360Service.Application.DTOs;

/// <summary>
/// DTO para crear un trabajo de procesamiento de video 360
/// </summary>
public record CreateVideo360JobRequest
{
    /// <summary>
    /// ID del vehículo asociado
    /// </summary>
    public Guid VehicleId { get; init; }
    
    /// <summary>
    /// Número de frames a extraer (default 6)
    /// </summary>
    public int FrameCount { get; init; } = 6;
    
    /// <summary>
    /// Ancho de salida de las imágenes
    /// </summary>
    public int? OutputWidth { get; init; }
    
    /// <summary>
    /// Alto de salida de las imágenes
    /// </summary>
    public int? OutputHeight { get; init; }
    
    /// <summary>
    /// Calidad JPEG (1-100)
    /// </summary>
    public int? JpegQuality { get; init; }
    
    /// <summary>
    /// Formato de salida (jpg, png, webp)
    /// </summary>
    public string? OutputFormat { get; init; }
    
    /// <summary>
    /// Generar miniaturas
    /// </summary>
    public bool GenerateThumbnails { get; init; } = true;
    
    /// <summary>
    /// Usar selección inteligente de frames
    /// </summary>
    public bool SmartFrameSelection { get; init; } = true;
    
    /// <summary>
    /// Corregir exposición automáticamente
    /// </summary>
    public bool AutoCorrectExposure { get; init; } = true;
}

/// <summary>
/// DTO de respuesta para un trabajo de video 360
/// </summary>
public record Video360JobResponse
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public Guid UserId { get; init; }
    public string VideoUrl { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public double? DurationSeconds { get; init; }
    public int FramesToExtract { get; init; }
    public Video360JobStatus Status { get; init; }
    public string StatusName => Status.ToString();
    public int Progress { get; init; }
    public string? ErrorMessage { get; init; }
    public List<ExtractedFrameResponse> Frames { get; init; } = new();
    public int? QueuePosition { get; init; }
    public DateTime? ProcessingStartedAt { get; init; }
    public DateTime? ProcessingCompletedAt { get; init; }
    public long? ProcessingDurationMs { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO de respuesta para un frame extraído
/// </summary>
public record ExtractedFrameResponse
{
    public Guid Id { get; init; }
    public int SequenceNumber { get; init; }
    public int AngleDegrees { get; init; }
    public string ViewName { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public long FileSizeBytes { get; init; }
    public string Format { get; init; } = string.Empty;
    public int? QualityScore { get; init; }
    public bool IsPrimary { get; init; }
    public double TimestampSeconds { get; init; }
}

/// <summary>
/// DTO de respuesta después de subir un video
/// </summary>
public record UploadVideoResponse
{
    public Guid JobId { get; init; }
    public string Message { get; init; } = string.Empty;
    public Video360JobStatus Status { get; init; }
    public int QueuePosition { get; init; }
    public int EstimatedWaitSeconds { get; init; }
}

/// <summary>
/// DTO de estado del trabajo
/// </summary>
public record JobStatusResponse
{
    public Guid JobId { get; init; }
    public Video360JobStatus Status { get; init; }
    public string StatusName => Status.ToString();
    public int Progress { get; init; }
    public int? QueuePosition { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsComplete => Status == Video360JobStatus.Completed;
    public bool IsFailed => Status == Video360JobStatus.Failed;
    public bool IsProcessing => Status is Video360JobStatus.Processing 
        or Video360JobStatus.ExtractingFrames 
        or Video360JobStatus.ProcessingImages 
        or Video360JobStatus.UploadingImages;
}

/// <summary>
/// DTO con las imágenes del viewer 360
/// </summary>
public record Vehicle360ViewerResponse
{
    public Guid VehicleId { get; init; }
    public Guid JobId { get; init; }
    public List<ViewerFrameResponse> Frames { get; init; } = new();
    public int TotalFrames => Frames.Count;
    public string? PrimaryImageUrl { get; init; }
    public DateTime ProcessedAt { get; init; }
}

/// <summary>
/// DTO de frame para el viewer
/// </summary>
public record ViewerFrameResponse
{
    public int Index { get; init; }
    public int Angle { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
}

/// <summary>
/// Request para procesar video desde URL (API interna para orquestador)
/// </summary>
public record ProcessVideoFromUrlRequest
{
    /// <summary>
    /// URL del video ya subido a S3
    /// </summary>
    public string VideoUrl { get; init; } = string.Empty;
    
    /// <summary>
    /// ID del vehículo asociado
    /// </summary>
    public Guid VehicleId { get; init; }
    
    /// <summary>
    /// Número de frames a extraer (default 6)
    /// </summary>
    public int FrameCount { get; init; } = 6;
    
    /// <summary>
    /// Ancho de salida de las imágenes
    /// </summary>
    public int? OutputWidth { get; init; }
    
    /// <summary>
    /// Alto de salida de las imágenes
    /// </summary>
    public int? OutputHeight { get; init; }
    
    /// <summary>
    /// Calidad JPEG (1-100)
    /// </summary>
    public int? JpegQuality { get; init; }
    
    /// <summary>
    /// Formato de salida (jpg, png, webp)
    /// </summary>
    public string? OutputFormat { get; init; }
    
    /// <summary>
    /// Generar miniaturas
    /// </summary>
    public bool GenerateThumbnails { get; init; } = true;
    
    /// <summary>
    /// Usar selección inteligente de frames
    /// </summary>
    public bool SmartFrameSelection { get; init; } = true;
    
    /// <summary>
    /// Corregir exposición automáticamente
    /// </summary>
    public bool AutoCorrectExposure { get; init; } = true;
}

/// <summary>
/// Response para procesamiento de video (compatible con orquestador)
/// </summary>
public record ProcessVideoResponse
{
    public Guid? JobId { get; init; }
    public Video360JobStatus Status { get; init; }
    public int Progress { get; init; }
    public List<FrameInfo> Frames { get; init; } = new();
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
}

/// <summary>
/// Información de frame individual
/// </summary>
public record FrameInfo
{
    public int SequenceNumber { get; init; }
    public string ViewName { get; init; } = string.Empty;
    public int AngleDegrees { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
}
