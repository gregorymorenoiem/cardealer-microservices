namespace Video360Service.Domain.Enums;

/// <summary>
/// Estado del trabajo de procesamiento de video 360
/// </summary>
public enum Video360JobStatus
{
    /// <summary>
    /// Trabajo creado, pendiente de procesamiento
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Video subido, esperando en cola
    /// </summary>
    Queued = 1,
    
    /// <summary>
    /// Video está siendo procesado
    /// </summary>
    Processing = 2,
    
    /// <summary>
    /// Extrayendo frames del video
    /// </summary>
    ExtractingFrames = 3,
    
    /// <summary>
    /// Procesando/mejorando las imágenes
    /// </summary>
    ProcessingImages = 4,
    
    /// <summary>
    /// Subiendo imágenes al storage
    /// </summary>
    UploadingImages = 5,
    
    /// <summary>
    /// Procesamiento completado exitosamente
    /// </summary>
    Completed = 10,
    
    /// <summary>
    /// Procesamiento falló
    /// </summary>
    Failed = 20,
    
    /// <summary>
    /// Trabajo cancelado por el usuario
    /// </summary>
    Cancelled = 30
}
