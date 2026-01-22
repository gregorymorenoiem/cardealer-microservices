// =====================================================
// AntiMoneyLaunderingService - Entities
// Ley 155-17 Prevención de Lavado de Activos (PLD)
// =====================================================

using AntiMoneyLaunderingService.Domain.Enums;

namespace AntiMoneyLaunderingService.Domain.Entities;

/// <summary>
/// Cliente con información KYC según Ley 155-17
/// </summary>
public class Customer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public IdentificationType IdentificationType { get; set; }
    public string IdentificationNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? Occupation { get; set; }
    public string? SourceOfFunds { get; set; }
    public decimal? EstimatedMonthlyIncome { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public KycStatus KycStatus { get; set; }
    public DateTime? LastKycReviewDate { get; set; }
    public DateTime? NextKycReviewDate { get; set; }
    public bool IsPep { get; set; }
    public PepCategory? PepCategory { get; set; }
    public string? PepPosition { get; set; }
    public DateTime? PepExpirationDate { get; set; }
    public bool IsOnSanctionsList { get; set; }
    public string? SanctionsListSource { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<AmlAlert> Alerts { get; set; } = new List<AmlAlert>();
    public ICollection<KycDocument> KycDocuments { get; set; } = new List<KycDocument>();
}

/// <summary>
/// Transacción monitoreada por AML
/// </summary>
public class Transaction
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "DOP";
    public string TransactionType { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? CounterpartyName { get; set; }
    public string? CounterpartyIdentification { get; set; }
    public string? CounterpartyCountry { get; set; }
    public bool IsAboveThreshold { get; set; }
    public bool IsSuspicious { get; set; }
    public bool IsReported { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Customer? Customer { get; set; }
}

/// <summary>
/// Reporte de Operación Sospechosa (ROS)
/// </summary>
public class SuspiciousActivityReport
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public RosReportType ReportType { get; set; }
    public decimal TransactionAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string SuspicionIndicators { get; set; } = string.Empty;
    public string? NarrativeDescription { get; set; }
    public RosStatus Status { get; set; }
    public DateTime? DetectedAt { get; set; }
    public DateTime? SubmittedToUafAt { get; set; }
    public string? UafConfirmationNumber { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public Customer? Customer { get; set; }
    public ICollection<RosTransaction> RelatedTransactions { get; set; } = new List<RosTransaction>();
}

/// <summary>
/// Transacción relacionada con un ROS
/// </summary>
public class RosTransaction
{
    public Guid Id { get; set; }
    public Guid SuspiciousActivityReportId { get; set; }
    public Guid TransactionId { get; set; }
    public string? Justification { get; set; }
    public DateTime AddedAt { get; set; }
    
    // Navigation
    public SuspiciousActivityReport? SuspiciousActivityReport { get; set; }
    public Transaction? Transaction { get; set; }
}

/// <summary>
/// Alerta de monitoreo AML
/// </summary>
public class AmlAlert
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? TransactionId { get; set; }
    public string AlertNumber { get; set; } = string.Empty;
    public AlertType AlertType { get; set; }
    public AlertStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal? RelatedAmount { get; set; }
    public int RiskScore { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? InvestigationNotes { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public Customer? Customer { get; set; }
    public Transaction? Transaction { get; set; }
}

/// <summary>
/// Documento KYC del cliente
/// </summary>
public class KycDocument
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime UploadedAt { get; set; }
    
    // Navigation
    public Customer? Customer { get; set; }
}

/// <summary>
/// Lista de sanciones para screening
/// </summary>
public class SanctionsList
{
    public Guid Id { get; set; }
    public string ListName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; // OFAC, UN, EU, etc.
    public DateTime LastUpdated { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public ICollection<SanctionedEntity> Entities { get; set; } = new List<SanctionedEntity>();
}

/// <summary>
/// Entidad sancionada
/// </summary>
public class SanctionedEntity
{
    public Guid Id { get; set; }
    public Guid SanctionsListId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AlternateNames { get; set; }
    public string? EntityType { get; set; } // Individual, Company, Vessel
    public string? Nationality { get; set; }
    public string? DateOfBirth { get; set; }
    public string? IdentificationNumber { get; set; }
    public string? SanctionReason { get; set; }
    public DateTime SanctionedSince { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public SanctionsList? SanctionsList { get; set; }
}
