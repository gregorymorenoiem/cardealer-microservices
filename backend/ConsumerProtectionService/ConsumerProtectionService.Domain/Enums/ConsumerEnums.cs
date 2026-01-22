// =====================================================
// ConsumerProtectionService - Enums
// Ley 358-05 Derechos del Consumidor de RD
// =====================================================

namespace ConsumerProtectionService.Domain.Enums;

/// <summary>
/// Tipo de garantía según Ley 358-05
/// </summary>
public enum WarrantyType
{
    /// <summary>Garantía legal mínima (6 meses productos nuevos)</summary>
    Legal,
    
    /// <summary>Garantía extendida (adicional a la legal)</summary>
    Extended,
    
    /// <summary>Garantía del fabricante</summary>
    Manufacturer,
    
    /// <summary>Garantía del distribuidor</summary>
    Distributor,
    
    /// <summary>Garantía comercial voluntaria</summary>
    Commercial
}

/// <summary>
/// Estado de la garantía
/// </summary>
public enum WarrantyStatus
{
    /// <summary>Activa y vigente</summary>
    Active,
    
    /// <summary>Vencida por tiempo</summary>
    Expired,
    
    /// <summary>Reclamada por el consumidor</summary>
    Claimed,
    
    /// <summary>Anulada por uso indebido</summary>
    Voided,
    
    /// <summary>Utilizada exitosamente</summary>
    Fulfilled
}

/// <summary>
/// Tipo de reclamación
/// </summary>
public enum ComplaintType
{
    /// <summary>Producto defectuoso</summary>
    DefectiveProduct,
    
    /// <summary>Problema con garantía</summary>
    WarrantyIssue,
    
    /// <summary>Publicidad engañosa</summary>
    MisleadingAdvertising,
    
    /// <summary>Disputa de precio</summary>
    PriceDispute,
    
    /// <summary>Solicitud de reembolso</summary>
    RefundRequest,
    
    /// <summary>Calidad de servicio</summary>
    ServiceQuality,
    
    /// <summary>Información incorrecta</summary>
    MisinformationClaim,
    
    /// <summary>Incumplimiento de contrato</summary>
    ContractBreach
}

/// <summary>
/// Estado de la reclamación
/// </summary>
public enum ComplaintStatus
{
    /// <summary>Recibida</summary>
    Received,
    
    /// <summary>En revisión</summary>
    UnderReview,
    
    /// <summary>En mediación</summary>
    InMediation,
    
    /// <summary>Resuelta</summary>
    Resolved,
    
    /// <summary>Escalada a Pro-Consumidor</summary>
    Escalated,
    
    /// <summary>Cerrada</summary>
    Closed,
    
    /// <summary>Rechazada</summary>
    Rejected
}

/// <summary>
/// Prioridad de la reclamación
/// </summary>
public enum ComplaintPriority
{
    /// <summary>Baja</summary>
    Low,
    
    /// <summary>Media</summary>
    Medium,
    
    /// <summary>Alta</summary>
    High,
    
    /// <summary>Urgente</summary>
    Urgent
}

/// <summary>
/// Estado de la mediación
/// </summary>
public enum MediationStatus
{
    /// <summary>Programada</summary>
    Scheduled,
    
    /// <summary>En progreso</summary>
    InProgress,
    
    /// <summary>Acuerdo alcanzado</summary>
    Agreement,
    
    /// <summary>Sin acuerdo</summary>
    NoAgreement,
    
    /// <summary>Cancelada</summary>
    Cancelled
}

/// <summary>
/// Tipo de resolución
/// </summary>
public enum ResolutionType
{
    /// <summary>Reembolso total</summary>
    FullRefund,
    
    /// <summary>Reembolso parcial</summary>
    PartialRefund,
    
    /// <summary>Reemplazo de producto</summary>
    ProductReplacement,
    
    /// <summary>Reparación</summary>
    Repair,
    
    /// <summary>Compensación</summary>
    Compensation,
    
    /// <summary>Sin resolución</summary>
    NoResolution
}
