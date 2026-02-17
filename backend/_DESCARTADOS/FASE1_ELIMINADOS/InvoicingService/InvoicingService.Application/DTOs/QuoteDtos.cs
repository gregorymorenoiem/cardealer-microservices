using InvoicingService.Domain.Entities;

namespace InvoicingService.Application.DTOs;

public record QuoteDto(
    Guid Id,
    Guid DealerId,
    string QuoteNumber,
    string Status,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string Title,
    string? Description,
    DateTime IssueDate,
    DateTime ValidUntil,
    string Currency,
    decimal Subtotal,
    decimal TaxRate,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal Total,
    Guid? DealId,
    Guid? LeadId,
    string? Terms,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ViewedAt,
    DateTime? AcceptedAt,
    DateTime? RejectedAt,
    string? RejectionReason,
    IEnumerable<QuoteItemDto> Items)
{
    public static QuoteDto FromEntity(Quote entity) => new(
        entity.Id,
        entity.DealerId,
        entity.QuoteNumber,
        entity.Status.ToString(),
        entity.CustomerId,
        entity.CustomerName,
        entity.CustomerEmail,
        entity.CustomerPhone,
        entity.Title,
        entity.Description,
        entity.IssueDate,
        entity.ValidUntil,
        entity.Currency,
        entity.Subtotal,
        entity.TaxRate,
        entity.TaxAmount,
        entity.DiscountAmount,
        entity.Total,
        entity.DealId,
        entity.LeadId,
        entity.Terms,
        entity.Notes,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.ViewedAt,
        entity.AcceptedAt,
        entity.RejectedAt,
        entity.RejectionReason,
        entity.Items.Select(QuoteItemDto.FromEntity));
}

public record QuoteItemDto(
    Guid Id,
    string Description,
    string? ProductCode,
    string? ProductName,
    Guid? ProductId,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal Total,
    int SortOrder)
{
    public static QuoteItemDto FromEntity(QuoteItem entity) => new(
        entity.Id,
        entity.Description,
        entity.ProductCode,
        entity.ProductName,
        entity.ProductId,
        entity.Quantity,
        entity.Unit,
        entity.UnitPrice,
        entity.DiscountPercent,
        entity.DiscountAmount,
        entity.Total,
        entity.SortOrder);
}

public record CreateQuoteRequest(
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string Title,
    string? Description,
    DateTime IssueDate,
    DateTime ValidUntil,
    string Currency,
    decimal TaxRate,
    string? Terms,
    string? Notes,
    Guid? DealId,
    Guid? LeadId,
    IEnumerable<CreateQuoteItemRequest>? Items);

public record UpdateQuoteRequest(
    string Title,
    string? Description,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    DateTime IssueDate,
    DateTime ValidUntil,
    string? Terms,
    string? Notes);

public record CreateQuoteItemRequest(
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountPercent,
    string? ProductCode,
    string? ProductName,
    Guid? ProductId);

public record SendQuoteRequest(
    string? EmailSubject,
    string? EmailBody);

public record RejectQuoteRequest(
    string? Reason);

public record ConvertToInvoiceRequest(
    DateTime IssueDate,
    DateTime DueDate);
