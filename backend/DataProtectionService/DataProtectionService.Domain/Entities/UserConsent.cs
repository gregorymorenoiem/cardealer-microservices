namespace DataProtectionService.Domain.Entities;

/// <summary>
/// Tipo de consentimiento según Ley 172-13
/// </summary>
public enum ConsentType
{
    TermsOfService = 1,
    PrivacyPolicy = 2,
    MarketingCommunications = 3,
    DataProcessing = 4,
    ThirdPartySharing = 5,
    Cookies = 6,
    LocationTracking = 7,
    PersonalizedAds = 8
}

/// <summary>
/// Consentimiento del usuario para tratamiento de datos personales
/// Cumple con Ley 172-13 de República Dominicana
/// </summary>
public class UserConsent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ConsentType Type { get; set; }
    
    /// <summary>
    /// Versión del documento aceptado (ej: "2.0.1")
    /// </summary>
    public string Version { get; set; } = string.Empty;
    
    /// <summary>
    /// Hash SHA256 del documento aceptado para verificar integridad
    /// </summary>
    public string DocumentHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Si el consentimiento fue otorgado
    /// </summary>
    public bool Granted { get; set; }
    
    /// <summary>
    /// Fecha y hora de otorgamiento
    /// </summary>
    public DateTime GrantedAt { get; set; }
    
    /// <summary>
    /// Fecha y hora de revocación (si aplica)
    /// </summary>
    public DateTime? RevokedAt { get; set; }
    
    /// <summary>
    /// Razón de revocación
    /// </summary>
    public string? RevokeReason { get; set; }
    
    /// <summary>
    /// IP desde donde se otorgó el consentimiento
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// User Agent del navegador/dispositivo
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;
    
    /// <summary>
    /// Método de recolección (web, mobile, api)
    /// </summary>
    public string CollectionMethod { get; set; } = "web";
    
    /// <summary>
    /// Si el consentimiento está activo (no revocado)
    /// </summary>
    public bool IsActive => Granted && !RevokedAt.HasValue;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
