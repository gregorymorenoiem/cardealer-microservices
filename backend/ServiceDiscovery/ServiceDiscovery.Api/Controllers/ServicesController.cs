using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceDiscovery.Application.Commands;
using ServiceDiscovery.Application.Queries;
using ServiceDiscovery.Domain.Entities;

namespace ServiceDiscovery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public ServicesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Register a new service instance
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ServiceInstance>> RegisterService([FromBody] RegisterServiceCommand command)
    {
        try
        {
            var instance = await _mediator.Send(command);
            return Ok(instance);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Deregister a service instance
    /// </summary>
    [HttpDelete("{instanceId}")]
    public async Task<ActionResult> DeregisterService(string instanceId)
    {
        var command = new DeregisterServiceCommand { InstanceId = instanceId };
        var success = await _mediator.Send(command);
        
        if (!success)
        {
            return NotFound(new { error = "Service instance not found" });
        }
        
        return Ok(new { message = "Service deregistered successfully" });
    }
    
    /// <summary>
    /// Get all registered service names
    /// </summary>
    [HttpGet("names")]
    public async Task<ActionResult<List<string>>> GetServiceNames()
    {
        var query = new GetServiceNamesQuery();
        var names = await _mediator.Send(query);
        return Ok(names);
    }
    
    /// <summary>
    /// Get all instances of a specific service
    /// </summary>
    [HttpGet("{serviceName}")]
    public async Task<ActionResult<List<ServiceInstance>>> GetServiceInstances(
        string serviceName, 
        [FromQuery] bool onlyHealthy = false)
    {
        var query = new GetServiceInstancesQuery 
        { 
            ServiceName = serviceName,
            OnlyHealthy = onlyHealthy
        };
        
        var instances = await _mediator.Send(query);
        
        if (instances.Count == 0)
        {
            return NotFound(new { error = $"No instances found for service '{serviceName}'" });
        }
        
        return Ok(instances);
    }
    
    /// <summary>
    /// Get a specific service instance by ID
    /// </summary>
    [HttpGet("instance/{instanceId}")]
    public async Task<ActionResult<ServiceInstance>> GetServiceInstanceById(string instanceId)
    {
        var query = new GetServiceInstanceByIdQuery { InstanceId = instanceId };
        var instance = await _mediator.Send(query);
        
        if (instance == null)
        {
            return NotFound(new { error = "Service instance not found" });
        }
        
        return Ok(instance);
    }
}
