using BankReconciliationService.Domain.Enums;

namespace BankReconciliationService.Domain.Entities;

/// <summary>
/// Estado de cuenta bancario importado desde la API del banco
/// </summary>
public class BankStatement
{
    public Guid Id { get; set; }
    public string BankCode { get; set; } = string.Empty; // BPD, BANRESERVAS, BHD
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public DateTime StatementDate { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public ReconciliationStatus Status { get; set; } = ReconciliationStatus.Pending;
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    public Guid ImportedByUserId { get; set; }
    public string? Notes { get; set; }
    
    // Relaciones
    public List<BankStatementLine> Lines { get; set; } = new();
    public List<Reconciliation> Reconciliations { get; set; } = new();
    
    // Metadata
    public string? ImportSource { get; set; } // API, CSV, MANUAL
    public string? ApiTransactionId { get; set; }
}

/// <summary>
/// Línea individual de movimiento bancario
/// </summary>
public class BankStatementLine
{
    public Guid Id { get; set; }
    public Guid BankStatementId { get; set; }
    public int LineNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ValueDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal Balance { get; set; }
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public Guid? ReconciledByUserId { get; set; }
    
    // Relaciones
    public BankStatement BankStatement { get; set; } = null!;
    public List<ReconciliationMatch> Matches { get; set; } = new();
    
    // Metadata adicional
    public string? BankCategory { get; set; }
    public string? Beneficiary { get; set; }
    public string? OriginAccount { get; set; }
}

/// <summary>
/// Transacción interna del sistema (pagos recibidos, desembolsos)
/// </summary>
public class InternalTransaction
{
    public Guid Id { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty; // PAYMENT, REFUND, TRANSFER, FEE
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "DOP";
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledAt { get; set; }
    
    // Origen de la transacción
    public string SourceService { get; set; } = string.Empty; // BillingService, PaymentService
    public Guid? SourceEntityId { get; set; } // PaymentId, RefundId, etc.
    public string? PaymentGateway { get; set; } // STRIPE, AZUL, FYGARO, PIXELPAY
    public string? GatewayTransactionId { get; set; }
    
    // Cliente/Dealer
    public Guid? CustomerId { get; set; }
    public Guid? DealerId { get; set; }
    public string? CustomerName { get; set; }
    
    // Relaciones
    public List<ReconciliationMatch> Matches { get; set; } = new();
    
    // Metadata contable
    public string? AccountCode { get; set; } // 1.1.02.01 (Banco Popular)
    public Guid? JournalEntryId { get; set; } // Link a AccountingService
}

/// <summary>
/// Conciliación completa de un período
/// </summary>
public class Reconciliation
{
    public Guid Id { get; set; }
    public Guid BankStatementId { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public ReconciliationStatus Status { get; set; } = ReconciliationStatus.InProgress;
    public Guid PerformedByUserId { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Resultados
    public int TotalBankLines { get; set; }
    public int TotalInternalTransactions { get; set; }
    public int MatchedCount { get; set; }
    public int UnmatchedBankCount { get; set; }
    public int UnmatchedInternalCount { get; set; }
    public decimal TotalDifference { get; set; }
    
    // Balances
    public decimal BankOpeningBalance { get; set; }
    public decimal BankClosingBalance { get; set; }
    public decimal SystemOpeningBalance { get; set; }
    public decimal SystemClosingBalance { get; set; }
    public decimal BalanceDifference { get; set; }
    
    // Relaciones
    public BankStatement BankStatement { get; set; } = null!;
    public List<ReconciliationMatch> Matches { get; set; } = new();
    public List<ReconciliationDiscrepancy> Discrepancies { get; set; } = new();
    
    // Notas y aprobación
    public string? Notes { get; set; }
    public bool IsApproved { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

/// <summary>
/// Match entre línea bancaria y transacción interna
/// </summary>
public class ReconciliationMatch
{
    public Guid Id { get; set; }
    public Guid ReconciliationId { get; set; }
    public Guid BankStatementLineId { get; set; }
    public Guid InternalTransactionId { get; set; }
    public Enums.MatchType MatchType { get; set; }
    public decimal MatchConfidence { get; set; } // 0.0 - 1.0 (ML confidence score)
    public DateTime MatchedAt { get; set; } = DateTime.UtcNow;
    public Guid MatchedByUserId { get; set; }
    public bool IsManual { get; set; }
    public string? MatchReason { get; set; }
    
    // Diferencias
    public decimal AmountDifference { get; set; }
    public int DaysDifference { get; set; }
    
    // Relaciones
    public Reconciliation Reconciliation { get; set; } = null!;
    public BankStatementLine BankLine { get; set; } = null!;
    public InternalTransaction InternalTransaction { get; set; } = null!;
    
    // Ajustes
    public decimal? AdjustmentAmount { get; set; }
    public string? AdjustmentReason { get; set; }
}

/// <summary>
/// Discrepancia detectada durante conciliación
/// </summary>
public class ReconciliationDiscrepancy
{
    public Guid Id { get; set; }
    public Guid ReconciliationId { get; set; }
    public DiscrepancyType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DiscrepancyStatus Status { get; set; } = DiscrepancyStatus.Pending;
    
    // Referencias
    public Guid? BankStatementLineId { get; set; }
    public Guid? InternalTransactionId { get; set; }
    
    // Resolución
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public string? ResolutionNotes { get; set; }
    public Guid? AdjustmentJournalEntryId { get; set; }
    
    // Relaciones
    public Reconciliation Reconciliation { get; set; } = null!;
}

/// <summary>
/// Configuración de cuenta bancaria
/// </summary>
public class BankAccountConfig
{
    public Guid Id { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty; // CHECKING, SAVINGS
    public string Currency { get; set; } = "DOP";
    public bool IsActive { get; set; } = true;
    
    // API Configuration
    public bool UseApiIntegration { get; set; }
    public string? ApiClientId { get; set; }
    public string? ApiClientSecretEncrypted { get; set; }
    public string? ApiBaseUrl { get; set; }
    public DateTime? LastApiSync { get; set; }
    public bool ApiSyncEnabled { get; set; }
    
    // Contabilidad
    public string ChartOfAccountsCode { get; set; } = string.Empty; // 1.1.02.01
    public Guid? DefaultJournalId { get; set; }
    
    // Conciliación automática
    public bool EnableAutoReconciliation { get; set; }
    public decimal AutoMatchThresholdAmount { get; set; } = 1.0m; // Diferencia máxima aceptable
    public int AutoMatchThresholdDays { get; set; } = 2; // Diferencia de días aceptable
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
