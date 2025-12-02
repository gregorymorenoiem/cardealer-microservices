using HealthCheckService.Application.Queries;
using HealthCheckService.Domain.Entities;
using HealthCheckService.Domain.Interfaces;
using MediatR;

namespace HealthCheckService.Application.Handlers;

public class GetSystemHealthQueryHandler : IRequestHandler<GetSystemHealthQuery, SystemHealth>
{
    private readonly IHealthAggregator _healthAggregator;

    public GetSystemHealthQueryHandler(IHealthAggregator healthAggregator)
    {
        _healthAggregator = healthAggregator;
    }

    public async Task<SystemHealth> Handle(GetSystemHealthQuery request, CancellationToken cancellationToken)
    {
        return await _healthAggregator.GetSystemHealthAsync(cancellationToken);
    }
}

public class GetServiceHealthQueryHandler : IRequestHandler<GetServiceHealthQuery, ServiceHealth?>
{
    private readonly IHealthAggregator _healthAggregator;

    public GetServiceHealthQueryHandler(IHealthAggregator healthAggregator)
    {
        _healthAggregator = healthAggregator;
    }

    public async Task<ServiceHealth?> Handle(GetServiceHealthQuery request, CancellationToken cancellationToken)
    {
        return await _healthAggregator.GetServiceHealthAsync(request.ServiceName, cancellationToken);
    }
}

public class GetRegisteredServicesQueryHandler : IRequestHandler<GetRegisteredServicesQuery, IEnumerable<string>>
{
    private readonly IHealthAggregator _healthAggregator;

    public GetRegisteredServicesQueryHandler(IHealthAggregator healthAggregator)
    {
        _healthAggregator = healthAggregator;
    }

    public Task<IEnumerable<string>> Handle(GetRegisteredServicesQuery request, CancellationToken cancellationToken)
    {
        var services = _healthAggregator.GetRegisteredServices();
        return Task.FromResult(services);
    }
}
