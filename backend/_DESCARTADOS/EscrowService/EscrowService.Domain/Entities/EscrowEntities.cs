// EscrowService - Domain Entities
// Servicio de depósito en garantía para transacciones seguras
// Cumple con normativas de protección al consumidor RD

namespace EscrowService.Domain.Entities;

using System;
using System.Collections.Generic;

#region Enums

/// <summary>
/// Tipo de transacción en escrow
/// </summary>
public enum EscrowTransactionType
{
    VehiclePurchase = 1,         // Compra de vehículo
    VehicleLease = 2,            // Arrendamiento
    ServiceDeposit = 3,          // Depósito por servicio
    TradeIn = 4,                 // Trade-in
    Warranty = 5,                // Garantía
    Inspection = 6,              // Depósito inspección
    Reservation = 7,             // Reserva de vehículo
    DownPayment = 8,             // Inicial
    FinancingGuarantee = 9       // Garantía de financiamiento
}

/// <summary>
/// Estado del escrow
/// </summary>
public enum EscrowStatus
{
    Pending = 1,                 // Pendiente de fondos
    Funded = 2,                  // Fondos recibidos
    InProgress = 3,              // En proceso (condiciones pendientes)
    ConditionsMet = 4,           // Condiciones cumplidas
    PendingRelease = 5,          // Pendiente de liberación
    Released = 6,                // Fondos liberados al vendedor
    Refunded = 7,                // Fondos devueltos al comprador
    PartialRelease = 8,          // Liberación parcial
    Disputed = 9,                // En disputa
    Cancelled = 10,              // Cancelado
    Expired = 11                 // Expirado
}

/// <summary>
/// Tipo de condición de liberación
/// </summary>
public enum ReleaseConditionType
{
    VehicleDelivery = 1,         // Entrega del vehículo
    TitleTransfer = 2,           // Transferencia de título
    InspectionApproved = 3,      // Inspección aprobada
    DocumentsVerified = 4,       // Documentos verificados
    BuyerApproval = 5,           // Aprobación del comprador
    SellerApproval = 6,          // Aprobación del vendedor
    TimeElapsed = 7,             // Tiempo transcurrido
    PaymentReceived = 8,         // Pago recibido
    ContractSigned = 9,          // Contrato firmado
    InsuranceVerified = 10,      // Seguro verificado
    FinancingApproved = 11,      // Financiamiento aprobado
    ManualApproval = 12          // Aprobación manual (admin)
}

/// <summary>
/// Estado de la condición
/// </summary>
public enum ConditionStatus
{
    Pending = 1,
    InProgress = 2,
    Met = 3,
    Failed = 4,
    Waived = 5,
    Expired = 6
}

/// <summary>
/// Tipo de movimiento de fondos
/// </summary>
public enum FundMovementType
{
    Deposit = 1,                 // Depósito inicial
    AdditionalDeposit = 2,       // Depósito adicional
    Release = 3,                 // Liberación al vendedor
    PartialRelease = 4,          // Liberación parcial
    Refund = 5,                  // Reembolso al comprador
    PartialRefund = 6,           // Reembolso parcial
    Fee = 7,                     // Comisión de escrow
    Adjustment = 8,              // Ajuste
    Penalty = 9,                 // Penalidad
    Interest = 10                // Intereses (si aplica)
}

/// <summary>
/// Método de pago
/// </summary>
public enum PaymentMethod
{
    BankTransfer = 1,            // Transferencia bancaria
    CreditCard = 2,              // Tarjeta de crédito
    DebitCard = 3,               // Tarjeta de débito
    Cash = 4,                    // Efectivo (en oficina)
    Check = 5,                   // Cheque certificado
    Wire = 6,                    // Transferencia internacional
    Crypto = 7                   // Criptomoneda (si permitido)
}

/// <summary>
/// Estado de la disputa
/// </summary>
public enum EscrowDisputeStatus
{
    Filed = 1,                   // Presentada
    UnderReview = 2,             // En revisión
    AwaitingDocuments = 3,       // Esperando documentos
    InMediation = 4,             // En mediación
    Resolved = 5,                // Resuelta
    Escalated = 6,               // Escalada
    Closed = 7                   // Cerrada
}

/// <summary>
/// Tipo de evento de auditoría
/// </summary>
public enum EscrowAuditEventType
{
    Created = 1,
    Funded = 2,
    ConditionMet = 3,
    ConditionFailed = 4,
    ReleaseRequested = 5,
    Released = 6,
    Refunded = 7,
    Disputed = 8,
    DisputeResolved = 9,
    Cancelled = 10,
    Expired = 11,
    DocumentUploaded = 12,
    StatusChanged = 13,
    AmountAdjusted = 14
}

#endregion

#region Entities

/// <summary>
/// Cuenta de escrow principal
/// </summary>
public class EscrowAccount
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;    // Número único de cuenta escrow
    public EscrowTransactionType TransactionType { get; set; }
    public EscrowStatus Status { get; set; } = EscrowStatus.Pending;
    
    // Partes involucradas
    public Guid BuyerId { get; set; }                            // ID del comprador
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string? BuyerPhone { get; set; }
    
    public Guid SellerId { get; set; }                           // ID del vendedor
    public string SellerName { get; set; } = string.Empty;
    public string SellerEmail { get; set; } = string.Empty;
    public string? SellerPhone { get; set; }
    
    // Objeto de la transacción
    public string? SubjectType { get; set; }                     // "Vehicle", "Service", etc.
    public Guid? SubjectId { get; set; }
    public string? SubjectDescription { get; set; }
    
    // Referencias
    public Guid? ContractId { get; set; }                        // Contrato asociado
    public Guid? TransactionId { get; set; }                     // Transacción original
    public string? ExternalReference { get; set; }               // Referencia externa
    
    // Montos
    public decimal TotalAmount { get; set; }                     // Monto total del escrow
    public decimal FundedAmount { get; set; } = 0;               // Monto depositado
    public decimal ReleasedAmount { get; set; } = 0;             // Monto liberado
    public decimal RefundedAmount { get; set; } = 0;             // Monto reembolsado
    public decimal PendingAmount { get; set; }                   // Monto pendiente
    public decimal FeeAmount { get; set; } = 0;                  // Comisión del servicio
    public decimal FeePercentage { get; set; } = 0;              // Porcentaje de comisión
    public string Currency { get; set; } = "DOP";                // Peso Dominicano
    
    // Fechas
    public DateTime CreatedAt { get; set; }
    public DateTime? FundedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }                     // Fecha de expiración
    public DateTime? ReleasedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    // Configuración
    public int ReleaseDelayDays { get; set; } = 3;               // Días de espera antes de liberar
    public bool AutoReleaseEnabled { get; set; } = false;        // Liberación automática
    public bool RequiresBothApproval { get; set; } = true;       // Requiere aprobación de ambas partes
    public bool AllowPartialRelease { get; set; } = false;       // Permite liberación parcial
    
    // Estado de aprobaciones
    public bool BuyerApproved { get; set; } = false;
    public DateTime? BuyerApprovedAt { get; set; }
    public bool SellerApproved { get; set; } = false;
    public DateTime? SellerApprovedAt { get; set; }
    
    // Términos y condiciones
    public bool TermsAccepted { get; set; } = false;
    public DateTime? TermsAcceptedAt { get; set; }
    
    // Notas y metadata
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? CustomFields { get; set; }                    // JSON
    
    // Auditoría
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navegación
    public ICollection<ReleaseCondition> Conditions { get; set; } = new List<ReleaseCondition>();
    public ICollection<FundMovement> Movements { get; set; } = new List<FundMovement>();
    public ICollection<EscrowDocument> Documents { get; set; } = new List<EscrowDocument>();
    public ICollection<EscrowDispute> Disputes { get; set; } = new List<EscrowDispute>();
    public ICollection<EscrowAuditLog> AuditLogs { get; set; } = new List<EscrowAuditLog>();
}

/// <summary>
/// Condición de liberación de fondos
/// </summary>
public class ReleaseCondition
{
    public Guid Id { get; set; }
    public Guid EscrowAccountId { get; set; }
    public ReleaseConditionType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ConditionStatus Status { get; set; } = ConditionStatus.Pending;
    
    // Configuración
    public bool IsMandatory { get; set; } = true;                // Obligatoria para liberación
    public int Order { get; set; } = 0;                          // Orden de verificación
    public bool RequiresEvidence { get; set; } = false;          // Requiere documentación
    public string? ExpectedValue { get; set; }                   // Valor esperado
    public string? ActualValue { get; set; }                     // Valor actual
    
    // Fechas
    public DateTime? DueDate { get; set; }                       // Fecha límite
    public DateTime? MetAt { get; set; }                         // Fecha en que se cumplió
    public DateTime? FailedAt { get; set; }
    
    // Verificación
    public string? VerifiedBy { get; set; }
    public string? VerificationNotes { get; set; }
    public Guid? EvidenceDocumentId { get; set; }                // Documento de evidencia
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navegación
    public EscrowAccount? EscrowAccount { get; set; }
}

/// <summary>
/// Movimiento de fondos en escrow
/// </summary>
public class FundMovement
{
    public Guid Id { get; set; }
    public Guid EscrowAccountId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty; // Número de transacción
    public FundMovementType Type { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    
    // Montos
    public decimal Amount { get; set; }
    public decimal? FeeAmount { get; set; }
    public string Currency { get; set; } = "DOP";
    
    // Origen/Destino
    public string? SourceAccount { get; set; }                   // Cuenta origen
    public string? DestinationAccount { get; set; }              // Cuenta destino
    public string? BankName { get; set; }
    public string? BankReference { get; set; }                   // Referencia bancaria
    
    // Estado
    public string Status { get; set; } = "Pending";              // Pending, Completed, Failed, Reversed
    public string? FailureReason { get; set; }
    
    // Parte involucrada
    public Guid? PartyId { get; set; }                           // Comprador o vendedor
    public string? PartyName { get; set; }
    public string? PartyType { get; set; }                       // "Buyer" o "Seller"
    
    // Fechas
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Auditoría
    public string InitiatedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public string? Notes { get; set; }
    
    // Navegación
    public EscrowAccount? EscrowAccount { get; set; }
}

/// <summary>
/// Documento adjunto al escrow
/// </summary>
public class EscrowDocument
{
    public Guid Id { get; set; }
    public Guid EscrowAccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DocumentType { get; set; } = string.Empty;     // "contract", "receipt", "evidence", etc.
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? FilePath { get; set; }                         // Ruta del archivo
    public string? FileHash { get; set; }
    
    // Eliminación lógica
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    // Verificación
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    
    // Visibilidad
    public bool VisibleToBuyer { get; set; } = true;
    public bool VisibleToSeller { get; set; } = true;
    
    // Auditoría
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    
    // Navegación
    public EscrowAccount? EscrowAccount { get; set; }
}

/// <summary>
/// Disputa del escrow
/// </summary>
public class EscrowDispute
{
    public Guid Id { get; set; }
    public Guid EscrowAccountId { get; set; }
    public string DisputeNumber { get; set; } = string.Empty;
    public EscrowDisputeStatus Status { get; set; } = EscrowDisputeStatus.Filed;
    
    // Iniciador
    public Guid FiledById { get; set; }
    public string FiledByName { get; set; } = string.Empty;
    public string FiledByType { get; set; } = string.Empty;      // "Buyer" o "Seller"
    
    // Razón
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? DisputedAmount { get; set; }
    public string? Category { get; set; }                        // Categoría de disputa
    
    // Resolución
    public string? Resolution { get; set; }
    public string? ResolutionNotes { get; set; }
    public decimal? ResolvedBuyerAmount { get; set; }            // Monto para comprador
    public decimal? ResolvedSellerAmount { get; set; }           // Monto para vendedor
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    
    // Escalación
    public string? EscalationReason { get; set; }                // Razón de escalación
    
    // Fechas
    public DateTime FiledAt { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    // Asignación
    public string? AssignedTo { get; set; }
    public DateTime? AssignedAt { get; set; }
    
    // Navegación
    public EscrowAccount? EscrowAccount { get; set; }
}

/// <summary>
/// Log de auditoría del escrow
/// </summary>
public class EscrowAuditLog
{
    public Guid Id { get; set; }
    public Guid EscrowAccountId { get; set; }
    public EscrowAuditEventType EventType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public decimal? AmountInvolved { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime PerformedAt { get; set; }
    
    // Navegación
    public EscrowAccount? EscrowAccount { get; set; }
}

/// <summary>
/// Configuración de tarifas de escrow
/// </summary>
public class EscrowFeeConfiguration
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EscrowTransactionType TransactionType { get; set; }
    public decimal MinAmount { get; set; }                       // Monto mínimo aplicable
    public decimal MaxAmount { get; set; }                       // Monto máximo aplicable
    public decimal FeePercentage { get; set; }                   // Porcentaje de comisión
    public decimal MinFee { get; set; }                          // Comisión mínima
    public decimal MaxFee { get; set; }                          // Comisión máxima
    public bool IsActive { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

#endregion
