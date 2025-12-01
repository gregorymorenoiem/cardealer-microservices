using Microsoft.AspNetCore.Mvc;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Domain.Entities;

namespace ServiceDiscovery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHealthChecker _healthChecker;
    private readonly IServiceDiscovery _discovery;
    
    public HealthController(IHealthChecker healthChecker, IServiceDiscovery discovery)
    {
        _healthChecker = healthChecker;
        _discovery = discovery;
    }
    
    /// <summary>
    /// Check health of all services
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<HealthCheckResult>>> CheckAllServices()
    {
        var results = await _healthChecker.CheckAllServicesHealthAsync();
        return Ok(results);
    }
    
    /// <summary>
    /// Check health of a specific service
    /// </summary>
    [HttpGet("service/{serviceName}")]
    public async Task<ActionResult<List<HealthCheckResult>>> CheckServiceHealth(string serviceName)
    {
        var results = await _healthChecker.CheckServiceHealthAsync(serviceName);
        
        if (results.Count == 0)
        {
            return NotFound(new { error = $"No instances found for service '{serviceName}'" });
        }
        
        return Ok(results);
    }
    
    /// <summary>
    /// Check health of a specific instance
    /// </summary>
    [HttpGet("instance/{instanceId}")]
    public async Task<ActionResult<HealthCheckResult>> CheckInstanceHealth(string instanceId)
    {
        var instance = await _discovery.GetServiceInstanceByIdAsync(instanceId);
        
        if (instance == null)
        {
            return NotFound(new { error = "Service instance not found" });
        }
        
        var result = await _healthChecker.CheckHealthAsync(instance);
        return Ok(result);
    }
    
    /// <summary>
    /// Basic health check for the discovery service itself
    /// </summary>
    [HttpGet]
    public IActionResult HealthCheck()
    {
        return Ok(new 
        { 
            status = "Healthy",
            service = "ServiceDiscovery",
            timestamp = DateTime.UtcNow
        });
    }
}
