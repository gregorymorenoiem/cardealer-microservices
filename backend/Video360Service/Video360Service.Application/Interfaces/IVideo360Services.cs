using Video360Service.Domain.Entities;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Application.Interfaces;

/// <summary>
/// Orquestador principal del procesamiento de videos 360.
/// Maneja la lógica de selección de proveedor, retry, y fallback.
/// </summary>
public interface IVideo360Orchestrator
{
    /// <summary>
    /// Procesa un video y extrae frames para vista 360
    /// </summary>
    /// <param name="job">El job a procesar</param>
    /// <param name="videoBytes">Bytes del video</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El job actualizado con los resultados</returns>
    Task<Video360Job> ProcessVideoAsync(
        Video360Job job, 
        byte[] videoBytes, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Procesa un video desde URL
    /// </summary>
    Task<Video360Job> ProcessVideoFromUrlAsync(
        Video360Job job, 
        string videoUrl, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reintenta un job fallido
    /// </summary>
    Task<Video360Job> RetryJobAsync(
        Guid jobId, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Factory para crear instancias de proveedores
/// </summary>
public interface IVideo360ProviderFactory
{
    /// <summary>
    /// Obtiene un proveedor específico
    /// </summary>
    IVideo360Provider? GetProvider(Domain.Enums.Video360Provider providerType);
    
    /// <summary>
    /// Obtiene el mejor proveedor disponible
    /// </summary>
    Task<IVideo360Provider?> GetBestAvailableProviderAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene todos los proveedores registrados
    /// </summary>
    IEnumerable<IVideo360Provider> GetAllProviders();
    
    /// <summary>
    /// Obtiene todos los proveedores disponibles
    /// </summary>
    Task<IEnumerable<IVideo360Provider>> GetAvailableProvidersAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio de almacenamiento para videos e imágenes
/// </summary>
public interface IVideoStorageService
{
    /// <summary>
    /// Sube un video al storage
    /// </summary>
    Task<string> UploadVideoAsync(byte[] videoBytes, string fileName, string contentType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sube una imagen al storage
    /// </summary>
    Task<string> UploadImageAsync(byte[] imageBytes, string fileName, string contentType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Descarga un archivo desde una URL
    /// </summary>
    Task<byte[]> DownloadAsync(string url, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Elimina un archivo del storage
    /// </summary>
    Task DeleteAsync(string url, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Genera una URL firmada para acceso temporal
    /// </summary>
    Task<string> GenerateSignedUrlAsync(string url, TimeSpan expiration, CancellationToken cancellationToken = default);
}
