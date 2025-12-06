using Microsoft.AspNetCore.Mvc;
using IntegrationService.Application.DTOs;
using IntegrationService.Domain.Entities;
using IntegrationService.Domain.Interfaces;

namespace IntegrationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntegrationsController : ControllerBase
{
    private readonly IIntegrationRepository _integrationRepository;

    public IntegrationsController(IIntegrationRepository integrationRepository)
    {
        _integrationRepository = integrationRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IntegrationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var integrations = await _integrationRepository.GetAllAsync(cancellationToken);
        return Ok(integrations.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IntegrationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var integration = await _integrationRepository.GetByIdAsync(id, cancellationToken);
        if (integration == null)
            return NotFound();
        return Ok(MapToDto(integration));
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<IntegrationDto>>> GetByType(string type, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<IntegrationType>(type, true, out var integrationType))
            return BadRequest($"Invalid integration type: {type}");

        var integrations = await _integrationRepository.GetByTypeAsync(integrationType, cancellationToken);
        return Ok(integrations.Select(MapToDto));
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<IntegrationDto>>> GetActive(CancellationToken cancellationToken)
    {
        var integrations = await _integrationRepository.GetActiveAsync(cancellationToken);
        return Ok(integrations.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<IntegrationDto>> Create([FromBody] CreateIntegrationRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<IntegrationType>(request.Type, true, out var type))
            return BadRequest($"Invalid integration type: {request.Type}");

        var dealerId = GetCurrentDealerId();
        var userId = GetCurrentUserId();

        var integration = new Integration(dealerId, request.Name, type, userId, request.Description);

        if (!string.IsNullOrEmpty(request.ApiKey) || !string.IsNullOrEmpty(request.ApiSecret))
            integration.SetCredentials(request.ApiKey, request.ApiSecret);

        if (!string.IsNullOrEmpty(request.WebhookUrl))
            integration.SetWebhook(request.WebhookUrl, null);

        if (!string.IsNullOrEmpty(request.Configuration))
            integration.SetConfiguration(request.Configuration);

        await _integrationRepository.AddAsync(integration, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = integration.Id }, MapToDto(integration));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<IntegrationDto>> Update(Guid id, [FromBody] UpdateIntegrationRequest request, CancellationToken cancellationToken)
    {
        var integration = await _integrationRepository.GetByIdAsync(id, cancellationToken);
        if (integration == null)
            return NotFound();

        if (!string.IsNullOrEmpty(request.Configuration))
            integration.SetConfiguration(request.Configuration);

        await _integrationRepository.UpdateAsync(integration, cancellationToken);
        return Ok(MapToDto(integration));
    }

    [HttpPost("{id:guid}/credentials")]
    public async Task<ActionResult<IntegrationDto>> SetCredentials(Guid id, [FromBody] SetCredentialsRequest request, CancellationToken cancellationToken)
    {
        var integration = await _integrationRepository.GetByIdAsync(id, cancellationToken);
        if (integration == null)
            return NotFound();

        integration.SetCredentials(request.ApiKey, request.ApiSecret);
        await _integrationRepository.UpdateAsync(integration, cancellationToken);
        return Ok(MapToDto(integration));
    }

    [HttpPost("{id:guid}/tokens")]
    public async Task<ActionResult<IntegrationDto>> SetTokens(Guid id, [FromBody] SetTokensRequest request, CancellationToken cancellationToken)
    {
        var integration = await _integrationRepository.GetByIdAsync(id, cancellationToken);
        if (integration == null)
            return NotFound();

        integration.SetTokens(request.AccessToken, request.RefreshToken, request.ExpiresAt);
        await _integrationRepository.UpdateAsync(integration, cancellationToken);
        return Ok(MapToDto(integration));
    }

    [HttpPost("{id:guid}/webhook")]
    public async Task<ActionResult<IntegrationDto>> SetWebhook(Guid id, [FromBody] SetWebhookRequest request, CancellationToken cancellationToken)
    {
        var integration = await _integrationRepository.GetByIdAsync(id, cancellationToken);
        if (integration == null)
            return NotFound();

        integration.SetWebhook(request.WebhookUrl, request.WebhookSecret);
        await _integrationRepository.UpdateAsync(integration, cancellationToken);
        return Ok(MapToDto(integration));
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<IntegrationDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var integration = await _integrationRepository.GetByIdAsync(id, cancellationToken);
        if (integration == null)
            return NotFound();

        integration.Activate();
        await _integrationRepository.UpdateAsync(integration, cancellationToken);
        return Ok(MapToDto(integration));
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<IntegrationDto>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var integration = await _integrationRepository.GetByIdAsync(id, cancellationToken);
        if (integration == null)
            return NotFound();

        integration.Deactivate();
        await _integrationRepository.UpdateAsync(integration, cancellationToken);
        return Ok(MapToDto(integration));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!await _integrationRepository.ExistsAsync(id, cancellationToken))
            return NotFound();

        await _integrationRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentDealerId()
    {
        var dealerIdClaim = User.FindFirst("dealer_id")?.Value;
        return dealerIdClaim != null ? Guid.Parse(dealerIdClaim) : Guid.Empty;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("user_id")?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }

    private static IntegrationDto MapToDto(Integration i) => new(
        i.Id,
        i.Name,
        i.Description,
        i.Type.ToString(),
        i.Status.ToString(),
        i.WebhookUrl,
        i.Configuration,
        i.LastSyncAt,
        i.LastSyncStatus,
        i.LastError,
        i.CreatedAt
    );
}
