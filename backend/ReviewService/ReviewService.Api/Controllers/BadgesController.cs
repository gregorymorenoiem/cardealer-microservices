using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewService.Application.DTOs;
using ReviewService.Application.Features.Badges.Commands;
using ReviewService.Application.Features.Badges.Queries;

namespace ReviewService.Api.Controllers;

/// <summary>
/// Controller para badges de vendedores (Sprint 15)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BadgesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BadgesController> _logger;

    public BadgesController(IMediator mediator, ILogger<BadgesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener badges activos de un vendedor
    /// </summary>
    /// <param name="sellerId">ID del vendedor</param>
    /// <returns>Lista de badges del vendedor</returns>
    [HttpGet("sellers/{sellerId}")]
    public async Task<ActionResult<List<SellerBadgeDto>>> GetSellerBadges(Guid sellerId)
    {
        try
        {
            var query = new GetSellerBadgesQuery(sellerId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting badges for seller {SellerId}", sellerId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Calcular y otorgar badges para un vendedor específico (ADMIN only)
    /// </summary>
    /// <param name="sellerId">ID del vendedor</param>
    /// <returns>Resultado del cálculo de badges</returns>
    [HttpPost("calculate/{sellerId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BadgeCalculationResultDto>> CalculateBadgesForSeller(Guid sellerId)
    {
        try
        {
            var command = new CalculateBadgesCommand(sellerId);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Calculated badges for seller {SellerId}: {BadgeCount} badges granted", 
                sellerId, result.TotalBadgesGranted);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating badges for seller {SellerId}", sellerId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Calcular y otorgar badges para todos los vendedores (ADMIN only)
    /// </summary>
    /// <returns>Resultado del cálculo masivo de badges</returns>
    [HttpPost("calculate-all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BadgeCalculationResultDto>> CalculateAllBadges()
    {
        try
        {
            var command = new CalculateBadgesCommand();
            var result = await _mediator.Send(command);

            _logger.LogInformation("Calculated badges for all sellers: {SellerCount} sellers processed, {BadgeCount} badges granted", 
                result.ProcessedSellers, result.TotalBadgesGranted);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating badges for all sellers");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}