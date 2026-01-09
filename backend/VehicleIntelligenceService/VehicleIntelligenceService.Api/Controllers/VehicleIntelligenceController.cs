using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleIntelligenceService.Application.DTOs;
using VehicleIntelligenceService.Application.Features.Demand.Queries;
using VehicleIntelligenceService.Application.Features.Pricing.Queries;
using VehicleIntelligenceService.Application.Validators;

namespace VehicleIntelligenceService.Api.Controllers;

[ApiController]
[Route("api/vehicleintelligence")]
public class VehicleIntelligenceController(IMediator mediator, ILogger<VehicleIntelligenceController> logger) : ControllerBase
{
    /// <summary>
    /// Sugerencia de precio, comparación vs mercado, demanda y tiempo estimado de venta.
    /// Consumido por el wizard de publicación.
    /// </summary>
    [HttpPost("price-suggestion")]
    [Authorize]
    [ProducesResponseType(typeof(PriceSuggestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPriceSuggestion([FromBody] PriceSuggestionRequestDto dto, CancellationToken ct)
    {
        try
        {
            var validator = new PriceSuggestionRequestValidator();
            var validation = await validator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
            {
                return BadRequest(new { message = "Validation failed", errors = validation.Errors.Select(e => e.ErrorMessage).ToList() });
            }

            var result = await mediator.Send(new GetPriceSuggestionQuery(dto), ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating price suggestion");
            return StatusCode(500, new { message = "Error generating suggestion" });
        }
    }

    /// <summary>
    /// Demanda por categoría (para dashboard dealer).
    /// </summary>
    [HttpGet("demand/categories")]
    [Authorize]
    [ProducesResponseType(typeof(List<CategoryDemandDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDemandByCategory(CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetDemandByCategoryQuery(), ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting demand by category");
            return StatusCode(500, new { message = "Error retrieving demand" });
        }
    }
}
