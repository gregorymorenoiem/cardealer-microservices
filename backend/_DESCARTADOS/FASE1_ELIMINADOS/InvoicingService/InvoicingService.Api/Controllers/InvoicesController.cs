using InvoicingService.Application.DTOs;
using InvoicingService.Domain.Entities;
using InvoicingService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InvoicingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(IInvoiceRepository invoiceRepository, ILogger<InvoicesController> logger)
    {
        _invoiceRepository = invoiceRepository;
        _logger = logger;
    }

    private Guid GetDealerId() => Guid.Parse(User.FindFirstValue("dealerId") ?? throw new UnauthorizedAccessException());
    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);
        return Ok(invoices.Select(i => InvoiceDto.FromEntity(i)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdWithItemsAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        return Ok(InvoiceDto.FromEntity(invoice));
    }

    [HttpGet("number/{invoiceNumber}")]
    public async Task<ActionResult<InvoiceDto>> GetByNumber(string invoiceNumber, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByInvoiceNumberAsync(invoiceNumber, cancellationToken);
        if (invoice == null)
            return NotFound();

        return Ok(InvoiceDto.FromEntity(invoice));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetByStatus(InvoiceStatus status, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetByStatusAsync(status, cancellationToken);
        return Ok(invoices.Select(i => InvoiceDto.FromEntity(i)));
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetByCustomerAsync(customerId, cancellationToken);
        return Ok(invoices.Select(i => InvoiceDto.FromEntity(i)));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetOverdue(CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetOverdueAsync(cancellationToken);
        return Ok(invoices.Select(i => InvoiceDto.FromEntity(i)));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetByDateRange(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetByDateRangeAsync(from, to, cancellationToken);
        return Ok(invoices.Select(i => InvoiceDto.FromEntity(i)));
    }

    [HttpGet("deal/{dealId:guid}")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetByDeal(Guid dealId, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetByDealAsync(dealId, cancellationToken);
        return Ok(invoices.Select(i => InvoiceDto.FromEntity(i)));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<decimal>> GetTotalRevenue(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var revenue = await _invoiceRepository.GetTotalRevenueAsync(from, to, cancellationToken);
        return Ok(revenue);
    }

    [HttpGet("count/{status}")]
    public async Task<ActionResult<int>> GetCountByStatus(InvoiceStatus status, CancellationToken cancellationToken)
    {
        var count = await _invoiceRepository.GetCountByStatusAsync(status, cancellationToken);
        return Ok(count);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create([FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var dealerId = GetDealerId();
        var userId = GetUserId();
        var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(cancellationToken);

        if (!Enum.TryParse<InvoiceType>(request.Type, out var invoiceType))
            invoiceType = InvoiceType.Standard;

        var invoice = new Invoice(
            dealerId,
            invoiceNumber,
            invoiceType,
            request.CustomerId,
            request.CustomerName,
            request.CustomerEmail,
            request.IssueDate,
            request.DueDate,
            request.Currency,
            request.TaxRate,
            userId);

        if (!string.IsNullOrEmpty(request.CustomerTaxId) || !string.IsNullOrEmpty(request.CustomerAddress))
            invoice.UpdateCustomerInfo(request.CustomerName, request.CustomerEmail, request.CustomerTaxId, request.CustomerAddress);

        if (!string.IsNullOrEmpty(request.Notes))
            invoice.SetNotes(request.Notes);

        if (request.QuoteId.HasValue)
            invoice.LinkToQuote(request.QuoteId.Value);

        if (request.DealId.HasValue)
            invoice.LinkToDeal(request.DealId.Value);

        if (request.Items != null)
        {
            int sortOrder = 1;
            foreach (var item in request.Items)
            {
                var invoiceItem = new InvoiceItem(
                    invoice.Id,
                    item.Description,
                    item.Quantity,
                    item.Unit,
                    item.UnitPrice,
                    sortOrder++,
                    item.ProductCode,
                    item.ProductName,
                    item.ProductId,
                    item.DiscountPercent);

                invoice.AddItem(invoiceItem);
            }
        }

        await _invoiceRepository.AddAsync(invoice, cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} created for dealer {DealerId}", invoiceNumber, dealerId);

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, InvoiceDto.FromEntity(invoice));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> Update(Guid id, [FromBody] UpdateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        invoice.UpdateCustomerInfo(request.CustomerName, request.CustomerEmail, request.CustomerTaxId, request.CustomerAddress);
        invoice.UpdateDates(request.IssueDate, request.DueDate);

        if (request.Notes != null)
            invoice.SetNotes(request.Notes);

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

        return Ok(InvoiceDto.FromEntity(invoice));
    }

    [HttpPost("{id:guid}/send")]
    public async Task<ActionResult<InvoiceDto>> Send(Guid id, [FromBody] SendInvoiceRequest? request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        invoice.Send();
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} sent", invoice.InvoiceNumber);

        return Ok(InvoiceDto.FromEntity(invoice));
    }

    [HttpPost("{id:guid}/discount")]
    public async Task<ActionResult<InvoiceDto>> ApplyDiscount(Guid id, [FromBody] ApplyDiscountRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdWithItemsAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        invoice.ApplyDiscount(request.DiscountAmount);
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

        return Ok(InvoiceDto.FromEntity(invoice));
    }

    [HttpPost("{id:guid}/payment")]
    public async Task<ActionResult<InvoiceDto>> RecordPayment(Guid id, [FromBody] RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        invoice.RecordPayment(request.Amount);
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

        _logger.LogInformation("Payment of {Amount} recorded for invoice {InvoiceNumber}", request.Amount, invoice.InvoiceNumber);

        return Ok(InvoiceDto.FromEntity(invoice));
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

        return Ok(InvoiceDto.FromEntity(invoice));
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<ActionResult<InvoiceDto>> Refund(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        invoice.Refund();
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} refunded", invoice.InvoiceNumber);

        return Ok(InvoiceDto.FromEntity(invoice));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null)
            return NotFound();

        if (invoice.Status != InvoiceStatus.Draft)
            return BadRequest("Only draft invoices can be deleted");

        await _invoiceRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} deleted", invoice.InvoiceNumber);

        return NoContent();
    }
}
