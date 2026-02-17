// =====================================================
// AntiMoneyLaunderingService - Enums
// Ley 155-17 Prevención de Lavado de Activos (PLD)
// =====================================================

namespace AntiMoneyLaunderingService.Domain.Enums;

/// <summary>
/// Tipo de identificación
/// </summary>
public enum IdentificationType
{
    /// <summary>Cédula de identidad dominicana</summary>
    Cedula,
    
    /// <summary>Pasaporte</summary>
    Passport,
    
    /// <summary>RNC - Registro Nacional de Contribuyentes</summary>
    Rnc,
    
    /// <summary>Residencia extranjera</summary>
    ForeignId
}

/// <summary>
/// Nivel de riesgo del cliente (Ley 155-17)
/// </summary>
public enum RiskLevel
{
    /// <summary>Riesgo bajo - Monitoreo estándar</summary>
    Low,
    
    /// <summary>Riesgo medio - Monitoreo reforzado</summary>
    Medium,
    
    /// <summary>Riesgo alto - Debida diligencia ampliada</summary>
    High,
    
    /// <summary>Prohibido - No se puede operar</summary>
    Prohibited
}

/// <summary>
/// Estado del proceso KYC (Know Your Customer)
/// </summary>
public enum KycStatus
{
    /// <summary>Pendiente de iniciar</summary>
    Pending,
    
    /// <summary>En proceso de verificación</summary>
    InProgress,
    
    /// <summary>Verificado satisfactoriamente</summary>
    Verified,
    
    /// <summary>Rechazado</summary>
    Rejected,
    
    /// <summary>Expirado - Requiere renovación</summary>
    Expired
}

/// <summary>
/// Tipo de ROS (Reporte de Operación Sospechosa)
/// </summary>
public enum RosReportType
{
    /// <summary>Transacción sospechosa</summary>
    SuspiciousTransaction,
    
    /// <summary>Patrón inusual de comportamiento</summary>
    UnusualPattern,
    
    /// <summary>Transacción estructurada (pitufeo)</summary>
    StructuredTransaction,
    
    /// <summary>Relacionado con PEP</summary>
    PepRelated,
    
    /// <summary>Posible financiamiento de terrorismo</summary>
    TerrorismFinancing,
    
    /// <summary>Intento de evasión</summary>
    EvasionAttempt,
    
    /// <summary>Documentación falsa</summary>
    FalseDocumentation
}

/// <summary>
/// Estado del ROS
/// </summary>
public enum RosStatus
{
    /// <summary>Borrador - En preparación</summary>
    Draft,
    
    /// <summary>Pendiente de revisión interna</summary>
    PendingReview,
    
    /// <summary>Aprobado para envío</summary>
    Approved,
    
    /// <summary>Enviado a UAF</summary>
    Submitted,
    
    /// <summary>Recibido por UAF</summary>
    Acknowledged,
    
    /// <summary>Cerrado</summary>
    Closed
}

/// <summary>
/// Categoría de PEP (Persona Expuesta Políticamente)
/// </summary>
public enum PepCategory
{
    /// <summary>Gobierno nacional</summary>
    NationalGovernment,
    
    /// <summary>Gobierno local/municipal</summary>
    LocalGovernment,
    
    /// <summary>Poder judicial</summary>
    Judicial,
    
    /// <summary>Militar o fuerzas de seguridad</summary>
    Military,
    
    /// <summary>Empresa estatal</summary>
    StateOwned,
    
    /// <summary>Organización internacional</summary>
    InternationalOrganization,
    
    /// <summary>Partido político</summary>
    PoliticalParty
}

/// <summary>
/// Tipo de alerta AML
/// </summary>
public enum AlertType
{
    /// <summary>Transacción sobre umbral</summary>
    ThresholdExceeded,
    
    /// <summary>Patrón de pitufeo detectado</summary>
    StructuringDetected,
    
    /// <summary>Cliente en lista de sanciones</summary>
    SanctionsMatch,
    
    /// <summary>PEP identificado</summary>
    PepIdentified,
    
    /// <summary>Actividad inusual</summary>
    UnusualActivity,
    
    /// <summary>Geografía de alto riesgo</summary>
    HighRiskGeography,
    
    /// <summary>KYC vencido</summary>
    KycExpired
}

/// <summary>
/// Estado de la alerta
/// </summary>
public enum AlertStatus
{
    /// <summary>Nueva - Sin revisar</summary>
    New,
    
    /// <summary>En investigación</summary>
    UnderInvestigation,
    
    /// <summary>Escalada a oficial de cumplimiento</summary>
    Escalated,
    
    /// <summary>Falso positivo</summary>
    FalsePositive,
    
    /// <summary>Confirmada - Requiere ROS</summary>
    Confirmed,
    
    /// <summary>Cerrada</summary>
    Closed
}
