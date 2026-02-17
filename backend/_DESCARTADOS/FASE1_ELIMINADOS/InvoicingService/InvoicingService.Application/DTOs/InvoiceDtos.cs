using InvoicingService.Domain.Entities;

namespace InvoicingService.Application.DTOs;

public record InvoiceDto(
    Guid Id,
    Guid DealerId,
    string InvoiceNumber,
    string Type,
    string Status,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerTaxId,
    string? CustomerAddress,
    DateTime IssueDate,
    DateTime DueDate,
    string Currency,
    decimal Subtotal,
    decimal TaxRate,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal Total,
    decimal PaidAmount,
    decimal BalanceDue,
    string? CfdiUuid,
    DateTime? CfdiStampedAt,
    Guid? QuoteId,
    Guid? DealId,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IEnumerable<InvoiceItemDto> Items)
{
    public static InvoiceDto FromEntity(Invoice entity) => new(
        entity.Id,
        entity.DealerId,
        entity.InvoiceNumber,
        entity.Type.ToString(),
        entity.Status.ToString(),
        entity.CustomerId,
        entity.CustomerName,
        entity.CustomerEmail,
        entity.CustomerTaxId,
        entity.CustomerAddress,
        entity.IssueDate,
        entity.DueDate,
        entity.Currency,
        entity.Subtotal,
        entity.TaxRate,
        entity.TaxAmount,
        entity.DiscountAmount,
        entity.Total,
        entity.PaidAmount,
        entity.BalanceDue,
        entity.CfdiUuid,
        entity.CfdiStampedAt,
        entity.QuoteId,
        entity.DealId,
        entity.Notes,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.Items.Select(InvoiceItemDto.FromEntity));
}

public record InvoiceItemDto(
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
    public static InvoiceItemDto FromEntity(InvoiceItem entity) => new(
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

public record CreateInvoiceRequest(
    string Type,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerTaxId,
    string? CustomerAddress,
    DateTime IssueDate,
    DateTime DueDate,
    string Currency,
    decimal TaxRate,
    string? Notes,
    Guid? QuoteId,
    Guid? DealId,
    IEnumerable<CreateInvoiceItemRequest>? Items);

public record UpdateInvoiceRequest(
    string CustomerName,
    string CustomerEmail,
    string? CustomerTaxId,
    string? CustomerAddress,
    DateTime IssueDate,
    DateTime DueDate,
    string? Notes);

public record CreateInvoiceItemRequest(
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountPercent,
    string? ProductCode,
    string? ProductName,
    Guid? ProductId);

public record SendInvoiceRequest(
    string? EmailSubject,
    string? EmailBody);

public record ApplyDiscountRequest(
    decimal DiscountAmount);

public record RecordPaymentRequest(
    decimal Amount);
