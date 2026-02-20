using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Api.Controllers;

/// <summary>
/// Gestión de facturas — DGII-compliant invoicing for OKLA.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(
        IInvoiceRepository invoiceRepo,
        ILogger<InvoicesController> logger)
    {
        _invoiceRepo = invoiceRepo;
        _logger = logger;
    }

    /// <summary>
    /// Obtener una factura por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(id, ct);
        if (invoice == null)
            return NotFound(new { error = "Invoice not found" });

        return Ok(MapToDto(invoice));
    }

    /// <summary>
    /// Listar facturas del usuario autenticado.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(InvoiceListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var invoices = await _invoiceRepo.GetByUserIdAsync(userId, page, pageSize, ct);
        var total = await _invoiceRepo.GetCountByUserIdAsync(userId, ct);

        return Ok(new InvoiceListResponse
        {
            Items = invoices.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Listar facturas de un dealer (solo para el dealer owner).
    /// </summary>
    [HttpGet("dealer/{dealerId:guid}")]
    [ProducesResponseType(typeof(InvoiceListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDealerInvoices(
        Guid dealerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var invoices = await _invoiceRepo.GetByDealerIdAsync(dealerId, page, pageSize, ct);
        var total = await _invoiceRepo.GetCountByDealerIdAsync(dealerId, ct);

        return Ok(new InvoiceListResponse
        {
            Items = invoices.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Descargar PDF de factura (redirige a URL pre-firmada).
    /// </summary>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadInvoice(Guid id, CancellationToken ct)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(id, ct);
        if (invoice == null)
            return NotFound(new { error = "Invoice not found" });

        if (string.IsNullOrEmpty(invoice.PdfUrl))
            return NotFound(new { error = "Invoice PDF not yet generated" });

        return Redirect(invoice.PdfUrl);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private static InvoiceDto MapToDto(Invoice invoice) => new()
    {
        Id = invoice.Id,
        InvoiceNumber = invoice.InvoiceNumber,
        PaymentTransactionId = invoice.PaymentTransactionId,
        Ncf = invoice.Ncf,
        Subtotal = invoice.Subtotal,
        TaxRate = invoice.TaxRate,
        TaxAmount = invoice.TaxAmount,
        TotalAmount = invoice.TotalAmount,
        Currency = invoice.Currency,
        Description = invoice.Description,
        BuyerName = invoice.BuyerName,
        BuyerEmail = invoice.BuyerEmail,
        Status = invoice.Status.ToString(),
        PdfUrl = invoice.PdfUrl,
        IssuedAt = invoice.IssuedAt,
        PaidAt = invoice.PaidAt
    };
}

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid PaymentTransactionId { get; set; }
    public string? Ncf { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "DOP";
    public string Description { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PdfUrl { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class InvoiceListResponse
{
    public List<InvoiceDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
