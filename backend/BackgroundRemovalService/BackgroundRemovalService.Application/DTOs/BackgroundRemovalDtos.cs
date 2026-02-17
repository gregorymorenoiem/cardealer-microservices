using BackgroundRemovalService.Domain.Enums;

namespace BackgroundRemovalService.Application.DTOs;

/// <summary>
/// DTO para crear un nuevo job de remoción de fondo
/// </summary>
public record CreateRemovalJobRequest
{
    /// <summary>
    /// URL de la imagen a procesar (alternativa a ImageBase64)
    /// </summary>
    public string? ImageUrl { get; init; }
    
    /// <summary>
    /// Imagen en Base64 (alternativa a ImageUrl)
    /// </summary>
    public string? ImageBase64 { get; init; }
    
    /// <summary>
    /// Nombre del archivo original
    /// </summary>
    public string? FileName { get; init; }
    
    /// <summary>
    /// Proveedor preferido (null = auto-select)
    /// </summary>
    public BackgroundRemovalProvider? PreferredProvider { get; init; }
    
    /// <summary>
    /// Formato de salida
    /// </summary>
    public OutputFormat OutputFormat { get; init; } = OutputFormat.Png;
    
    /// <summary>
    /// Tamaño de salida
    /// </summary>
    public ImageSize OutputSize { get; init; } = ImageSize.Original;
    
    /// <summary>
    /// Color de fondo (null = transparente)
    /// </summary>
    public string? BackgroundColor { get; init; }
    
    /// <summary>
    /// Agregar sombra
    /// </summary>
    public bool AddShadow { get; init; } = false;
    
    /// <summary>
    /// Recortar al objeto
    /// </summary>
    public bool CropToForeground { get; init; } = false;
    
    /// <summary>
    /// Margen de recorte en px
    /// </summary>
    public int CropMargin { get; init; } = 0;
    
    /// <summary>
    /// Ancho de salida (null = auto)
    /// </summary>
    public int? Width { get; init; }
    
    /// <summary>
    /// Alto de salida (null = auto)
    /// </summary>
    public int? Height { get; init; }
    
    /// <summary>
    /// Calidad de compresión (1-100)
    /// </summary>
    public int Quality { get; init; } = 90;
    
    /// <summary>
    /// Tipo de objeto para mejor detección
    /// </summary>
    public string? ObjectType { get; init; }
    
    /// <summary>
    /// URL de callback cuando termine
    /// </summary>
    public string? CallbackUrl { get; init; }
    
    /// <summary>
    /// ID de correlación para tracking
    /// </summary>
    public string? CorrelationId { get; init; }
    
    /// <summary>
    /// Prioridad (mayor = más prioritario)
    /// </summary>
    public int Priority { get; init; } = 0;
    
    /// <summary>
    /// Metadatos adicionales
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
    
    /// <summary>
    /// Si debe procesarse de forma síncrona (esperar resultado)
    /// </summary>
    public bool ProcessSync { get; init; } = true;
}

/// <summary>
/// DTO de respuesta para un job de remoción
/// </summary>
public record RemovalJobResponse
{
    public Guid JobId { get; init; }
    public ProcessingStatus Status { get; init; }
    public string StatusText => Status.ToString();
    public BackgroundRemovalProvider? Provider { get; init; }
    public string? SourceImageUrl { get; init; }
    public string? ResultImageUrl { get; init; }
    public string? ResultBase64 { get; init; }
    public long? ProcessingTimeMs { get; init; }
    public decimal? CreditsConsumed { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// DTO para listar jobs con paginación
/// </summary>
public record RemovalJobListResponse
{
    public IEnumerable<RemovalJobResponse> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// DTO para información de proveedor
/// </summary>
public record ProviderInfoResponse
{
    public BackgroundRemovalProvider Provider { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public bool IsAvailable { get; init; }
    public decimal CostPerImageUsd { get; init; }
    public int RateLimitPerMinute { get; init; }
    public int RateLimitPerDay { get; init; }
    public int RequestsUsedToday { get; init; }
    public decimal? AvailableCredits { get; init; }
    public double SuccessRate { get; init; }
    public double AverageResponseTimeMs { get; init; }
    public string[] SupportedInputFormats { get; init; } = [];
    public string[] SupportedOutputFormats { get; init; } = [];
}

/// <summary>
/// DTO para estadísticas de uso
/// </summary>
public record UsageStatisticsResponse
{
    public Guid? UserId { get; init; }
    public int BillingPeriod { get; init; }
    public int TotalRequests { get; init; }
    public int SuccessfulRequests { get; init; }
    public int FailedRequests { get; init; }
    public decimal TotalCreditsConsumed { get; init; }
    public decimal TotalCostUsd { get; init; }
    public double AverageProcessingTimeMs { get; init; }
    public Dictionary<string, int> RequestsByProvider { get; init; } = [];
    public Dictionary<string, decimal> CostByProvider { get; init; } = [];
}

/// <summary>
/// DTO para health check de proveedores
/// </summary>
public record ProviderHealthResponse
{
    public BackgroundRemovalProvider Provider { get; init; }
    public bool IsHealthy { get; init; }
    public string Status { get; init; } = string.Empty;
    public long? LatencyMs { get; init; }
    public decimal? AvailableCredits { get; init; }
    public DateTime CheckedAt { get; init; } = DateTime.UtcNow;
    public string? ErrorMessage { get; init; }
}
