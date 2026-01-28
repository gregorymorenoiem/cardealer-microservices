namespace Vehicle360ProcessingService.Domain.Entities;

/// <summary>
/// Representa un job de procesamiento 360 completo para un vehículo.
/// Orquesta: Upload → Extracción de frames → Remoción de fondo → Resultado final
/// </summary>
public class Vehicle360Job
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VehicleId { get; set; }
    public Guid UserId { get; set; }
    public Guid? TenantId { get; set; }
    
    // Estado general del job
    public Vehicle360JobStatus Status { get; set; } = Vehicle360JobStatus.Pending;
    public int Progress { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    
    // Video original
    public string? OriginalVideoUrl { get; set; }
    public string? OriginalFileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? VideoContentType { get; set; }
    
    // IDs de jobs en servicios externos
    public string? MediaUploadId { get; set; }
    public Guid? Video360JobId { get; set; }
    public List<Guid> BackgroundRemovalJobIds { get; set; } = new();
    
    // Configuración
    public int FrameCount { get; set; } = 6;
    public ProcessingOptions Options { get; set; } = new();
    
    // Resultados
    public List<ProcessedFrame> ProcessedFrames { get; set; } = new();
    public bool IsComplete => Status == Vehicle360JobStatus.Completed;
    public bool IsFailed => Status == Vehicle360JobStatus.Failed;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? ProcessingDurationMs { get; set; }
    
    // Metadata
    public string? ClientIpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    
    // Métodos de estado
    public void StartProcessing()
    {
        Status = Vehicle360JobStatus.Processing;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Progress = 5;
    }
    
    public void UpdateProgress(int progress, Vehicle360JobStatus? status = null)
    {
        Progress = Math.Clamp(progress, 0, 100);
        if (status.HasValue) Status = status.Value;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetVideoUploaded(string videoUrl, string? mediaUploadId = null)
    {
        OriginalVideoUrl = videoUrl;
        MediaUploadId = mediaUploadId;
        Status = Vehicle360JobStatus.VideoUploaded;
        Progress = 15;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetFramesExtracted(Guid video360JobId, List<ExtractedFrameInfo> frames)
    {
        Video360JobId = video360JobId;
        Status = Vehicle360JobStatus.FramesExtracted;
        Progress = 40;
        UpdatedAt = DateTime.UtcNow;
        
        // Crear ProcessedFrames con las URLs originales
        ProcessedFrames = frames.Select((f, i) => new ProcessedFrame
        {
            SequenceNumber = i + 1,
            ViewName = f.ViewName,
            AngleDegrees = f.AngleDegrees,
            OriginalImageUrl = f.ImageUrl,
            ThumbnailUrl = f.ThumbnailUrl,
            Status = FrameProcessingStatus.Pending
        }).ToList();
    }
    
    public void SetBackgroundRemovalStarted(List<Guid> removalJobIds)
    {
        BackgroundRemovalJobIds = removalJobIds;
        Status = Vehicle360JobStatus.RemovingBackgrounds;
        Progress = 50;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateFrameProcessed(int sequenceNumber, string processedImageUrl, Guid? removalJobId = null)
    {
        var frame = ProcessedFrames.FirstOrDefault(f => f.SequenceNumber == sequenceNumber);
        if (frame != null)
        {
            frame.ProcessedImageUrl = processedImageUrl;
            frame.BackgroundRemovalJobId = removalJobId;
            frame.Status = FrameProcessingStatus.Completed;
            frame.ProcessedAt = DateTime.UtcNow;
        }
        
        // Actualizar progreso basado en frames procesados
        var completedCount = ProcessedFrames.Count(f => f.Status == FrameProcessingStatus.Completed);
        var totalFrames = ProcessedFrames.Count;
        Progress = 50 + (int)((completedCount / (double)totalFrames) * 40); // 50-90%
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Complete()
    {
        Status = Vehicle360JobStatus.Completed;
        Progress = 100;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        if (StartedAt.HasValue)
        {
            ProcessingDurationMs = (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds;
        }
    }
    
    public void Fail(string errorMessage, string? errorCode = null)
    {
        Status = Vehicle360JobStatus.Failed;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        if (StartedAt.HasValue)
        {
            ProcessingDurationMs = (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds;
        }
    }
    
    public bool CanRetry()
    {
        return Status == Vehicle360JobStatus.Failed && RetryCount < MaxRetries;
    }
    
    public void PrepareRetry()
    {
        RetryCount++;
        Status = Vehicle360JobStatus.Pending;
        ErrorMessage = null;
        ErrorCode = null;
        Progress = 0;
        CompletedAt = null;
        StartedAt = null;
        ProcessingDurationMs = null;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Información de un frame extraído del Video360Service
/// </summary>
public class ExtractedFrameInfo
{
    public int SequenceNumber { get; set; }
    public string ViewName { get; set; } = string.Empty;
    public int AngleDegrees { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
}

/// <summary>
/// Frame procesado con fondo removido
/// </summary>
public class ProcessedFrame
{
    public int SequenceNumber { get; set; }
    public string ViewName { get; set; } = string.Empty;
    public int AngleDegrees { get; set; }
    public string OriginalImageUrl { get; set; } = string.Empty;
    public string? ProcessedImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid? BackgroundRemovalJobId { get; set; }
    public FrameProcessingStatus Status { get; set; } = FrameProcessingStatus.Pending;
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Opciones de procesamiento
/// </summary>
public class ProcessingOptions
{
    public int OutputWidth { get; set; } = 1920;
    public int OutputHeight { get; set; } = 1080;
    public string OutputFormat { get; set; } = "png"; // PNG para transparencia
    public int JpegQuality { get; set; } = 90;
    public bool SmartFrameSelection { get; set; } = true;
    public bool AutoCorrectExposure { get; set; } = true;
    public bool GenerateThumbnails { get; set; } = true;
    public string BackgroundColor { get; set; } = "transparent"; // transparent, white, #RRGGBB
}

public enum Vehicle360JobStatus
{
    Pending = 0,
    Queued = 1,
    Processing = 2,
    UploadingVideo = 3,
    VideoUploaded = 4,
    ExtractingFrames = 5,
    FramesExtracted = 6,
    RemovingBackgrounds = 7,
    UploadingResults = 8,
    Completed = 10,
    Failed = 20,
    Cancelled = 30
}

public enum FrameProcessingStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
