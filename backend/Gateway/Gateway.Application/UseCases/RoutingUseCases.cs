using Gateway.Domain.Interfaces;

namespace Gateway.Application.UseCases;

/// <summary>
/// Use case for checking if a route exists
/// </summary>
public class CheckRouteExistsUseCase
{
    private readonly IRoutingService _routingService;

    public CheckRouteExistsUseCase(IRoutingService routingService)
    {
        _routingService = routingService;
    }

    public async Task<bool> ExecuteAsync(string upstreamPath)
    {
        return await _routingService.RouteExists(upstreamPath);
    }
}

/// <summary>
/// Use case for resolving downstream path from upstream path
/// </summary>
public class ResolveDownstreamPathUseCase
{
    private readonly IRoutingService _routingService;

    public ResolveDownstreamPathUseCase(IRoutingService routingService)
    {
        _routingService = routingService;
    }

    public async Task<string> ExecuteAsync(string upstreamPath)
    {
        return await _routingService.ResolveDownstreamPath(upstreamPath);
    }
}
