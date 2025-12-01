using ConfigurationService.Application.Interfaces;
using ConfigurationService.Application.Queries;
using ConfigurationService.Domain.Entities;
using MediatR;

namespace ConfigurationService.Application.Handlers;

public class GetConfigurationHandler : IRequestHandler<GetConfigurationQuery, ConfigurationItem?>
{
    private readonly IConfigurationManager _manager;

    public GetConfigurationHandler(IConfigurationManager manager)
    {
        _manager = manager;
    }

    public async Task<ConfigurationItem?> Handle(GetConfigurationQuery request, CancellationToken cancellationToken)
    {
        return await _manager.GetConfigurationAsync(request.Key, request.Environment, request.TenantId);
    }
}

public class GetAllConfigurationsHandler : IRequestHandler<GetAllConfigurationsQuery, IEnumerable<ConfigurationItem>>
{
    private readonly IConfigurationManager _manager;

    public GetAllConfigurationsHandler(IConfigurationManager manager)
    {
        _manager = manager;
    }

    public async Task<IEnumerable<ConfigurationItem>> Handle(GetAllConfigurationsQuery request, CancellationToken cancellationToken)
    {
        return await _manager.GetAllConfigurationsAsync(request.Environment, request.TenantId);
    }
}

public class IsFeatureEnabledHandler : IRequestHandler<IsFeatureEnabledQuery, bool>
{
    private readonly IFeatureFlagManager _manager;

    public IsFeatureEnabledHandler(IFeatureFlagManager manager)
    {
        _manager = manager;
    }

    public async Task<bool> Handle(IsFeatureEnabledQuery request, CancellationToken cancellationToken)
    {
        return await _manager.IsFeatureEnabledAsync(request.Key, request.Environment, request.TenantId, request.UserId);
    }
}
