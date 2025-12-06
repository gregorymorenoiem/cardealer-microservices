using InvoicingService.Domain.Entities;

namespace InvoicingService.Application.DTOs;

public record PaymentDto(
    Guid Id,
    Guid DealerId,
    string PaymentNumber,
    Guid InvoiceId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    DateTime PaymentDate,
    string? Reference,
    string? TransactionId,
    string? Notes,
    decimal RefundedAmount,
    DateTime? RefundedAt,
    string? RefundReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static PaymentDto FromEntity(Payment entity) => new(
        entity.Id,
        entity.DealerId,
        entity.PaymentNumber,
        entity.InvoiceId,
        entity.CustomerId,
        entity.Amount,
        entity.Currency,
        entity.Method.ToString(),
        entity.Status.ToString(),
        entity.PaymentDate,
        entity.Reference,
        entity.TransactionId,
        entity.Notes,
        entity.RefundedAmount,
        entity.RefundedAt,
        entity.RefundReason,
        entity.CreatedAt,
        entity.UpdatedAt);
}

public record CreatePaymentRequest(
    Guid InvoiceId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string Method,
    DateTime PaymentDate,
    string? Reference,
    string? Notes);

public record UpdatePaymentRequest(
    string? Reference,
    string? Notes);

public record CompletePaymentRequest(
    string? TransactionId);

public record RefundPaymentRequest(
    decimal Amount,
    string Reason);
