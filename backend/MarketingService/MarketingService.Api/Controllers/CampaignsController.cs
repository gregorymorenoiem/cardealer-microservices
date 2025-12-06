using Microsoft.AspNetCore.Mvc;
using MarketingService.Application.DTOs;
using MarketingService.Domain.Entities;
using MarketingService.Domain.Interfaces;

namespace MarketingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignRepository _campaignRepository;

    public CampaignsController(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CampaignDto>>> GetAll(CancellationToken cancellationToken)
    {
        var campaigns = await _campaignRepository.GetAllAsync(cancellationToken);
        return Ok(campaigns.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CampaignDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
        if (campaign == null)
            return NotFound();
        return Ok(MapToDto(campaign));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<CampaignDto>>> GetByStatus(string status, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CampaignStatus>(status, true, out var campaignStatus))
            return BadRequest($"Invalid status: {status}");

        var campaigns = await _campaignRepository.GetByStatusAsync(campaignStatus, cancellationToken);
        return Ok(campaigns.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<CampaignDto>> Create([FromBody] CreateCampaignRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CampaignType>(request.Type, true, out var type))
            return BadRequest($"Invalid campaign type: {request.Type}");

        var dealerId = GetCurrentDealerId();
        var userId = GetCurrentUserId();

        var campaign = new Campaign(dealerId, request.Name, type, userId, request.Description);

        if (request.AudienceId.HasValue)
            campaign.SetAudience(request.AudienceId.Value);

        if (request.TemplateId.HasValue)
            campaign.SetTemplate(request.TemplateId.Value);

        if (request.Budget > 0)
            campaign.SetBudget(request.Budget);

        await _campaignRepository.AddAsync(campaign, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = campaign.Id }, MapToDto(campaign));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CampaignDto>> Update(Guid id, [FromBody] UpdateCampaignRequest request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
        if (campaign == null)
            return NotFound();

        campaign.Update(request.Name, request.Description);
        await _campaignRepository.UpdateAsync(campaign, cancellationToken);
        return Ok(MapToDto(campaign));
    }

    [HttpPost("{id:guid}/schedule")]
    public async Task<ActionResult<CampaignDto>> Schedule(Guid id, [FromBody] ScheduleCampaignRequest request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
        if (campaign == null)
            return NotFound();

        try
        {
            campaign.Schedule(request.ScheduledDate);
            await _campaignRepository.UpdateAsync(campaign, cancellationToken);
            return Ok(MapToDto(campaign));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<CampaignDto>> Start(Guid id, [FromQuery] int recipientCount, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
        if (campaign == null)
            return NotFound();

        try
        {
            campaign.Start(recipientCount);
            await _campaignRepository.UpdateAsync(campaign, cancellationToken);
            return Ok(MapToDto(campaign));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/pause")]
    public async Task<ActionResult<CampaignDto>> Pause(Guid id, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
        if (campaign == null)
            return NotFound();

        try
        {
            campaign.Pause();
            await _campaignRepository.UpdateAsync(campaign, cancellationToken);
            return Ok(MapToDto(campaign));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/resume")]
    public async Task<ActionResult<CampaignDto>> Resume(Guid id, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
        if (campaign == null)
            return NotFound();

        try
        {
            campaign.Resume();
            await _campaignRepository.UpdateAsync(campaign, cancellationToken);
            return Ok(MapToDto(campaign));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<CampaignDto>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
        if (campaign == null)
            return NotFound();

        campaign.Complete();
        await _campaignRepository.UpdateAsync(campaign, cancellationToken);
        return Ok(MapToDto(campaign));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<CampaignDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
        if (campaign == null)
            return NotFound();

        try
        {
            campaign.Cancel();
            await _campaignRepository.UpdateAsync(campaign, cancellationToken);
            return Ok(MapToDto(campaign));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!await _campaignRepository.ExistsAsync(id, cancellationToken))
            return NotFound();

        await _campaignRepository.DeleteAsync(id, cancellationToken);
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

    private static CampaignDto MapToDto(Campaign c) => new(
        c.Id,
        c.Name,
        c.Description,
        c.Type.ToString(),
        c.Status.ToString(),
        c.AudienceId,
        c.TemplateId,
        c.ScheduledDate,
        c.StartedAt,
        c.CompletedAt,
        c.TotalRecipients,
        c.SentCount,
        c.DeliveredCount,
        c.OpenedCount,
        c.ClickedCount,
        c.BouncedCount,
        c.UnsubscribedCount,
        c.Budget,
        c.SpentAmount,
        c.OpenRate,
        c.ClickRate,
        c.BounceRate,
        c.CreatedAt
    );
}
