using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Enums;

namespace BankReconciliationService.Domain.Interfaces;

/// <summary>
/// Repository for managing bank statements
/// </summary>
public interface IBankStatementRepository
{
    Task<BankStatement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<BankStatement>> GetByBankAccountAsync(Guid bankAccountConfigId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BankStatement>> GetByDateRangeAsync(Guid bankAccountConfigId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<BankStatement?> GetByFileHashAsync(string fileHash, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid bankAccountConfigId, DateTime statementDate, CancellationToken cancellationToken = default);
    Task<BankStatement> AddAsync(BankStatement statement, CancellationToken cancellationToken = default);
    Task UpdateAsync(BankStatement statement, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetUnreconciledCountAsync(Guid bankAccountConfigId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BankStatementLine>> GetUnreconciledLinesAsync(Guid bankAccountConfigId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for managing internal transactions
/// </summary>
public interface IInternalTransactionRepository
{
    Task<InternalTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<InternalTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<InternalTransaction>> GetUnreconciledAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<InternalTransaction?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
    Task<IEnumerable<InternalTransaction>> FindPotentialMatchesAsync(decimal amount, DateTime transactionDate, int daysTolerance = 3, CancellationToken cancellationToken = default);
    Task<InternalTransaction> AddAsync(InternalTransaction transaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(InternalTransaction transaction, CancellationToken cancellationToken = default);
    Task MarkAsReconciledAsync(Guid id, Guid reconciliationMatchId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for managing reconciliations
/// </summary>
public interface IReconciliationRepository
{
    Task<Reconciliation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Reconciliation?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reconciliation>> GetByBankAccountAsync(Guid bankAccountConfigId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reconciliation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Reconciliation?> GetLatestByBankAccountAsync(Guid bankAccountConfigId, CancellationToken cancellationToken = default);
    Task<Reconciliation> AddAsync(Reconciliation reconciliation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Reconciliation reconciliation, CancellationToken cancellationToken = default);
    Task<ReconciliationMatch> AddMatchAsync(ReconciliationMatch match, CancellationToken cancellationToken = default);
    Task<ReconciliationDiscrepancy> AddDiscrepancyAsync(ReconciliationDiscrepancy discrepancy, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for managing bank account configurations
/// </summary>
public interface IBankAccountConfigRepository
{
    Task<BankAccountConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BankAccountConfig?> GetByAccountNumberAsync(string bankCode, string accountNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<BankAccountConfig>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<BankAccountConfig>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<BankAccountConfig>> GetByBankCodeAsync(string bankCode, CancellationToken cancellationToken = default);
    Task<BankAccountConfig> AddAsync(BankAccountConfig config, CancellationToken cancellationToken = default);
    Task UpdateAsync(BankAccountConfig config, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string bankCode, string accountNumber, CancellationToken cancellationToken = default);
}

/// <summary>
/// Reconciliation engine interface
/// </summary>
public interface IReconciliationEngine
{
    /// <summary>
    /// Executes automatic reconciliation using ML + rules
    /// </summary>
    Task<Reconciliation> ExecuteReconciliationAsync(
        Guid bankStatementId, 
        ReconciliationSettings settings, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds suggested matches for a bank line
    /// </summary>
    Task<List<MatchSuggestion>> SuggestMatchesAsync(
        BankStatementLine bankLine, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a manual match
    /// </summary>
    Task<ReconciliationMatch> CreateManualMatchAsync(
        Guid bankLineId, 
        Guid internalTxId, 
        Guid userId, 
        string? reason = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Undoes a match
    /// </summary>
    Task UndoMatchAsync(Guid matchId, Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Settings for reconciliation process
/// </summary>
public class ReconciliationSettings
{
    public bool UseAutomaticMatching { get; set; } = true;
    public decimal AmountTolerance { get; set; } = 1.0m;
    public int DateToleranceDays { get; set; } = 2;
    public decimal MinimumConfidenceScore { get; set; } = 0.8m;
    public bool RequireManualApproval { get; set; } = true;
    public bool CreateAdjustmentsForDifferences { get; set; } = false;
}

/// <summary>
/// Match suggestion from reconciliation engine
/// </summary>
public class MatchSuggestion
{
    public InternalTransaction InternalTransaction { get; set; } = null!;
    public decimal ConfidenceScore { get; set; }
    public Enums.MatchType MatchType { get; set; }
    public decimal AmountDifference { get; set; }
    public int DaysDifference { get; set; }
    public string MatchReason { get; set; } = string.Empty;
}
