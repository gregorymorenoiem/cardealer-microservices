using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BillingService.Application.DTOs;
using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(
        IInvoiceRepository invoiceRepository,
        ILogger<InvoicesController> logger)
    {
        _invoiceRepository = invoiceRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        return Ok(invoices.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        return Ok(MapToDto(invoice));
    }

    [HttpGet("number/{invoiceNumber}")]
    public async Task<ActionResult<InvoiceDto>> GetByNumber(string invoiceNumber, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByInvoiceNumberAsync(invoiceNumber, cancellationToken);
        if (invoice == null)
            return NotFound();

        return Ok(MapToDto(invoice));
    }

    [HttpGet("subscription/{subscriptionId:guid}")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetBySubscription(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetBySubscriptionIdAsync(subscriptionId, cancellationToken);
        return Ok(invoices.Select(MapToDto));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetByStatus(
        InvoiceStatus status,
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetByStatusAsync(status, cancellationToken);
        return Ok(invoices.Where(i => i.DealerId == dealerId).Select(MapToDto));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return Ok(invoices.Where(i => i.DealerId == dealerId).Select(MapToDto));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetOverdue(CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetOverdueInvoicesAsync(cancellationToken);
        return Ok(invoices.Select(MapToDto));
    }

    [HttpGet("unpaid")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetUnpaid(CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetUnpaidInvoicesAsync(cancellationToken);
        return Ok(invoices.Select(MapToDto));
    }

    [HttpGet("total/{dealerId:guid}")]
    public async Task<ActionResult<decimal>> GetTotalByDealer(Guid dealerId, CancellationToken cancellationToken)
    {
        var total = await _invoiceRepository.GetTotalAmountByDealerAsync(dealerId, cancellationToken);
        return Ok(total);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create(
        [FromBody] CreateInvoiceRequest request,
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken)
    {
        var invoiceNumber = await GenerateInvoiceNumber(cancellationToken);

        var invoice = new Invoice(
            dealerId,
            invoiceNumber,
            request.Subtotal,
            request.TaxAmount,
            request.DueDate,
            request.SubscriptionId);

        if (!string.IsNullOrEmpty(request.Notes))
            invoice.AddNotes(request.Notes);

        if (!string.IsNullOrEmpty(request.LineItems))
            invoice.SetLineItems(request.LineItems);

        var created = await _invoiceRepository.AddAsync(invoice, cancellationToken);
        _logger.LogInformation("Invoice {InvoiceNumber} created for dealer {DealerId}", invoiceNumber, dealerId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [HttpPost("{id:guid}/issue")]
    public async Task<ActionResult<InvoiceDto>> Issue(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        invoice.Issue();
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        _logger.LogInformation("Invoice {InvoiceNumber} issued", invoice.InvoiceNumber);

        return Ok(MapToDto(invoice));
    }

    [HttpPost("{id:guid}/send")]
    public async Task<ActionResult<InvoiceDto>> Send(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        invoice.MarkSent();
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        _logger.LogInformation("Invoice {InvoiceNumber} marked as sent", invoice.InvoiceNumber);

        return Ok(MapToDto(invoice));
    }

    [HttpPost("{id:guid}/record-payment")]
    public async Task<ActionResult<InvoiceDto>> RecordPayment(
        Guid id,
        [FromBody] RecordPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        invoice.RecordPayment(request.Amount);
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        _logger.LogInformation("Payment of {Amount} recorded for invoice {InvoiceNumber}", request.Amount, invoice.InvoiceNumber);

        return Ok(MapToDto(invoice));
    }

    [HttpPost("{id:guid}/mark-overdue")]
    public async Task<ActionResult<InvoiceDto>> MarkOverdue(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        invoice.MarkOverdue();
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        _logger.LogInformation("Invoice {InvoiceNumber} marked as overdue", invoice.InvoiceNumber);

        return Ok(MapToDto(invoice));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<InvoiceDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        invoice.Cancel();
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        _logger.LogInformation("Invoice {InvoiceNumber} cancelled", invoice.InvoiceNumber);

        return Ok(MapToDto(invoice));
    }

    [HttpPost("{id:guid}/void")]
    public async Task<ActionResult<InvoiceDto>> VoidInvoice(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        invoice.Void();
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        _logger.LogInformation("Invoice {InvoiceNumber} voided", invoice.InvoiceNumber);

        return Ok(MapToDto(invoice));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _invoiceRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            return NotFound();

        await _invoiceRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Invoice {InvoiceId} deleted", id);

        return NoContent();
    }

    private async Task<string> GenerateInvoiceNumber(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"INV-{today:yyyyMM}";

        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        var invoicesThisMonth = await _invoiceRepository.GetByDateRangeAsync(startOfMonth, endOfMonth, cancellationToken);
        var count = invoicesThisMonth.Count() + 1;

        return $"{prefix}-{count:D5}";
    }

    private static InvoiceDto MapToDto(Invoice invoice) => new(
        invoice.Id,
        invoice.InvoiceNumber,
        invoice.SubscriptionId,
        invoice.Status.ToString(),
        invoice.Subtotal,
        invoice.TaxAmount,
        invoice.TotalAmount,
        invoice.PaidAmount,
        invoice.GetBalanceDue(),
        invoice.Currency,
        invoice.IssueDate,
        invoice.DueDate,
        invoice.PaidDate,
        invoice.StripeInvoiceId,
        invoice.PdfUrl,
        invoice.Notes,
        invoice.LineItems,
        invoice.CreatedAt
    );
}
