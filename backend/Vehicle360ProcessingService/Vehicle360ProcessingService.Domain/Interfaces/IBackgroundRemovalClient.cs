namespace Vehicle360ProcessingService.Domain.Interfaces;

/// <summary>
/// Cliente resiliente para comunicación con BackgroundRemovalService
/// </summary>
public interface IBackgroundRemovalClient
{
    /// <summary>
    /// Remueve el fondo de una imagen
    /// </summary>
    Task<BackgroundRemovalResult> RemoveBackgroundAsync(
        string imageUrl,
        BackgroundRemovalOptions? options = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remueve el fondo de múltiples imágenes en paralelo
    /// </summary>
    Task<List<BackgroundRemovalResult>> RemoveBackgroundBatchAsync(
        List<string> imageUrls,
        BackgroundRemovalOptions? options = null,
        int maxConcurrency = 3,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene el estado de un job de remoción
    /// </summary>
    Task<BgJobStatus> GetJobStatusAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si BackgroundRemovalService está disponible
    /// </summary>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}

public class BackgroundRemovalResult
{
    public bool Success { get; set; }
    public Guid? JobId { get; set; }
    public string? OriginalImageUrl { get; set; }
    public string? ProcessedImageUrl { get; set; }
    public string? ProcessedImageBase64 { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public long? ProcessingTimeMs { get; set; }
    public string? Provider { get; set; }
}

public class BgJobStatus
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public bool IsComplete { get; set; }
    public bool IsFailed { get; set; }
    public string? ProcessedImageUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

public class BackgroundRemovalOptions
{
    public string OutputFormat { get; set; } = "png";
    public string BackgroundColor { get; set; } = "transparent";
    public int? OutputWidth { get; set; }
    public int? OutputHeight { get; set; }
    public bool MaintainAspectRatio { get; set; } = true;
    public string? Provider { get; set; } // remove.bg, photoroom, etc.
    public bool CropToSubject { get; set; } = false;
    public bool AddShadow { get; set; } = false;
}
