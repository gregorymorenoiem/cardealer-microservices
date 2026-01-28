namespace BackgroundRemovalService.Application.Interfaces;

/// <summary>
/// Servicio para almacenamiento de imágenes procesadas
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Guarda una imagen y retorna la URL pública
    /// </summary>
    Task<string> SaveImageAsync(
        byte[] imageBytes, 
        string fileName, 
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Descarga una imagen desde URL
    /// </summary>
    Task<byte[]?> DownloadImageAsync(
        string imageUrl, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Elimina una imagen
    /// </summary>
    Task<bool> DeleteImageAsync(
        string imageUrl, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Genera una URL pre-firmada para upload directo
    /// </summary>
    Task<string> GenerateUploadUrlAsync(
        string fileName, 
        string contentType,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si una imagen existe
    /// </summary>
    Task<bool> ImageExistsAsync(
        string imageUrl, 
        CancellationToken cancellationToken = default);
}
