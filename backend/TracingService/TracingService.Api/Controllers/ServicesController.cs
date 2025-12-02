using MediatR;
using Microsoft.AspNetCore.Mvc;
using TracingService.Application.Queries;

namespace TracingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ServicesController> _logger;

    public ServicesController(IMediator mediator, ILogger<ServicesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get list of all services that have reported traces
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServices()
    {
        _logger.LogInformation("Getting list of services");
        
        var services = await _mediator.Send(new GetServicesQuery());
        
        return Ok(new { services, count = services.Count });
    }

    /// <summary>
    /// Get list of operations for a specific service
    /// </summary>
    [HttpGet("{serviceName}/operations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOperations(string serviceName)
    {
        _logger.LogInformation("Getting operations for service {ServiceName}", serviceName);
        
        var operations = await _mediator.Send(new GetOperationsQuery { ServiceName = serviceName });
        
        return Ok(new { serviceName, operations, count = operations.Count });
    }
}
