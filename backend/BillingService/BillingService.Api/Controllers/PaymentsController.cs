using Microsoft.AspNetCore.Mvc;
using BillingService.Application.DTOs;
using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;
using CarDealer.Shared.Idempotency.Attributes;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentRepository paymentRepository,
        ILogger<PaymentsController> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        return Ok(payments.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        return Ok(MapToDto(payment));
    }

    [HttpGet("subscription/{subscriptionId:guid}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetBySubscription(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetBySubscriptionIdAsync(subscriptionId, cancellationToken);
        return Ok(payments.Select(MapToDto));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByStatus(
        PaymentStatus status,
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetByStatusAsync(status, cancellationToken);
        return Ok(payments.Where(p => p.DealerId == dealerId).Select(MapToDto));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return Ok(payments.Where(p => p.DealerId == dealerId).Select(MapToDto));
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPending(CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetPendingPaymentsAsync(cancellationToken);
        return Ok(payments.Select(MapToDto));
    }

    [HttpGet("failed")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetFailed(CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetFailedPaymentsAsync(cancellationToken);
        return Ok(payments.Select(MapToDto));
    }

    [HttpGet("stripe/{stripePaymentIntentId}")]
    public async Task<ActionResult<PaymentDto>> GetByStripePaymentIntent(string stripePaymentIntentId, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByExternalReferenceAsync(stripePaymentIntentId, cancellationToken);
        if (payment == null)
            return NotFound();

        return Ok(MapToDto(payment));
    }

    [HttpGet("total/{dealerId:guid}")]
    public async Task<ActionResult<decimal>> GetTotalByDealer(Guid dealerId, CancellationToken cancellationToken)
    {
        var total = await _paymentRepository.GetTotalAmountByDealerAsync(dealerId, cancellationToken);
        return Ok(total);
    }

    [HttpPost]
    [Idempotent(RequireKey = true, KeyPrefix = "payment-create")]
    public async Task<ActionResult<PaymentDto>> Create(
        [FromBody] CreatePaymentRequest request,
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PaymentMethod>(request.Method, true, out var method))
            return BadRequest("Invalid payment method");

        var payment = new Payment(
            dealerId,
            request.Amount,
            method,
            request.Description,
            request.SubscriptionId,
            request.InvoiceId);

        var created = await _paymentRepository.AddAsync(payment, cancellationToken);
        _logger.LogInformation("Payment {PaymentId} created for dealer {DealerId}", created.Id, dealerId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [Idempotent(RequireKey = true, KeyPrefix = "payment-process")]
    [HttpPost("{id:guid}/process")]
    public async Task<ActionResult<PaymentDto>> Process(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        payment.MarkProcessing();
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        _logger.LogInformation("Payment {PaymentId} marked as processing", id);

        return Ok(MapToDto(payment));
    }

    [Idempotent(RequireKey = true, KeyPrefix = "payment-succeed")]
    [HttpPost("{id:guid}/succeed")]
    public async Task<ActionResult<PaymentDto>> Succeed(
        Guid id,
        [FromBody] SucceedPaymentRequest? request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        payment.MarkSucceeded(request?.ReceiptUrl);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        _logger.LogInformation("Payment {PaymentId} marked as succeeded", id);

        return Ok(MapToDto(payment));
    }

    [HttpPost("{id:guid}/fail")]
    public async Task<ActionResult<PaymentDto>> Fail(
        Guid id,
        [FromBody] FailPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        payment.MarkFailed(request.Reason);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        _logger.LogInformation("Payment {PaymentId} marked as failed: {Reason}", id, request.Reason);

        return Ok(MapToDto(payment));
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<ActionResult<PaymentDto>> Refund(
        Guid id,
        [FromBody] RefundPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        payment.Refund(request.Amount, request.Reason);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        _logger.LogInformation("Payment {PaymentId} refunded: {Amount}", id, request.Amount);

        return Ok(MapToDto(payment));
    }

    [HttpPost("{id:guid}/dispute")]
    public async Task<ActionResult<PaymentDto>> Dispute(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment == null)
            return NotFound();

        payment.MarkDisputed();
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        _logger.LogInformation("Payment {PaymentId} marked as disputed", id);

        return Ok(MapToDto(payment));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _paymentRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            return NotFound();

        await _paymentRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Payment {PaymentId} deleted", id);

        return NoContent();
    }

    private static PaymentDto MapToDto(Payment payment) => new(
        payment.Id,
        payment.SubscriptionId,
        payment.InvoiceId,
        payment.Amount,
        payment.Currency,
        payment.Status.ToString(),
        payment.Method.ToString(),
        payment.StripePaymentIntentId,
        payment.Description,
        payment.ReceiptUrl,
        payment.FailureReason,
        payment.RefundedAmount,
        payment.CreatedAt,
        payment.ProcessedAt
    );
}

public record SucceedPaymentRequest(string? ReceiptUrl = null);
public record FailPaymentRequest(string Reason);
