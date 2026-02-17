using Video360Service.Domain.Enums;

namespace Video360Service.Domain.Entities;

/// <summary>
/// Configuración de un proveedor de extracción de frames.
/// Permite habilitar/deshabilitar proveedores y establecer prioridades.
/// </summary>
public class ProviderConfiguration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Tipo de proveedor
    /// </summary>
    public Video360Provider Provider { get; set; }
    
    /// <summary>
    /// Si el proveedor está habilitado
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Prioridad del proveedor (mayor = más prioritario)
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// Costo por procesamiento en USD
    /// </summary>
    public decimal CostPerVideoUsd { get; set; }
    
    /// <summary>
    /// Límite de procesamiento por día (0 = sin límite)
    /// </summary>
    public int DailyLimit { get; set; } = 0;
    
    /// <summary>
    /// Contador de uso diario
    /// </summary>
    public int DailyUsageCount { get; set; } = 0;
    
    /// <summary>
    /// Fecha del último reset del contador diario
    /// </summary>
    public DateTime LastDailyReset { get; set; } = DateTime.UtcNow.Date;
    
    /// <summary>
    /// Límite de tamaño de video en MB (0 = sin límite)
    /// </summary>
    public int MaxVideoSizeMb { get; set; } = 100;
    
    /// <summary>
    /// Duración máxima de video en segundos (0 = sin límite)
    /// </summary>
    public int MaxVideoDurationSeconds { get; set; } = 120;
    
    /// <summary>
    /// Timeout para el procesamiento en segundos
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;
    
    /// <summary>
    /// Formatos de video soportados (ej: "mp4,webm,mov")
    /// </summary>
    public string SupportedFormats { get; set; } = "mp4,webm,mov,avi";
    
    /// <summary>
    /// Notas o comentarios
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Verifica si el proveedor puede procesar más videos hoy
    /// </summary>
    public bool CanProcessToday()
    {
        ResetDailyCounterIfNeeded();
        return DailyLimit == 0 || DailyUsageCount < DailyLimit;
    }
    
    /// <summary>
    /// Incrementa el contador de uso diario
    /// </summary>
    public void IncrementUsage()
    {
        ResetDailyCounterIfNeeded();
        DailyUsageCount++;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Resetea el contador diario si es un nuevo día
    /// </summary>
    private void ResetDailyCounterIfNeeded()
    {
        if (LastDailyReset.Date < DateTime.UtcNow.Date)
        {
            DailyUsageCount = 0;
            LastDailyReset = DateTime.UtcNow.Date;
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Verifica si un formato de video está soportado
    /// </summary>
    public bool IsFormatSupported(string format)
    {
        var formats = SupportedFormats.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries);
        return formats.Contains(format.ToLowerInvariant().TrimStart('.'));
    }
}
