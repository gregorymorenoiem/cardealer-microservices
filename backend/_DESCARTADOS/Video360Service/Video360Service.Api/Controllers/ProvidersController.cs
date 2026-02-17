using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Video360Service.Application.DTOs;
using Video360Service.Application.Features.Queries;

namespace Video360Service.Api.Controllers;

/// <summary>
/// Controller para información de proveedores y estadísticas de uso
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProvidersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProvidersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene información de todos los proveedores de procesamiento de video
    /// </summary>
    /// <returns>Lista de proveedores con disponibilidad y costos</returns>
    /// <remarks>
    /// Proveedores disponibles:
    /// - **FfmpegApi**: $0.011/vehículo - Excelente calidad (DEFAULT)
    /// - **ApyHub**: $0.009/vehículo - Muy buena calidad (Más económico)
    /// - **Cloudinary**: $0.012/vehículo - Buena calidad
    /// - **Imgix**: $0.018/vehículo - Excelente calidad
    /// - **Shotstack**: $0.05/vehículo - Profesional (Más caro)
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProviderInfoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProviderInfoResponse>>> GetProviders()
    {
        var query = new GetProvidersInfoQuery();
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    /// <summary>
    /// Obtiene estadísticas de uso por período
    /// </summary>
    /// <param name="billingPeriod">Período de facturación (YYYY-MM)</param>
    /// <returns>Estadísticas agregadas de uso</returns>
    [HttpGet("usage")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UsageStatsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsageStatsResponse>> GetUsageStats(
        [FromQuery] string? billingPeriod = null)
    {
        var tenantId = User.FindFirst("tenant")?.Value;
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        
        var query = new GetUsageStatsQuery
        {
            UserId = userId != null ? Guid.Parse(userId) : null,
            TenantId = tenantId,
            BillingPeriod = billingPeriod ?? DateTime.UtcNow.ToString("yyyy-MM")
        };
        
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
}
