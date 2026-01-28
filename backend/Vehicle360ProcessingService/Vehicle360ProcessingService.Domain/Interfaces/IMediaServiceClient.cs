namespace Vehicle360ProcessingService.Domain.Interfaces;

/// <summary>
/// Cliente resiliente para comunicación con MediaService
/// </summary>
public interface IMediaServiceClient
{
    /// <summary>
    /// Sube un video a S3 a través de MediaService
    /// </summary>
    Task<MediaUploadResult> UploadVideoAsync(
        Stream videoStream, 
        string fileName, 
        string contentType,
        string folder = "videos/360",
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sube una imagen procesada a S3
    /// </summary>
    Task<MediaUploadResult> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        string folder = "images/360",
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sube una imagen desde URL (descarga y re-sube)
    /// </summary>
    Task<MediaUploadResult> UploadImageFromUrlAsync(
        string imageUrl,
        string fileName,
        string folder = "images/360",
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene la URL pública de un archivo
    /// </summary>
    Task<string?> GetFileUrlAsync(string storageKey, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Descarga un archivo como stream
    /// </summary>
    Task<Stream?> DownloadFileAsync(string url, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si MediaService está disponible
    /// </summary>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}

public class MediaUploadResult
{
    public bool Success { get; set; }
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? PublicId { get; set; }
    public string? StorageKey { get; set; }
    public long? Size { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
}
