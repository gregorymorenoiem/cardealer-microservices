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
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentRepository paymentRepository,
        IInvoiceRepository invoiceRepository,
        ILogger<PaymentsController> logger)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _logger = logger;
    }

    private Guid GetDealerId() => Guid.Parse(User.FindFirstValue("dealerId") ?? throw new UnauthorizedAccessException());
    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll(CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetAllAsync(cancellationToken);
        return Ok(payments.Select(p => PaymentDto.FromEntity(p)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        return Ok(PaymentDto.FromEntity(payment));
    }

    [HttpGet("number/{paymentNumber}")]
    public async Task<ActionResult<PaymentDto>> GetByNumber(string paymentNumber, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByPaymentNumberAsync(paymentNumber, cancellationToken);
        if (payment == null)
            return NotFound();

        return Ok(PaymentDto.FromEntity(payment));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByStatus(PaymentStatus status, CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetByStatusAsync(status, cancellationToken);
        return Ok(payments.Select(p => PaymentDto.FromEntity(p)));
    }

    [HttpGet("invoice/{invoiceId:guid}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByInvoice(Guid invoiceId, CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetByInvoiceAsync(invoiceId, cancellationToken);
        return Ok(payments.Select(p => PaymentDto.FromEntity(p)));
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetByCustomerAsync(customerId, cancellationToken);
        return Ok(payments.Select(p => PaymentDto.FromEntity(p)));
    }

    [HttpGet("method/{method}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByMethod(PaymentMethod method, CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetByMethodAsync(method, cancellationToken);
        return Ok(payments.Select(p => PaymentDto.FromEntity(p)));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByDateRange(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetByDateRangeAsync(from, to, cancellationToken);
        return Ok(payments.Select(p => PaymentDto.FromEntity(p)));
    }

    [HttpGet("total")]
    public async Task<ActionResult<decimal>> GetTotalReceived(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var total = await _paymentRepository.GetTotalReceivedAsync(from, to, cancellationToken);
        return Ok(total);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var dealerId = GetDealerId();
        var userId = GetUserId();

        // Verify invoice exists
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice == null)
            return BadRequest("Invoice not found");

        if (!Enum.TryParse<PaymentMethod>(request.Method, out var paymentMethod))
            paymentMethod = PaymentMethod.Other;

        var paymentNumber = await _paymentRepository.GeneratePaymentNumberAsync(cancellationToken);

        var payment = new Payment(
            dealerId,
            paymentNumber,
            request.InvoiceId,
            request.CustomerId,
            request.Amount,
            request.Currency,
            paymentMethod,
            request.PaymentDate,
            userId);

        if (!string.IsNullOrEmpty(request.Reference))
            payment.SetReference(request.Reference);

        if (!string.IsNullOrEmpty(request.Notes))
            payment.SetNotes(request.Notes);

        await _paymentRepository.AddAsync(payment, cancellationToken);

        _logger.LogInformation("Payment {PaymentNumber} created for invoice {InvoiceId}", paymentNumber, request.InvoiceId);

        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, PaymentDto.FromEntity(payment));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> Update(Guid id, [FromBody] UpdatePaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        if (payment.Status != PaymentStatus.Pending)
            return BadRequest("Only pending payments can be updated");

        if (!string.IsNullOrEmpty(request.Reference))
            payment.SetReference(request.Reference);

        if (request.Notes != null)
            payment.SetNotes(request.Notes);

        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        return Ok(PaymentDto.FromEntity(payment));
    }

    [HttpPost("{id:guid}/process")]
    public async Task<ActionResult<PaymentDto>> Process(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        payment.MarkAsProcessing();
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        _logger.LogInformation("Payment {PaymentNumber} processing", payment.PaymentNumber);

        return Ok(PaymentDto.FromEntity(payment));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<PaymentDto>> Complete(Guid id, [FromBody] CompletePaymentRequest? request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        payment.Complete(request?.TransactionId);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        // Update invoice paid amount
        var invoice = await _invoiceRepository.GetByIdAsync(payment.InvoiceId, cancellationToken);
        if (invoice != null)
        {
            invoice.RecordPayment(payment.Amount);
            await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        }

        _logger.LogInformation("Payment {PaymentNumber} completed", payment.PaymentNumber);

        return Ok(PaymentDto.FromEntity(payment));
    }

    [HttpPost("{id:guid}/fail")]
    public async Task<ActionResult<PaymentDto>> Fail(Guid id, [FromBody] string? reason, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        payment.Fail(reason);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        _logger.LogWarning("Payment {PaymentNumber} failed: {Reason}", payment.PaymentNumber, reason);

        return Ok(PaymentDto.FromEntity(payment));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<PaymentDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        payment.Cancel();
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        _logger.LogInformation("Payment {PaymentNumber} cancelled", payment.PaymentNumber);

        return Ok(PaymentDto.FromEntity(payment));
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<ActionResult<PaymentDto>> Refund(Guid id, [FromBody] RefundPaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        payment.Refund(request.Amount, request.Reason);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        _logger.LogInformation("Payment {PaymentNumber} refunded {Amount}: {Reason}", payment.PaymentNumber, request.Amount, request.Reason);

        return Ok(PaymentDto.FromEntity(payment));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        if (payment.Status != PaymentStatus.Pending)
            return BadRequest("Only pending payments can be deleted");

        await _paymentRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Payment {PaymentNumber} deleted", payment.PaymentNumber);

        return NoContent();
    }
}
