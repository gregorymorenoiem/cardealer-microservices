using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using DealerAnalyticsService.Application.Features.Dashboard.Queries;
using DealerAnalyticsService.Application.DTOs;

namespace DealerAnalyticsService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;
    
    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    /// <summary>
    /// Obtener resumen completo del dashboard para un dealer
    /// </summary>
    /// <param name="dealerId">ID del dealer</param>
    /// <param name="fromDate">Fecha de inicio (opcional, default: últimos 30 días)</param>
    /// <param name="toDate">Fecha de fin (opcional, default: hoy)</param>
    /// <returns>Resumen completo del dashboard</returns>
    [HttpGet("{dealerId:guid}/summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(
        Guid dealerId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var endDate = toDate ?? DateTime.UtcNow;
            var startDate = fromDate ?? endDate.AddDays(-30);
            
            var query = new GetDashboardSummaryQuery(dealerId, startDate, endDate);
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary for dealer {DealerId}", dealerId);
            return StatusCode(500, new { Message = "Error retrieving dashboard data" });
        }
    }
    
    /// <summary>
    /// Obtener métricas rápidas para el header del dashboard
    /// </summary>
    /// <param name="dealerId">ID del dealer</param>
    /// <returns>Métricas principales</returns>
    [HttpGet("{dealerId:guid}/quick-stats")]
    public async Task<ActionResult<object>> GetQuickStats(Guid dealerId)
    {
        try
        {
            var today = DateTime.UtcNow;
            var thirtyDaysAgo = today.AddDays(-30);
            
            var query = new GetDashboardSummaryQuery(dealerId, thirtyDaysAgo, today);
            var result = await _mediator.Send(query);
            
            var quickStats = new
            {
                TotalViews = result.Analytics.TotalViews,
                TotalContacts = result.Analytics.TotalContacts,
                ActualSales = result.Analytics.ActualSales,
                TotalRevenue = result.Analytics.TotalRevenue,
                ConversionRate = result.Analytics.ConversionRate,
                ViewsGrowth = result.ViewsGrowth,
                ContactsGrowth = result.ContactsGrowth,
                SalesGrowth = result.SalesGrowth,
                RevenueGrowth = result.RevenueGrowth
            };
            
            return Ok(quickStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quick stats for dealer {DealerId}", dealerId);
            return StatusCode(500, new { Message = "Error retrieving quick stats" });
        }
    }
}
