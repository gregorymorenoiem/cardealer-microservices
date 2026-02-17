namespace BackgroundRemovalService.Domain.Enums;

/// <summary>
/// Estado del procesamiento de remoción de fondo
/// </summary>
public enum ProcessingStatus
{
    /// <summary>
    /// En cola esperando ser procesado
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Actualmente procesándose
    /// </summary>
    Processing = 1,
    
    /// <summary>
    /// Completado exitosamente
    /// </summary>
    Completed = 2,
    
    /// <summary>
    /// Falló el procesamiento
    /// </summary>
    Failed = 3,
    
    /// <summary>
    /// Cancelado por el usuario
    /// </summary>
    Cancelled = 4,
    
    /// <summary>
    /// Reintentando después de un fallo
    /// </summary>
    Retrying = 5
}
