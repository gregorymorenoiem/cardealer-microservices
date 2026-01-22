namespace DataProtectionService.Domain.Entities;

/// <summary>
/// Registro de cambios en datos personales del usuario
/// Para trazabilidad según Ley 172-13
/// </summary>
public class DataChangeLog
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// ID del usuario cuyos datos fueron modificados
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Campo de datos modificado (Email, Phone, Address, etc.)
    /// </summary>
    public string DataField { get; set; } = string.Empty;
    
    /// <summary>
    /// Categoría del dato (Personal, Contact, Financial, etc.)
    /// </summary>
    public string DataCategory { get; set; } = string.Empty;
    
    /// <summary>
    /// Hash del valor anterior (no se guarda el valor real por privacidad)
    /// </summary>
    public string OldValueHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Hash del nuevo valor
    /// </summary>
    public string NewValueHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Valor anterior enmascarado (ej: j***@email.com)
    /// </summary>
    public string? OldValueMasked { get; set; }
    
    /// <summary>
    /// Nuevo valor enmascarado
    /// </summary>
    public string? NewValueMasked { get; set; }
    
    /// <summary>
    /// Tipo de quien hizo el cambio (User, Admin, System)
    /// </summary>
    public string ChangedByType { get; set; } = "User";
    
    /// <summary>
    /// ID de quien hizo el cambio
    /// </summary>
    public Guid? ChangedById { get; set; }
    
    /// <summary>
    /// Nombre de quien hizo el cambio
    /// </summary>
    public string? ChangedByName { get; set; }
    
    /// <summary>
    /// Razón del cambio
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Servicio que originó el cambio
    /// </summary>
    public string SourceService { get; set; } = string.Empty;
    
    /// <summary>
    /// IP desde donde se realizó el cambio
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// User Agent
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// ID de correlación para trazabilidad
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Fecha del cambio
    /// </summary>
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
// PrivacyPolicy and AnonymizationRecord are defined in DataProtectionEntities.cs
