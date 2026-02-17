// =====================================================
// AntiMoneyLaunderingService - DTOs
// Ley 155-17 Prevención de Lavado de Activos (PLD)
// =====================================================

namespace AntiMoneyLaunderingService.Application.DTOs;

public record CustomerDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string IdentificationType,
    string IdentificationNumber,
    DateTime? DateOfBirth,
    string? Nationality,
    string? Occupation,
    string? SourceOfFunds,
    decimal? EstimatedMonthlyIncome,
    string RiskLevel,
    string KycStatus,
    DateTime? LastKycReviewDate,
    DateTime? NextKycReviewDate,
    bool IsPep,
    string? PepCategory,
    string? PepPosition,
    bool IsOnSanctionsList,
    DateTime CreatedAt
);

public record CreateCustomerDto(
    Guid UserId,
    string FullName,
    string IdentificationType,
    string IdentificationNumber,
    DateTime? DateOfBirth,
    string? Nationality,
    string? Occupation,
    string? SourceOfFunds,
    decimal? EstimatedMonthlyIncome
);

public record UpdateCustomerDto(
    string? FullName,
    string? Nationality,
    string? Occupation,
    string? SourceOfFunds,
    decimal? EstimatedMonthlyIncome,
    string? RiskLevel,
    bool? IsPep,
    string? PepCategory,
    string? PepPosition
);

public record TransactionDto(
    Guid Id,
    Guid CustomerId,
    string TransactionReference,
    decimal Amount,
    string Currency,
    string TransactionType,
    DateTime TransactionDate,
    string? CounterpartyName,
    string? CounterpartyCountry,
    bool IsAboveThreshold,
    bool IsSuspicious,
    bool IsReported,
    DateTime CreatedAt
);

public record CreateTransactionDto(
    Guid CustomerId,
    string TransactionReference,
    decimal Amount,
    string Currency,
    string TransactionType,
    DateTime TransactionDate,
    string? CounterpartyName,
    string? CounterpartyIdentification,
    string? CounterpartyCountry
);

public record SuspiciousActivityReportDto(
    Guid Id,
    Guid CustomerId,
    string ReportNumber,
    string ReportType,
    decimal TransactionAmount,
    string Currency,
    string SuspicionIndicators,
    string? NarrativeDescription,
    string Status,
    DateTime? DetectedAt,
    DateTime? SubmittedToUafAt,
    string? UafConfirmationNumber,
    DateTime CreatedAt
);

public record CreateSuspiciousActivityReportDto(
    Guid CustomerId,
    string ReportType,
    decimal TransactionAmount,
    string Currency,
    string SuspicionIndicators,
    string? NarrativeDescription,
    List<Guid>? RelatedTransactionIds
);

public record AmlAlertDto(
    Guid Id,
    Guid CustomerId,
    string AlertType,
    string Severity,
    string Description,
    string Status,
    DateTime CreatedAt,
    DateTime? AcknowledgedAt,
    string? AcknowledgedBy
);

public record AmlStatisticsDto(
    int TotalCustomers,
    int HighRiskCustomers,
    int PepCustomers,
    int PendingReports,
    int SubmittedReports,
    int ActiveAlerts,
    decimal TotalSuspiciousAmount,
    DateTime GeneratedAt
);
