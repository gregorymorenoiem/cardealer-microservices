namespace Video360Service.Domain.Enums;

/// <summary>
/// Estado del procesamiento de extracción de frames
/// </summary>
public enum ProcessingStatus
{
    /// <summary>
    /// En cola esperando ser procesado
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Subiendo video al proveedor
    /// </summary>
    Uploading = 1,
    
    /// <summary>
    /// Actualmente procesándose (extrayendo frames)
    /// </summary>
    Processing = 2,
    
    /// <summary>
    /// Descargando imágenes resultantes
    /// </summary>
    Downloading = 3,
    
    /// <summary>
    /// Completado exitosamente
    /// </summary>
    Completed = 4,
    
    /// <summary>
    /// Falló el procesamiento
    /// </summary>
    Failed = 5,
    
    /// <summary>
    /// Cancelado por el usuario
    /// </summary>
    Cancelled = 6,
    
    /// <summary>
    /// Reintentando después de un fallo
    /// </summary>
    Retrying = 7
}
