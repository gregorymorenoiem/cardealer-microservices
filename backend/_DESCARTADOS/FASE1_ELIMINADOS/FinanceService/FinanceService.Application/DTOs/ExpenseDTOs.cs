namespace FinanceService.Application.DTOs;

public record ExpenseDto(
    Guid Id,
    string ExpenseNumber,
    string Category,
    string Status,
    string Description,
    decimal Amount,
    string Currency,
    DateTime ExpenseDate,
    DateTime? DueDate,
    DateTime? PaidDate,
    string? Vendor,
    string? VendorTaxId,
    string? InvoiceNumber,
    string? ReceiptUrl,
    Guid? AccountId,
    string? Notes,
    DateTime CreatedAt,
    Guid CreatedBy,
    Guid? ApprovedBy,
    DateTime? ApprovedAt
);

public record CreateExpenseRequest(
    string Category,
    string Description,
    decimal Amount,
    string Currency,
    DateTime ExpenseDate,
    DateTime? DueDate = null,
    string? Vendor = null,
    string? VendorTaxId = null,
    string? InvoiceNumber = null,
    string? ReceiptUrl = null,
    Guid? AccountId = null,
    string? Notes = null
);

public record UpdateExpenseRequest(
    string? Description,
    decimal? Amount,
    string? Category,
    string? Vendor,
    string? VendorTaxId,
    string? InvoiceNumber,
    string? ReceiptUrl,
    string? Notes
);

public record ApproveExpenseRequest(
    bool Approve,
    string? RejectionReason = null
);
