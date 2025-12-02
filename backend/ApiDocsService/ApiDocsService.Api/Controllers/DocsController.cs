using ApiDocsService.Core.Interfaces;
using ApiDocsService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiDocsService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DocsController : ControllerBase
{
    private readonly IApiAggregatorService _aggregatorService;
    private readonly ILogger<DocsController> _logger;

    public DocsController(IApiAggregatorService aggregatorService, ILogger<DocsController> logger)
    {
        _aggregatorService = aggregatorService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard with service statistics
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiDocsDashboard), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiDocsDashboard>> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _aggregatorService.GetDashboardAsync(cancellationToken);
        return Ok(dashboard);
    }

    /// <summary>
    /// Get all registered services
    /// </summary>
    [HttpGet("services")]
    [ProducesResponseType(typeof(List<ServiceInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceInfo>>> GetAllServices(CancellationToken cancellationToken)
    {
        var services = await _aggregatorService.GetAllServicesAsync(cancellationToken);
        return Ok(services);
    }

    /// <summary>
    /// Get a specific service by name
    /// </summary>
    [HttpGet("services/{serviceName}")]
    [ProducesResponseType(typeof(ServiceInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceInfo>> GetService(string serviceName, CancellationToken cancellationToken)
    {
        var service = await _aggregatorService.GetServiceByNameAsync(serviceName, cancellationToken);

        if (service == null)
            return NotFound(new { message = $"Service '{serviceName}' not found" });

        return Ok(service);
    }

    /// <summary>
    /// Get services by category
    /// </summary>
    [HttpGet("services/category/{category}")]
    [ProducesResponseType(typeof(List<ServiceInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceInfo>>> GetServicesByCategory(string category, CancellationToken cancellationToken)
    {
        var services = await _aggregatorService.GetServicesByCategoryAsync(category, cancellationToken);
        return Ok(services);
    }

    /// <summary>
    /// Check health of all services
    /// </summary>
    [HttpGet("health/all")]
    [ProducesResponseType(typeof(List<ServiceStatus>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceStatus>>> CheckAllHealth(CancellationToken cancellationToken)
    {
        var statuses = await _aggregatorService.CheckAllServicesHealthAsync(cancellationToken);
        return Ok(statuses);
    }

    /// <summary>
    /// Check health of a specific service
    /// </summary>
    [HttpGet("health/{serviceName}")]
    [ProducesResponseType(typeof(ServiceStatus), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceStatus>> CheckServiceHealth(string serviceName, CancellationToken cancellationToken)
    {
        var status = await _aggregatorService.CheckServiceHealthAsync(serviceName, cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Get OpenAPI spec for a service
    /// </summary>
    [HttpGet("openapi/{serviceName}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetOpenApiSpec(string serviceName, CancellationToken cancellationToken)
    {
        var spec = await _aggregatorService.GetOpenApiSpecAsync(serviceName, cancellationToken);

        if (spec == null)
            return NotFound(new { message = $"OpenAPI spec for '{serviceName}' not found or unavailable" });

        return Content(spec, "application/json");
    }

    /// <summary>
    /// Get aggregated documentation for all services
    /// </summary>
    [HttpGet("aggregated")]
    [ProducesResponseType(typeof(List<AggregatedApiDoc>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AggregatedApiDoc>>> GetAllApiDocs(CancellationToken cancellationToken)
    {
        var docs = await _aggregatorService.GetAllApiDocsAsync(cancellationToken);
        return Ok(docs);
    }

    /// <summary>
    /// Search endpoints across all services
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<ApiEndpointInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ApiEndpointInfo>>> SearchEndpoints(
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Query parameter is required" });

        var endpoints = await _aggregatorService.SearchEndpointsAsync(query, cancellationToken);
        return Ok(endpoints);
    }

    /// <summary>
    /// Refresh all cached documentation
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RefreshDocs(CancellationToken cancellationToken)
    {
        await _aggregatorService.RefreshAllDocsAsync(cancellationToken);
        return Ok(new { message = "Documentation refreshed successfully", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Get list of all categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> GetCategories(CancellationToken cancellationToken)
    {
        var services = await _aggregatorService.GetAllServicesAsync(cancellationToken);
        var categories = services
            .Select(s => s.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        return Ok(categories);
    }
}
