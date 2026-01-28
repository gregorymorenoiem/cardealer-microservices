namespace Vehicle360ProcessingService.Domain.Interfaces;

/// <summary>
/// Cliente resiliente para comunicación con Video360Service
/// </summary>
public interface IVideo360ServiceClient
{
    /// <summary>
    /// Inicia el procesamiento de un video 360 y espera el resultado
    /// </summary>
    Task<Video360ProcessingResult> ProcessVideoAsync(
        string videoUrl,
        Guid vehicleId,
        int frameCount = 6,
        Video360Options? options = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene el estado de un job de Video360
    /// </summary>
    Task<Video360JobStatus> GetJobStatusAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene el resultado completo de un job
    /// </summary>
    Task<Video360ProcessingResult> GetJobResultAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si Video360Service está disponible
    /// </summary>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}

public class Video360ProcessingResult
{
    public bool Success { get; set; }
    public Guid? JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public List<Video360Frame> Frames { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public long? ProcessingDurationMs { get; set; }
}

public class Video360Frame
{
    public int SequenceNumber { get; set; }
    public string ViewName { get; set; } = string.Empty;
    public int AngleDegrees { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double? QualityScore { get; set; }
}

public class Video360Options
{
    public int OutputWidth { get; set; } = 1920;
    public int OutputHeight { get; set; } = 1080;
    public string OutputFormat { get; set; } = "jpg";
    public int JpegQuality { get; set; } = 90;
    public bool SmartFrameSelection { get; set; } = true;
    public bool AutoCorrectExposure { get; set; } = true;
    public bool GenerateThumbnails { get; set; } = true;
}

public class Video360JobStatus
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public bool IsComplete { get; set; }
    public bool IsFailed { get; set; }
    public string? ErrorMessage { get; set; }
}
