namespace BankReconciliationService.Application.DTOs;

public record BankStatementDto(
    Guid Id,
    string BankCode,
    string AccountNumber,
    string AccountName,
    DateTime StatementDate,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    decimal OpeningBalance,
    decimal ClosingBalance,
    decimal TotalDebits,
    decimal TotalCredits,
    string Status,
    DateTime ImportedAt,
    int TotalLines,
    int ReconciledLines,
    int UnreconciledLines
);

public record BankStatementLineDto(
    Guid Id,
    int LineNumber,
    DateTime TransactionDate,
    DateTime? ValueDate,
    string ReferenceNumber,
    string Description,
    string Type,
    decimal DebitAmount,
    decimal CreditAmount,
    decimal Balance,
    bool IsReconciled,
    DateTime? ReconciledAt,
    string? Beneficiary
);

public record InternalTransactionDto(
    Guid Id,
    DateTime TransactionDate,
    string TransactionType,
    string ReferenceNumber,
    string Description,
    decimal Amount,
    string Currency,
    bool IsReconciled,
    string? PaymentGateway,
    string? CustomerName,
    string SourceService
);

public record ReconciliationDto(
    Guid Id,
    Guid BankStatementId,
    DateTime ReconciliationDate,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    string Status,
    int TotalBankLines,
    int TotalInternalTransactions,
    int MatchedCount,
    int UnmatchedBankCount,
    int UnmatchedInternalCount,
    decimal TotalDifference,
    decimal BankClosingBalance,
    decimal SystemClosingBalance,
    decimal BalanceDifference,
    bool IsApproved,
    DateTime? CompletedAt
);

public record ReconciliationMatchDto(
    Guid Id,
    BankStatementLineDto BankLine,
    InternalTransactionDto InternalTransaction,
    string MatchType,
    decimal MatchConfidence,
    decimal AmountDifference,
    int DaysDifference,
    bool IsManual,
    string? MatchReason,
    DateTime MatchedAt
);

public record ReconciliationDiscrepancyDto(
    Guid Id,
    string Type,
    string Description,
    decimal Amount,
    string Status,
    DateTime? ResolvedAt,
    string? ResolutionNotes
);

public record BankAccountConfigDto(
    Guid Id,
    string BankCode,
    string BankName,
    string AccountNumber,
    string AccountName,
    string AccountType,
    string Currency,
    bool IsActive,
    bool UseApiIntegration,
    bool ApiSyncEnabled,
    DateTime? LastApiSync,
    bool EnableAutoReconciliation
);

// Requests
public record ImportBankStatementRequest(
    Guid BankAccountConfigId,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    string? ImportMethod = "API"
);

public record CreateManualMatchRequest(
    Guid BankStatementLineId,
    Guid InternalTransactionId,
    string? Reason = null
);

public record StartReconciliationRequest(
    Guid BankStatementId,
    bool UseAutomaticMatching = true,
    decimal AmountTolerance = 1.0m,
    int DateToleranceDays = 2,
    bool RequireManualApproval = true
);

public record ApproveReconciliationRequest(
    Guid ReconciliationId,
    string? Notes = null
);

public record ResolveDiscrepancyRequest(
    Guid DiscrepancyId,
    string ResolutionNotes,
    bool CreateAdjustmentEntry = false
);

public record BankAccountConfigRequest(
    string BankCode,
    string BankName,
    string AccountNumber,
    string AccountName,
    string AccountType,
    string Currency,
    bool UseApiIntegration,
    string? ApiClientId,
    string? ApiClientSecret,
    string? ApiBaseUrl,
    bool ApiSyncEnabled,
    string ChartOfAccountsCode,
    bool EnableAutoReconciliation,
    decimal AutoMatchThresholdAmount,
    int AutoMatchThresholdDays
);

// Dashboard DTOs
public record ReconciliationDashboardDto(
    int TotalAccountsConfigured,
    int AccountsWithPendingReconciliation,
    decimal TotalUnreconciledAmount,
    int UnreconciledTransactionsCount,
    List<AccountReconciliationStatusDto> AccountsStatus,
    List<RecentReconciliationDto> RecentReconciliations,
    List<PendingDiscrepancyDto> PendingDiscrepancies
);

public record AccountReconciliationStatusDto(
    string AccountNumber,
    string BankName,
    DateTime? LastReconciliationDate,
    int DaysSinceLastReconciliation,
    int PendingTransactions,
    decimal PendingAmount,
    string Status
);

public record RecentReconciliationDto(
    Guid Id,
    string AccountNumber,
    DateTime Date,
    string Status,
    int MatchedCount,
    int UnmatchedCount,
    decimal Difference
);

public record PendingDiscrepancyDto(
    Guid Id,
    string AccountNumber,
    string Type,
    decimal Amount,
    DateTime DetectedAt,
    int DaysPending
);

// Suggestions
public record MatchSuggestionDto(
    Guid InternalTransactionId,
    string ReferenceNumber,
    string Description,
    decimal Amount,
    DateTime Date,
    decimal ConfidenceScore,
    string MatchType,
    decimal AmountDifference,
    int DaysDifference,
    string MatchReason
);
// API Requests (Controllers)
public record CreateBankAccountRequest(
    string BankCode,
    string BankName,
    string AccountNumber,
    string AccountName,
    string AccountType = "CHECKING",
    string Currency = "DOP",
    bool UseApiIntegration = false,
    string? ApiClientId = null,
    string? ApiClientSecret = null,
    string? ApiBaseUrl = null,
    bool ApiSyncEnabled = false,
    string ChartOfAccountsCode = "",
    bool EnableAutoReconciliation = false,
    decimal AutoMatchThresholdAmount = 1.0m,
    int AutoMatchThresholdDays = 2
);

public record UpdateBankAccountRequest(
    string? BankName = null,
    string? AccountName = null,
    bool? IsActive = null,
    bool? UseApiIntegration = null,
    string? ApiClientId = null,
    string? ApiClientSecret = null,
    string? ApiBaseUrl = null,
    bool? ApiSyncEnabled = null,
    string? ChartOfAccountsCode = null,
    bool? EnableAutoReconciliation = null,
    decimal? AutoMatchThresholdAmount = null,
    int? AutoMatchThresholdDays = null
);

public record ApiConnectionTestResult(
    bool Success,
    string Message,
    DateTime TestedAt,
    int? ResponseTimeMs = null
);

public record SupportedBankDto(
    string BankCode,
    string BankName,
    bool SupportsApi,
    string ApiType,
    string[] Features
);

public record CreateBulkMatchesRequest(
    List<CreateManualMatchRequest> Matches
);

// Reports DTOs
public record ReconciliationReportDto(
    Guid ReconciliationId,
    string BankCode,
    string AccountNumber,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    DateTime GeneratedAt,
    ReconciliationSummaryDto Summary,
    List<ReconciliationMatchDto> Matches,
    List<ReconciliationDiscrepancyDto> Discrepancies
);

public record ReconciliationSummaryDto(
    int TotalBankLines,
    int TotalInternalTransactions,
    int MatchedCount,
    int UnmatchedBankCount,
    int UnmatchedInternalCount,
    decimal TotalDifference,
    decimal MatchPercentage,
    decimal BankOpeningBalance,
    decimal BankClosingBalance,
    decimal SystemOpeningBalance,
    decimal SystemClosingBalance,
    decimal BalanceDifference
);

public record MonthlyReconciliationSummaryDto(
    int Year,
    int Month,
    int TotalReconciliations,
    int CompletedReconciliations,
    int PendingReconciliations,
    decimal TotalDifferencesResolved,
    decimal TotalPendingDifferences,
    decimal AverageMatchRate
);

public record AnnualReconciliationSummaryDto(
    int Year,
    int TotalReconciliations,
    int CompletedReconciliations,
    decimal TotalDifferencesResolved,
    decimal AverageMatchRate,
    List<MonthlyReconciliationSummaryDto> MonthlyBreakdown
);

public record ReconciliationHistoryDto(
    Guid Id,
    string BankCode,
    string AccountNumber,
    DateTime ReconciliationDate,
    string Status,
    int MatchedCount,
    int TotalLines,
    decimal BalanceDifference,
    DateTime? CompletedAt,
    string PerformedByUserName
);

public record DiscrepanciesSummaryDto(
    int TotalPending,
    int TotalInvestigating,
    int TotalResolved,
    int TotalIgnored,
    decimal TotalPendingAmount,
    decimal TotalResolvedAmount
);

public record SendReportEmailRequest(
    string[] Recipients,
    string Subject = "Reconciliation Report",
    bool IncludeDetails = true
);