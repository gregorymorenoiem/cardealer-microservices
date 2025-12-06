using Microsoft.AspNetCore.Mvc;
using IntegrationService.Application.DTOs;
using IntegrationService.Domain.Entities;
using IntegrationService.Domain.Interfaces;

namespace IntegrationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly IWebhookEventRepository _webhookRepository;

    public WebhooksController(IWebhookEventRepository webhookRepository)
    {
        _webhookRepository = webhookRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WebhookEventDto>>> GetAll(CancellationToken cancellationToken)
    {
        var events = await _webhookRepository.GetAllAsync(cancellationToken);
        return Ok(events.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WebhookEventDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var webhookEvent = await _webhookRepository.GetByIdAsync(id, cancellationToken);
        if (webhookEvent == null)
            return NotFound();
        return Ok(MapToDto(webhookEvent));
    }

    [HttpGet("integration/{integrationId:guid}")]
    public async Task<ActionResult<IEnumerable<WebhookEventDto>>> GetByIntegration(Guid integrationId, CancellationToken cancellationToken)
    {
        var events = await _webhookRepository.GetByIntegrationIdAsync(integrationId, cancellationToken);
        return Ok(events.Select(MapToDto));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<WebhookEventDto>>> GetByStatus(string status, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<WebhookStatus>(status, true, out var webhookStatus))
            return BadRequest($"Invalid webhook status: {status}");

        var events = await _webhookRepository.GetByStatusAsync(webhookStatus, cancellationToken);
        return Ok(events.Select(MapToDto));
    }

    [HttpGet("pending-retry")]
    public async Task<ActionResult<IEnumerable<WebhookEventDto>>> GetPendingRetry(CancellationToken cancellationToken)
    {
        var events = await _webhookRepository.GetPendingRetryAsync(cancellationToken);
        return Ok(events.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<WebhookEventDto>> Create([FromBody] CreateWebhookEventRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<WebhookEventType>(request.EventType, true, out var eventType))
            return BadRequest($"Invalid event type: {request.EventType}");

        var dealerId = GetCurrentDealerId();

        var webhookEvent = new WebhookEvent(
            dealerId,
            request.IntegrationId,
            eventType,
            request.EventName,
            request.Payload,
            request.Headers,
            request.MaxRetries
        );

        await _webhookRepository.AddAsync(webhookEvent, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = webhookEvent.Id }, MapToDto(webhookEvent));
    }

    [HttpPost("{id:guid}/process")]
    public async Task<ActionResult<WebhookEventDto>> Process(Guid id, [FromBody] ProcessWebhookEventRequest request, CancellationToken cancellationToken)
    {
        var webhookEvent = await _webhookRepository.GetByIdAsync(id, cancellationToken);
        if (webhookEvent == null)
            return NotFound();

        webhookEvent.StartProcessing();

        if (request.Success)
            webhookEvent.Complete(request.Response);
        else
            webhookEvent.Fail(request.ErrorMessage ?? "Unknown error");

        await _webhookRepository.UpdateAsync(webhookEvent, cancellationToken);
        return Ok(MapToDto(webhookEvent));
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<ActionResult<WebhookEventDto>> Retry(Guid id, CancellationToken cancellationToken)
    {
        var webhookEvent = await _webhookRepository.GetByIdAsync(id, cancellationToken);
        if (webhookEvent == null)
            return NotFound();

        webhookEvent.Retry();
        await _webhookRepository.UpdateAsync(webhookEvent, cancellationToken);
        return Ok(MapToDto(webhookEvent));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!await _webhookRepository.ExistsAsync(id, cancellationToken))
            return NotFound();

        await _webhookRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentDealerId()
    {
        var dealerIdClaim = User.FindFirst("dealer_id")?.Value;
        return dealerIdClaim != null ? Guid.Parse(dealerIdClaim) : Guid.Empty;
    }

    private static WebhookEventDto MapToDto(WebhookEvent e) => new(
        e.Id,
        e.IntegrationId,
        e.EventType.ToString(),
        e.EventName,
        e.Status.ToString(),
        e.Payload,
        e.Response,
        e.RetryCount,
        e.MaxRetries,
        e.NextRetryAt,
        e.ReceivedAt,
        e.ProcessedAt,
        e.ErrorMessage
    );
}
