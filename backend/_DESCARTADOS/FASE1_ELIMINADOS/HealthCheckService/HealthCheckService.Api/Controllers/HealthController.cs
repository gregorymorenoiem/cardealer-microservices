using HealthCheckService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HealthCheckService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IMediator mediator, ILogger<HealthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets the overall system health status
    /// </summary>
    [HttpGet("system")]
    [ProducesResponseType(typeof(Domain.Entities.SystemHealth), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemHealth(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting system health status");

        var query = new GetSystemHealthQuery();
        var systemHealth = await _mediator.Send(query, cancellationToken);

        // Return appropriate HTTP status based on overall health
        return systemHealth.OverallStatus switch
        {
            Domain.Enums.HealthStatus.Healthy => Ok(systemHealth),
            Domain.Enums.HealthStatus.Degraded => StatusCode(StatusCodes.Status200OK, systemHealth), // 200 but with degraded status
            Domain.Enums.HealthStatus.Unhealthy => StatusCode(StatusCodes.Status503ServiceUnavailable, systemHealth),
            _ => StatusCode(StatusCodes.Status500InternalServerError, systemHealth)
        };
    }

    /// <summary>
    /// Gets the health status of a specific service
    /// </summary>
    [HttpGet("service/{serviceName}")]
    [ProducesResponseType(typeof(Domain.Entities.ServiceHealth), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceHealth(string serviceName, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting health status for service: {ServiceName}", serviceName);

        var query = new GetServiceHealthQuery(serviceName);
        var serviceHealth = await _mediator.Send(query, cancellationToken);

        if (serviceHealth == null)
        {
            return NotFound(new { message = $"Service '{serviceName}' is not registered" });
        }

        return serviceHealth.Status switch
        {
            Domain.Enums.HealthStatus.Healthy => Ok(serviceHealth),
            Domain.Enums.HealthStatus.Degraded => Ok(serviceHealth),
            Domain.Enums.HealthStatus.Unhealthy => StatusCode(StatusCodes.Status503ServiceUnavailable, serviceHealth),
            _ => StatusCode(StatusCodes.Status500InternalServerError, serviceHealth)
        };
    }

    /// <summary>
    /// Gets all registered services
    /// </summary>
    [HttpGet("services")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegisteredServices(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting registered services");

        var query = new GetRegisteredServicesQuery();
        var services = await _mediator.Send(query, cancellationToken);

        return Ok(services);
    }

    /// <summary>
    /// Simple health check endpoint for this service itself
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "HealthCheckService",
            timestamp = DateTime.UtcNow
        });
    }
}
