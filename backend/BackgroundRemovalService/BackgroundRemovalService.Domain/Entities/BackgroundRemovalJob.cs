using BackgroundRemovalService.Domain.Enums;

namespace BackgroundRemovalService.Domain.Entities;

/// <summary>
/// Representa un trabajo de remoción de fondo.
/// Almacena el estado y resultado del procesamiento.
/// </summary>
public class BackgroundRemovalJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// ID del usuario que solicitó el procesamiento
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// ID del tenant (para multi-tenancy)
    /// </summary>
    public string? TenantId { get; set; }
    
    /// <summary>
    /// URL o path de la imagen original
    /// </summary>
    public string SourceImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre original del archivo
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Tamaño original en bytes
    /// </summary>
    public long OriginalFileSizeBytes { get; set; }
    
    /// <summary>
    /// Content type original (image/jpeg, image/png, etc.)
    /// </summary>
    public string OriginalContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// URL de la imagen resultante (sin fondo)
    /// </summary>
    public string? ResultImageUrl { get; set; }
    
    /// <summary>
    /// Tamaño del resultado en bytes
    /// </summary>
    public long? ResultFileSizeBytes { get; set; }
    
    /// <summary>
    /// Content type del resultado
    /// </summary>
    public string? ResultContentType { get; set; }
    
    /// <summary>
    /// Proveedor utilizado para el procesamiento
    /// </summary>
    public BackgroundRemovalProvider Provider { get; set; } = BackgroundRemovalProvider.RemoveBg;
    
    /// <summary>
    /// Proveedor de respaldo si el principal falla
    /// </summary>
    public BackgroundRemovalProvider? FallbackProvider { get; set; }
    
    /// <summary>
    /// Estado actual del procesamiento
    /// </summary>
    public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;
    
    /// <summary>
    /// Formato de salida solicitado
    /// </summary>
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Png;
    
    /// <summary>
    /// Tamaño de salida solicitado
    /// </summary>
    public ImageSize OutputSize { get; set; } = ImageSize.Original;
    
    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Código de error del proveedor
    /// </summary>
    public string? ErrorCode { get; set; }
    
    /// <summary>
    /// Número de intentos realizados
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Máximo de reintentos permitidos
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Tiempo de procesamiento en milisegundos
    /// </summary>
    public long? ProcessingTimeMs { get; set; }
    
    /// <summary>
    /// Créditos de API consumidos (para billing)
    /// </summary>
    public decimal? CreditsConsumed { get; set; }
    
    /// <summary>
    /// Costo estimado en USD
    /// </summary>
    public decimal? EstimatedCostUsd { get; set; }
    
    /// <summary>
    /// Metadatos adicionales (JSON)
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Prioridad del trabajo (mayor = más prioritario)
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// ID de correlación para tracking
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Webhook URL para notificar cuando termine
    /// </summary>
    public string? CallbackUrl { get; set; }
    
    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha en que comenzó el procesamiento
    /// </summary>
    public DateTime? ProcessingStartedAt { get; set; }
    
    /// <summary>
    /// Fecha en que terminó el procesamiento
    /// </summary>
    public DateTime? ProcessingCompletedAt { get; set; }
    
    /// <summary>
    /// Fecha de expiración del resultado (para cleanup)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    // === Métodos de dominio ===
    
    public void MarkAsProcessing()
    {
        Status = ProcessingStatus.Processing;
        ProcessingStartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsCompleted(string resultUrl, long resultSize, string contentType, long processingTimeMs)
    {
        Status = ProcessingStatus.Completed;
        ResultImageUrl = resultUrl;
        ResultFileSizeBytes = resultSize;
        ResultContentType = contentType;
        ProcessingTimeMs = processingTimeMs;
        ProcessingCompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsFailed(string errorMessage, string? errorCode = null)
    {
        Status = ProcessingStatus.Failed;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        ProcessingCompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void IncrementRetry()
    {
        RetryCount++;
        Status = ProcessingStatus.Retrying;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool CanRetry() => RetryCount < MaxRetries;
    
    public void Cancel()
    {
        Status = ProcessingStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
