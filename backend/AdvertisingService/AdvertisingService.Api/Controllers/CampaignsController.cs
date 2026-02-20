using AdvertisingService.Application.DTOs;
using AdvertisingService.Application.Features.Campaigns.Commands.CancelCampaign;
using AdvertisingService.Application.Features.Campaigns.Commands.CreateCampaign;
using AdvertisingService.Application.Features.Campaigns.Commands.PauseCampaign;
using AdvertisingService.Application.Features.Campaigns.Commands.ResumeCampaign;
using AdvertisingService.Application.Features.Campaigns.Queries.GetCampaignById;
using AdvertisingService.Application.Features.Campaigns.Queries.GetCampaignsByOwner;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisingService.Api.Controllers;

[ApiController]
[Route("api/advertising/campaigns")]
[Authorize]
public class CampaignsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CampaignsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Create a new ad campaign</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CampaignDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<CampaignDto>.Ok(result));
    }

    /// <summary>Get campaign by ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CampaignDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCampaignByIdQuery(id), ct);
        if (result == null) return NotFound(ApiResponse<CampaignDto>.Fail("Campaign not found"));
        return Ok(ApiResponse<CampaignDto>.Ok(result));
    }

    /// <summary>Get campaigns by owner (paged)</summary>
    [HttpGet("owner/{ownerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<CampaignSummaryDto>>), 200)]
    public async Task<IActionResult> GetByOwner(
        Guid ownerId,
        [FromQuery] string ownerType = "Individual",
        [FromQuery] int? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var statusEnum = status.HasValue
            ? (Domain.Enums.CampaignStatus?)status.Value
            : null;

        var result = await _mediator.Send(
            new GetCampaignsByOwnerQuery(ownerId, ownerType, statusEnum, page, pageSize), ct);
        return Ok(ApiResponse<PaginatedResult<CampaignSummaryDto>>.Ok(result));
    }

    /// <summary>Pause a campaign</summary>
    [HttpPost("{id:guid}/pause")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> Pause(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        await _mediator.Send(new PauseCampaignCommand(id, userId), ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Resume a campaign</summary>
    [HttpPost("{id:guid}/resume")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        await _mediator.Send(new ResumeCampaignCommand(id, userId), ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Cancel a campaign</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        await _mediator.Send(new CancelCampaignCommand(id, userId), ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }
}
