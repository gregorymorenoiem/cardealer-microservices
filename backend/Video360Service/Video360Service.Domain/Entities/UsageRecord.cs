using Video360Service.Domain.Enums;

namespace Video360Service.Domain.Entities;

/// <summary>
/// Registro de uso del servicio para facturación y analytics.
/// </summary>
public class UsageRecord
{
    public UsageRecord()
    {
    }
    
    /// <summary>
    /// Constructor con parámetros principales
    /// </summary>
    public UsageRecord(
        Video360Provider provider,
        Guid vehicleId,
        long? processingTimeMs,
        decimal? costUsd,
        bool isSuccess)
    {
        Provider = provider;
        VehicleId = vehicleId;
        ProcessingTimeMs = processingTimeMs ?? 0;
        CostUsd = costUsd ?? 0;
        IsSuccess = isSuccess;
    }
    
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// ID del job asociado
    /// </summary>
    public Guid Video360JobId { get; set; }
    
    /// <summary>
    /// Alias para compatibilidad
    /// </summary>
    public Guid JobId
    {
        get => Video360JobId;
        set => Video360JobId = value;
    }
    
    /// <summary>
    /// ID del vehículo
    /// </summary>
    public Guid? VehicleId { get; set; }
    
    /// <summary>
    /// ID del usuario
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// ID del tenant
    /// </summary>
    public string? TenantId { get; set; }
    
    /// <summary>
    /// Proveedor utilizado
    /// </summary>
    public Video360Provider Provider { get; set; }
    
    /// <summary>
    /// Si el procesamiento fue exitoso
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Tamaño del video procesado en bytes
    /// </summary>
    public long VideoSizeBytes { get; set; }
    
    /// <summary>
    /// Duración del video en segundos
    /// </summary>
    public double VideoDurationSeconds { get; set; }
    
    /// <summary>
    /// Número de frames extraídos
    /// </summary>
    public int FramesExtracted { get; set; }
    
    /// <summary>
    /// Alias para compatibilidad
    /// </summary>
    public int FrameCount
    {
        get => FramesExtracted;
        set => FramesExtracted = value;
    }
    
    /// <summary>
    /// Tiempo de procesamiento en ms
    /// </summary>
    public long ProcessingTimeMs { get; set; }
    
    /// <summary>
    /// Costo del procesamiento en USD
    /// </summary>
    public decimal CostUsd { get; set; }
    
    /// <summary>
    /// Fecha del uso
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Mes de facturación (YYYY-MM)
    /// </summary>
    public string BillingPeriod { get; set; } = DateTime.UtcNow.ToString("yyyy-MM");
    
    /// <summary>
    /// Si ya fue facturado
    /// </summary>
    public bool IsBilled { get; set; } = false;
    
    /// <summary>
    /// Fecha de facturación
    /// </summary>
    public DateTime? BilledAt { get; set; }
    
    /// <summary>
    /// ID de la factura
    /// </summary>
    public string? InvoiceId { get; set; }
}
