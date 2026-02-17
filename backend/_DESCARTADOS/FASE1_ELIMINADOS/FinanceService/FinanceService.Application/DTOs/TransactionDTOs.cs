namespace FinanceService.Application.DTOs;

public record TransactionDto(
    Guid Id,
    string TransactionNumber,
    string Type,
    string Status,
    Guid AccountId,
    string? AccountName,
    Guid? TargetAccountId,
    string? TargetAccountName,
    decimal Amount,
    string Currency,
    decimal? ExchangeRate,
    string Description,
    string? Reference,
    string? Category,
    DateTime TransactionDate,
    DateTime? PostedDate,
    DateTime? ReconciledDate,
    Guid? InvoiceId,
    Guid? PaymentId,
    Guid? ExpenseId,
    DateTime CreatedAt
);

public record CreateTransactionRequest(
    string Type,
    Guid AccountId,
    Guid? TargetAccountId,
    decimal Amount,
    string Currency,
    string Description,
    string? Reference,
    string? Category,
    DateTime TransactionDate,
    decimal? ExchangeRate = null
);

public record TransferRequest(
    Guid SourceAccountId,
    Guid TargetAccountId,
    decimal Amount,
    string Currency,
    string Description,
    DateTime TransactionDate
);
