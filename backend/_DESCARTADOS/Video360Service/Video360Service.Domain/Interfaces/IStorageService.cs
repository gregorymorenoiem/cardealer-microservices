namespace Video360Service.Domain.Interfaces;

/// <summary>
/// Interface para servicio de almacenamiento (S3, Azure Blob, etc.)
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Sube un archivo al storage
    /// </summary>
    Task<StorageUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sube un archivo desde ruta local
    /// </summary>
    Task<StorageUploadResult> UploadFromPathAsync(
        string localPath,
        string? folder = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Descarga un archivo a una ruta local
    /// </summary>
    Task<string> DownloadToPathAsync(
        string fileUrl,
        string localPath,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Elimina un archivo del storage
    /// </summary>
    Task<bool> DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene una URL temporal para acceso
    /// </summary>
    Task<string> GetPresignedUrlAsync(
        string fileUrl,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado de subida al storage
/// </summary>
public class StorageUploadResult
{
    public bool Success { get; set; }
    public string? Url { get; set; }
    public string? Key { get; set; }
    public long FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ContentType { get; set; }
}
