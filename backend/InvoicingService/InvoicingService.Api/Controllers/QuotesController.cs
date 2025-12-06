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
public class QuotesController : ControllerBase
{
    private readonly IQuoteRepository _quoteRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<QuotesController> _logger;

    public QuotesController(
        IQuoteRepository quoteRepository,
        IInvoiceRepository invoiceRepository,
        ILogger<QuotesController> logger)
    {
        _quoteRepository = quoteRepository;
        _invoiceRepository = invoiceRepository;
        _logger = logger;
    }

    private Guid GetDealerId() => Guid.Parse(User.FindFirstValue("dealerId") ?? throw new UnauthorizedAccessException());
    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuoteDto>>> GetAll(CancellationToken cancellationToken)
    {
        var quotes = await _quoteRepository.GetAllAsync(cancellationToken);
        return Ok(quotes.Select(q => QuoteDto.FromEntity(q)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuoteDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var quote = await _quoteRepository.GetByIdWithItemsAsync(id, cancellationToken);
        if (quote == null)
            return NotFound();

        return Ok(QuoteDto.FromEntity(quote));
    }

    [HttpGet("number/{quoteNumber}")]
    public async Task<ActionResult<QuoteDto>> GetByNumber(string quoteNumber, CancellationToken cancellationToken)
    {
        var quote = await _quoteRepository.GetByQuoteNumberAsync(quoteNumber, cancellationToken);
        if (quote == null)
            return NotFound();

        return Ok(QuoteDto.FromEntity(quote));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<QuoteDto>>> GetByStatus(QuoteStatus status, CancellationToken cancellationToken)
    {
        var quotes = await _quoteRepository.GetByStatusAsync(status, cancellationToken);
        return Ok(quotes.Select(q => QuoteDto.FromEntity(q)));
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<QuoteDto>>> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var quotes = await _quoteRepository.GetByCustomerAsync(customerId, cancellationToken);
        return Ok(quotes.Select(q => QuoteDto.FromEntity(q)));
    }

    [HttpGet("expiring")]
    public async Task<ActionResult<IEnumerable<QuoteDto>>> GetExpiring([FromQuery] int days = 7, CancellationToken cancellationToken = default)
    {
        var quotes = await _quoteRepository.GetExpiringAsync(days, cancellationToken);
        return Ok(quotes.Select(q => QuoteDto.FromEntity(q)));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<QuoteDto>>> GetByDateRange(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        var quotes = await _quoteRepository.GetByDateRangeAsync(from, to, cancellationToken);
        return Ok(quotes.Select(q => QuoteDto.FromEntity(q)));
    }

    [HttpGet("deal/{dealId:guid}")]
    public async Task<ActionResult<IEnumerable<QuoteDto>>> GetByDeal(Guid dealId, CancellationToken cancellationToken)
    {
        var quotes = await _quoteRepository.GetByDealAsync(dealId, cancellationToken);
        return Ok(quotes.Select(q => QuoteDto.FromEntity(q)));
    }

    [HttpGet("lead/{leadId:guid}")]
    public async Task<ActionResult<IEnumerable<QuoteDto>>> GetByLead(Guid leadId, CancellationToken cancellationToken)
    {
        var quotes = await _quoteRepository.GetByLeadAsync(leadId, cancellationToken);
        return Ok(quotes.Select(q => QuoteDto.FromEntity(q)));
    }

    [HttpGet("count/{status}")]
    public async Task<ActionResult<int>> GetCountByStatus(QuoteStatus status, CancellationToken cancellationToken)
    {
        var count = await _quoteRepository.GetCountByStatusAsync(status, cancellationToken);
        return Ok(count);
    }

    [HttpPost]
    public async Task<ActionResult<QuoteDto>> Create([FromBody] CreateQuoteRequest request, CancellationToken cancellationToken)
    {
        var dealerId = GetDealerId();
        var userId = GetUserId();
        var quoteNumber = await _quoteRepository.GenerateQuoteNumberAsync(cancellationToken);

        var quote = new Quote(
            dealerId,
            quoteNumber,
            request.CustomerId,
            request.CustomerName,
            request.CustomerEmail,
            request.Title,
            request.IssueDate,
            request.ValidUntil,
            request.Currency,
            request.TaxRate,
            userId);

        if (!string.IsNullOrEmpty(request.CustomerPhone))
            quote.UpdateCustomerInfo(request.CustomerName, request.CustomerEmail, request.CustomerPhone);

        quote.UpdateDetails(request.Title, request.Description, request.Terms, request.Notes);

        if (request.DealId.HasValue)
            quote.LinkToDeal(request.DealId.Value);

        if (request.LeadId.HasValue)
            quote.LinkToLead(request.LeadId.Value);

        if (request.Items != null)
        {
            int sortOrder = 1;
            foreach (var item in request.Items)
            {
                var quoteItem = new QuoteItem(
                    quote.Id,
                    item.Description,
                    item.Quantity,
                    item.Unit,
                    item.UnitPrice,
                    sortOrder++,
                    item.ProductCode,
                    item.ProductName,
                    item.ProductId,
                    item.DiscountPercent);

                quote.AddItem(quoteItem);
            }
        }

        await _quoteRepository.AddAsync(quote, cancellationToken);

        _logger.LogInformation("Quote {QuoteNumber} created for dealer {DealerId}", quoteNumber, dealerId);

        return CreatedAtAction(nameof(GetById), new { id = quote.Id }, QuoteDto.FromEntity(quote));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<QuoteDto>> Update(Guid id, [FromBody] UpdateQuoteRequest request, CancellationToken cancellationToken)
    {
        var quote = await _quoteRepository.GetByIdAsync(id, cancellationToken);
        if (quote == null)
            return NotFound();

        quote.UpdateDetails(request.Title, request.Description, request.Terms, request.Notes);
        quote.UpdateCustomerInfo(request.CustomerName, request.CustomerEmail, request.CustomerPhone);
        quote.UpdateDates(request.IssueDate, request.ValidUntil);

        await _quoteRepository.UpdateAsync(quote, cancellationToken);

        return Ok(QuoteDto.FromEntity(quote));
    }

    [HttpPost("{id:guid}/send")]
    public async Task<ActionResult<QuoteDto>> Send(Guid id, [FromBody] SendQuoteRequest? request, CancellationToken cancellationToken)
    {
        var quote = await _quoteRepository.GetByIdAsync(id, cancellationToken);
        if (quote == null)
            return NotFound();

        quote.Send();
        await _quoteRepository.UpdateAsync(quote, cancellationToken);

        _logger.LogInformation("Quote {QuoteNumber} sent", quote.QuoteNumber);

        return Ok(QuoteDto.FromEntity(quote));
    }

    [HttpPost("{id:guid}/view")]
    [AllowAnonymous]
    public async Task<ActionResult<QuoteDto>> MarkAsViewed(Guid id, CancellationToken cancellationToken)
    {
        var quote = await _quoteRepository.GetByIdWithItemsAsync(id, cancellationToken);
        if (quote == null)
            return NotFound();

        quote.MarkAsViewed();
        await _quoteRepository.UpdateAsync(quote, cancellationToken);

        return Ok(QuoteDto.FromEntity(quote));
    }

    [HttpPost("{id:guid}/accept")]
    [AllowAnonymous]
    public async Task<ActionResult<QuoteDto>> Accept(Guid id, CancellationToken cancellationToken)
    {
        var quote = await _quoteRepository.GetByIdAsync(id, cancellationToken);
        if (quote == null)
            return NotFound();

        quote.Accept();
        await _quoteRepository.UpdateAsync(quote, cancellationToken);

        _logger.LogInformation("Quote {QuoteNumber} accepted", quote.QuoteNumber);

        return Ok(QuoteDto.FromEntity(quote));
    }

    [HttpPost("{id:guid}/reject")]
    [AllowAnonymous]
    public async Task<ActionResult<QuoteDto>> Reject(Guid id, [FromBody] RejectQuoteRequest request, CancellationToken cancellationToken)
    {
        var quote = await _quoteRepository.GetByIdAsync(id, cancellationToken);
        if (quote == null)
            return NotFound();

        quote.Reject(request.Reason);
        await _quoteRepository.UpdateAsync(quote, cancellationToken);

        _logger.LogInformation("Quote {QuoteNumber} rejected: {Reason}", quote.QuoteNumber, request.Reason);

        return Ok(QuoteDto.FromEntity(quote));
    }

    [HttpPost("{id:guid}/convert")]
    public async Task<ActionResult<InvoiceDto>> ConvertToInvoice(Guid id, [FromBody] ConvertToInvoiceRequest request, CancellationToken cancellationToken)
    {
        var quote = await _quoteRepository.GetByIdWithItemsAsync(id, cancellationToken);
        if (quote == null)
            return NotFound();

        var userId = GetUserId();
        var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(cancellationToken);

        var invoice = quote.ConvertToInvoice(invoiceNumber, request.IssueDate, request.DueDate, userId);

        // Copy items
        foreach (var quoteItem in quote.Items)
        {
            var invoiceItem = quoteItem.ToInvoiceItem(invoice.Id);
            invoice.AddItem(invoiceItem);
        }

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _quoteRepository.UpdateAsync(quote, cancellationToken);

        _logger.LogInformation("Quote {QuoteNumber} converted to invoice {InvoiceNumber}", quote.QuoteNumber, invoiceNumber);

        return CreatedAtAction("GetById", "Invoices", new { id = invoice.Id }, InvoiceDto.FromEntity(invoice));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var quote = await _quoteRepository.GetByIdAsync(id, cancellationToken);
        if (quote == null)
            return NotFound();

        if (quote.Status != QuoteStatus.Draft)
            return BadRequest("Only draft quotes can be deleted");

        await _quoteRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Quote {QuoteNumber} deleted", quote.QuoteNumber);

        return NoContent();
    }
}
