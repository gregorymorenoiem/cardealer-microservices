using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleIntelligenceService.Application.DTOs;
using VehicleIntelligenceService.Application.Features.Demand.Queries;
using VehicleIntelligenceService.Application.Features.Intelligence.Queries;
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

    /// <summary>
    /// Análisis de mercado para un vehículo específico (make/model/year).
    /// Consumido por el dashboard de análisis de mercado de dealers.
    /// </summary>
    [HttpGet("market-analysis/{make}/{model}/{year}")]
    [Authorize]
    [ProducesResponseType(typeof(MarketAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMarketAnalysis(
        string make,
        string model,
        int year,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetMarketAnalysisQuery(make, model, year), ct);
            if (result == null)
                return NotFound(new { message = "Market analysis not found" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting market analysis");
            return StatusCode(500, new { message = "Error retrieving market analysis" });
        }
    }

    /// <summary>
    /// Dashboard de análisis de mercado con filtros opcionales.
    /// Consumido por el dashboard principal de análisis de dealers.
    /// </summary>
    [HttpGet("market-analysis/dashboard")]
    [Authorize]
    [ProducesResponseType(typeof(List<MarketAnalysisDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMarketAnalysisDashboard(
        [FromQuery] string? make,
        [FromQuery] string? model,
        [FromQuery] int? minYear,
        [FromQuery] int? maxYear,
        [FromQuery] string? fuelType,
        [FromQuery] string? bodyType,
        CancellationToken ct)
    {
        try
        {
            var query = new GetMarketAnalysisDashboardQuery(make, model, minYear, maxYear, fuelType, bodyType);
            var result = await mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting market analysis dashboard");
            return StatusCode(500, new { message = "Error retrieving dashboard" });
        }
    }

    /// <summary>
    /// Estadísticas del servicio ML (admin only).
    /// </summary>
    [HttpGet("ml/statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMLStatistics(CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetMLStatisticsQuery(), ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting ML statistics");
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }

    /// <summary>
    /// Performance de los modelos ML (admin only).
    /// </summary>
    [HttpGet("ml/performance")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModelPerformance(CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetModelPerformanceQuery(), ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting model performance");
            return StatusCode(500, new { message = "Error retrieving performance" });
        }
    }

    /// <summary>
    /// Métricas de inferencia del servicio ML (admin only).
    /// </summary>
    [HttpGet("ml/metrics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInferenceMetrics(CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetInferenceMetricsQuery(), ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting inference metrics");
            return StatusCode(500, new { message = "Error retrieving metrics" });
        }
    }
}
