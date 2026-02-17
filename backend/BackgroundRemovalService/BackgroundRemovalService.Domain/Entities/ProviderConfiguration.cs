using BackgroundRemovalService.Domain.Enums;

namespace BackgroundRemovalService.Domain.Entities;

/// <summary>
/// Configuración de un proveedor de remoción de fondo.
/// Permite configurar múltiples proveedores con diferentes prioridades.
/// </summary>
public class ProviderConfiguration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Tipo de proveedor
    /// </summary>
    public BackgroundRemovalProvider Provider { get; set; }
    
    /// <summary>
    /// Nombre descriptivo del proveedor
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Si el proveedor está activo
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Prioridad (menor = más prioritario)
    /// </summary>
    public int Priority { get; set; } = 100;
    
    /// <summary>
    /// API Key encriptada
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// URL base de la API
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Límite de requests por minuto
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 60;
    
    /// <summary>
    /// Límite de requests por día
    /// </summary>
    public int RateLimitPerDay { get; set; } = 1000;
    
    /// <summary>
    /// Requests usados hoy
    /// </summary>
    public int RequestsUsedToday { get; set; } = 0;
    
    /// <summary>
    /// Última vez que se reseteo el contador diario
    /// </summary>
    public DateTime LastDailyReset { get; set; } = DateTime.UtcNow.Date;
    
    /// <summary>
    /// Timeout en segundos para la API
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;
    
    /// <summary>
    /// Costo por imagen procesada en USD
    /// </summary>
    public decimal CostPerImageUsd { get; set; }
    
    /// <summary>
    /// Créditos disponibles (si el proveedor usa sistema de créditos)
    /// </summary>
    public decimal? AvailableCredits { get; set; }
    
    /// <summary>
    /// Tamaño máximo de imagen soportado en megapíxeles
    /// </summary>
    public int MaxImageSizeMegapixels { get; set; } = 25;
    
    /// <summary>
    /// Tamaño máximo de archivo en MB
    /// </summary>
    public int MaxFileSizeMb { get; set; } = 12;
    
    /// <summary>
    /// Formatos de entrada soportados
    /// </summary>
    public string SupportedInputFormats { get; set; } = "jpg,jpeg,png,webp";
    
    /// <summary>
    /// Formatos de salida soportados
    /// </summary>
    public string SupportedOutputFormats { get; set; } = "png,webp,jpg";
    
    /// <summary>
    /// Opciones adicionales del proveedor (JSON)
    /// </summary>
    public string? AdditionalOptions { get; set; }
    
    /// <summary>
    /// Estadísticas de uso
    /// </summary>
    public int TotalRequestsProcessed { get; set; } = 0;
    
    /// <summary>
    /// Promedio de tiempo de respuesta en ms
    /// </summary>
    public double AverageResponseTimeMs { get; set; } = 0;
    
    /// <summary>
    /// Tasa de éxito (0-100)
    /// </summary>
    public double SuccessRate { get; set; } = 100;
    
    /// <summary>
    /// Número de fallos consecutivos
    /// </summary>
    public int ConsecutiveFailures { get; set; } = 0;
    
    /// <summary>
    /// Si está en circuit breaker (pausado por fallos)
    /// </summary>
    public bool IsCircuitBreakerOpen { get; set; } = false;
    
    /// <summary>
    /// Cuándo se puede reintentar si está en circuit breaker
    /// </summary>
    public DateTime? CircuitBreakerResetAt { get; set; }
    
    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // === Métodos de dominio ===
    
    public bool IsAvailable()
    {
        if (!IsEnabled) return false;
        if (IsCircuitBreakerOpen && CircuitBreakerResetAt > DateTime.UtcNow) return false;
        if (RequestsUsedToday >= RateLimitPerDay) return false;
        return true;
    }
    
    public void RecordSuccess(long responseTimeMs)
    {
        TotalRequestsProcessed++;
        RequestsUsedToday++;
        ConsecutiveFailures = 0;
        IsCircuitBreakerOpen = false;
        
        // Calcular nuevo promedio
        var totalTime = AverageResponseTimeMs * (TotalRequestsProcessed - 1) + responseTimeMs;
        AverageResponseTimeMs = totalTime / TotalRequestsProcessed;
        
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RecordFailure()
    {
        TotalRequestsProcessed++;
        ConsecutiveFailures++;
        
        // Abrir circuit breaker después de 5 fallos consecutivos
        if (ConsecutiveFailures >= 5)
        {
            IsCircuitBreakerOpen = true;
            CircuitBreakerResetAt = DateTime.UtcNow.AddMinutes(5);
        }
        
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void ResetDailyCounter()
    {
        if (LastDailyReset.Date < DateTime.UtcNow.Date)
        {
            RequestsUsedToday = 0;
            LastDailyReset = DateTime.UtcNow.Date;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
