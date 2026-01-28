using Video360Service.Domain.Enums;

namespace Video360Service.Domain.Entities;

/// <summary>
/// Representa un trabajo de procesamiento de video 360
/// </summary>
public class Video360Job
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid UserId { get; set; }
    
    /// <summary>
    /// URL del video original
    /// </summary>
    public string VideoUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del archivo original
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long FileSizeBytes { get; set; }
    
    /// <summary>
    /// Duración del video en segundos
    /// </summary>
    public double? DurationSeconds { get; set; }
    
    /// <summary>
    /// Número de frames a extraer (default 6)
    /// </summary>
    public int FramesToExtract { get; set; } = 6;
    
    /// <summary>
    /// Estado actual del procesamiento
    /// </summary>
    public Video360JobStatus Status { get; set; } = Video360JobStatus.Pending;
    
    /// <summary>
    /// Progreso del procesamiento (0-100)
    /// </summary>
    public int Progress { get; set; }
    
    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Imágenes extraídas del video
    /// </summary>
    public ICollection<ExtractedFrame> ExtractedFrames { get; set; } = new List<ExtractedFrame>();
    
    /// <summary>
    /// Opciones de procesamiento aplicadas
    /// </summary>
    public ProcessingOptions Options { get; set; } = new();
    
    /// <summary>
    /// Tiempo de inicio del procesamiento
    /// </summary>
    public DateTime? ProcessingStartedAt { get; set; }
    
    /// <summary>
    /// Tiempo de finalización del procesamiento
    /// </summary>
    public DateTime? ProcessingCompletedAt { get; set; }
    
    /// <summary>
    /// Duración del procesamiento en milisegundos
    /// </summary>
    public long? ProcessingDurationMs { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Marca el trabajo como en procesamiento
    /// </summary>
    public void StartProcessing()
    {
        Status = Video360JobStatus.Processing;
        ProcessingStartedAt = DateTime.UtcNow;
        Progress = 0;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marca el trabajo como completado exitosamente
    /// </summary>
    public void Complete()
    {
        Status = Video360JobStatus.Completed;
        ProcessingCompletedAt = DateTime.UtcNow;
        Progress = 100;
        
        if (ProcessingStartedAt.HasValue)
        {
            ProcessingDurationMs = (long)(ProcessingCompletedAt.Value - ProcessingStartedAt.Value).TotalMilliseconds;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marca el trabajo como fallido
    /// </summary>
    public void Fail(string errorMessage)
    {
        Status = Video360JobStatus.Failed;
        ErrorMessage = errorMessage;
        ProcessingCompletedAt = DateTime.UtcNow;
        
        if (ProcessingStartedAt.HasValue)
        {
            ProcessingDurationMs = (long)(ProcessingCompletedAt.Value - ProcessingStartedAt.Value).TotalMilliseconds;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Actualiza el progreso del trabajo
    /// </summary>
    public void UpdateProgress(int progress)
    {
        Progress = Math.Clamp(progress, 0, 100);
        UpdatedAt = DateTime.UtcNow;
    }
}
