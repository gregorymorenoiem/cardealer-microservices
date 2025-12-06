namespace BillingService.Application.DTOs;

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid? SubscriptionId,
    string Status,
    decimal Subtotal,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceDue,
    string Currency,
    DateTime IssueDate,
    DateTime DueDate,
    DateTime? PaidDate,
    string? StripeInvoiceId,
    string? PdfUrl,
    string? Notes,
    string? LineItems,
    DateTime CreatedAt
);

public record CreateInvoiceRequest(
    decimal Subtotal,
    decimal TaxAmount,
    DateTime DueDate,
    Guid? SubscriptionId = null,
    string? Notes = null,
    string? LineItems = null
);

public record RecordPaymentRequest(
    decimal Amount
);
