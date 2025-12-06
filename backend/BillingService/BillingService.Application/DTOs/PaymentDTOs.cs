namespace BillingService.Application.DTOs;

public record PaymentDto(
    Guid Id,
    Guid? SubscriptionId,
    Guid? InvoiceId,
    decimal Amount,
    string Currency,
    string Status,
    string Method,
    string? StripePaymentIntentId,
    string? Description,
    string? ReceiptUrl,
    string? FailureReason,
    decimal RefundedAmount,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);

public record CreatePaymentRequest(
    decimal Amount,
    string Method,
    string? Description = null,
    Guid? SubscriptionId = null,
    Guid? InvoiceId = null
);

public record RefundPaymentRequest(
    decimal Amount,
    string Reason
);
