using BackgroundRemovalService.Domain.Enums;

namespace BackgroundRemovalService.Domain.Entities;

/// <summary>
/// Registro de uso de la API para auditoría y billing
/// </summary>
public class UsageRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Referencia al job procesado
    /// </summary>
    public Guid JobId { get; set; }
    
    /// <summary>
    /// Usuario que realizó la solicitud
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Tenant ID
    /// </summary>
    public string? TenantId { get; set; }
    
    /// <summary>
    /// Proveedor utilizado
    /// </summary>
    public BackgroundRemovalProvider Provider { get; set; }
    
    /// <summary>
    /// Si fue exitoso
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Tamaño de imagen de entrada en bytes
    /// </summary>
    public long InputSizeBytes { get; set; }
    
    /// <summary>
    /// Tamaño de imagen de salida en bytes
    /// </summary>
    public long? OutputSizeBytes { get; set; }
    
    /// <summary>
    /// Tiempo de procesamiento en ms
    /// </summary>
    public long ProcessingTimeMs { get; set; }
    
    /// <summary>
    /// Créditos consumidos
    /// </summary>
    public decimal CreditsConsumed { get; set; }
    
    /// <summary>
    /// Costo en USD
    /// </summary>
    public decimal CostUsd { get; set; }
    
    /// <summary>
    /// IP del cliente
    /// </summary>
    public string? ClientIpAddress { get; set; }
    
    /// <summary>
    /// User agent del cliente
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Fecha del registro
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Mes/año para agregaciones (YYYYMM)
    /// </summary>
    public int BillingPeriod { get; set; }
    
    public UsageRecord()
    {
        BillingPeriod = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
    }
}
