using BackgroundRemovalService.Domain.Enums;

namespace BackgroundRemovalService.Domain.Interfaces;

/// <summary>
/// Interfaz del Strategy Pattern para proveedores de remoción de fondo.
/// Cada proveedor (Remove.bg, Photoroom, etc.) implementa esta interfaz.
/// </summary>
public interface IBackgroundRemovalProvider
{
    /// <summary>
    /// Identificador del proveedor
    /// </summary>
    BackgroundRemovalProvider ProviderType { get; }
    
    /// <summary>
    /// Nombre descriptivo del proveedor
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// Verifica si el proveedor está configurado y disponible
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remueve el fondo de una imagen desde bytes
    /// </summary>
    /// <param name="imageBytes">Bytes de la imagen original</param>
    /// <param name="options">Opciones de procesamiento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del procesamiento</returns>
    Task<BackgroundRemovalResult> RemoveBackgroundAsync(
        byte[] imageBytes, 
        BackgroundRemovalOptions options,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remueve el fondo de una imagen desde URL
    /// </summary>
    /// <param name="imageUrl">URL de la imagen</param>
    /// <param name="options">Opciones de procesamiento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del procesamiento</returns>
    Task<BackgroundRemovalResult> RemoveBackgroundFromUrlAsync(
        string imageUrl, 
        BackgroundRemovalOptions options,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene los créditos/saldo disponibles
    /// </summary>
    Task<ProviderAccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Opciones para el procesamiento de remoción de fondo
/// </summary>
public record BackgroundRemovalOptions
{
    /// <summary>
    /// Formato de salida deseado
    /// </summary>
    public OutputFormat OutputFormat { get; init; } = OutputFormat.Png;
    
    /// <summary>
    /// Tamaño de salida deseado
    /// </summary>
    public ImageSize OutputSize { get; init; } = ImageSize.Original;
    
    /// <summary>
    /// Color de fondo (null = transparente)
    /// Formato: #RRGGBB o nombre de color
    /// </summary>
    public string? BackgroundColor { get; init; }
    
    /// <summary>
    /// Imagen de fondo a usar (URL)
    /// </summary>
    public string? BackgroundImageUrl { get; init; }
    
    /// <summary>
    /// Si es true, agrega sombra al objeto
    /// </summary>
    public bool AddShadow { get; init; } = false;
    
    /// <summary>
    /// Tipo de sombra (natural, flat, etc.)
    /// </summary>
    public string? ShadowType { get; init; }
    
    /// <summary>
    /// Si es true, recorta la imagen al objeto
    /// </summary>
    public bool CropToForeground { get; init; } = false;
    
    /// <summary>
    /// Margen en pixeles si se recorta
    /// </summary>
    public int CropMargin { get; init; } = 0;
    
    /// <summary>
    /// Ancho específico de salida (0 = auto)
    /// </summary>
    public int? Width { get; init; }
    
    /// <summary>
    /// Alto específico de salida (0 = auto)
    /// </summary>
    public int? Height { get; init; }
    
    /// <summary>
    /// Calidad de compresión (1-100, solo para JPEG)
    /// </summary>
    public int Quality { get; init; } = 90;
    
    /// <summary>
    /// Tipo de objeto para mejorar detección (car, person, product, etc.)
    /// </summary>
    public string? ObjectType { get; init; }
    
    /// <summary>
    /// Opciones adicionales específicas del proveedor
    /// </summary>
    public Dictionary<string, string>? ProviderSpecificOptions { get; init; }
}

/// <summary>
/// Resultado del procesamiento de remoción de fondo
/// </summary>
public record BackgroundRemovalResult
{
    /// <summary>
    /// Si el procesamiento fue exitoso
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// Bytes de la imagen resultante
    /// </summary>
    public byte[]? ImageBytes { get; init; }
    
    /// <summary>
    /// Content type del resultado (image/png, etc.)
    /// </summary>
    public string? ContentType { get; init; }
    
    /// <summary>
    /// Ancho de la imagen resultante
    /// </summary>
    public int? Width { get; init; }
    
    /// <summary>
    /// Alto de la imagen resultante
    /// </summary>
    public int? Height { get; init; }
    
    /// <summary>
    /// Tiempo de procesamiento en ms
    /// </summary>
    public long ProcessingTimeMs { get; init; }
    
    /// <summary>
    /// Créditos consumidos
    /// </summary>
    public decimal CreditsConsumed { get; init; }
    
    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Código de error del proveedor
    /// </summary>
    public string? ErrorCode { get; init; }
    
    /// <summary>
    /// Créditos restantes en la cuenta
    /// </summary>
    public decimal? RemainingCredits { get; init; }
    
    /// <summary>
    /// Metadatos adicionales del proveedor
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
    
    public static BackgroundRemovalResult Success(byte[] imageBytes, string contentType, long processingTimeMs, decimal creditsConsumed) 
        => new()
        {
            IsSuccess = true,
            ImageBytes = imageBytes,
            ContentType = contentType,
            ProcessingTimeMs = processingTimeMs,
            CreditsConsumed = creditsConsumed
        };
    
    public static BackgroundRemovalResult Failure(string errorMessage, string? errorCode = null) 
        => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
}

/// <summary>
/// Información de la cuenta del proveedor
/// </summary>
public record ProviderAccountInfo
{
    /// <summary>
    /// Créditos disponibles
    /// </summary>
    public decimal AvailableCredits { get; init; }
    
    /// <summary>
    /// Créditos usados este periodo
    /// </summary>
    public decimal UsedCredits { get; init; }
    
    /// <summary>
    /// Límite de créditos del periodo
    /// </summary>
    public decimal? CreditLimit { get; init; }
    
    /// <summary>
    /// Fecha de reset del periodo
    /// </summary>
    public DateTime? PeriodResetDate { get; init; }
    
    /// <summary>
    /// Plan de suscripción
    /// </summary>
    public string? SubscriptionPlan { get; init; }
    
    /// <summary>
    /// Si la cuenta está activa
    /// </summary>
    public bool IsActive { get; init; }
}
