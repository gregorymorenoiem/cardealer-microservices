using AdvertisingService.Application.DTOs;
using AdvertisingService.Application.Features.Pricing.Queries.GetPricingEstimate;
using AdvertisingService.Application.Features.Reports.Queries.GetCampaignReport;
using AdvertisingService.Application.Features.Reports.Queries.GetOwnerReport;
using AdvertisingService.Application.Features.Reports.Queries.GetPlatformReport;
using AdvertisingService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisingService.Api.Controllers;

[ApiController]
[Route("api/advertising/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get report for a specific campaign</summary>
    [HttpGet("campaign/{campaignId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CampaignReportDto>), 200)]
    public async Task<IActionResult> GetCampaignReport(Guid campaignId, [FromQuery] int daysBack = 30, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCampaignReportQuery(campaignId, daysBack), ct);
        if (result == null) return NotFound(ApiResponse<CampaignReportDto>.Fail("Campaign not found"));
        return Ok(ApiResponse<CampaignReportDto>.Ok(result));
    }

    /// <summary>Get aggregated report for an owner (Individual or Dealer)</summary>
    [HttpGet("owner/{ownerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OwnerReportDto>), 200)]
    public async Task<IActionResult> GetOwnerReport(
        Guid ownerId,
        [FromQuery] string ownerType = "Individual",
        [FromQuery] int daysBack = 30,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetOwnerReportQuery(ownerId, ownerType, daysBack), ct);
        if (result == null) return NotFound(ApiResponse<OwnerReportDto>.Fail("No campaigns found for this owner"));
        return Ok(ApiResponse<OwnerReportDto>.Ok(result));
    }

    /// <summary>Get platform-wide report (admin only)</summary>
    [HttpGet("platform")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PlatformReportDto>), 200)]
    public async Task<IActionResult> GetPlatformReport([FromQuery] int daysBack = 30, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPlatformReportQuery(daysBack), ct);
        return Ok(ApiResponse<PlatformReportDto>.Ok(result));
    }

    /// <summary>Get pricing estimates for a placement type (public)</summary>
    [HttpGet("pricing/{placementType}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PricingEstimateDto>), 200)]
    public async Task<IActionResult> GetPricing(AdPlacementType placementType, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPricingEstimateQuery(placementType), ct);
        return Ok(ApiResponse<PricingEstimateDto>.Ok(result));
    }
}
