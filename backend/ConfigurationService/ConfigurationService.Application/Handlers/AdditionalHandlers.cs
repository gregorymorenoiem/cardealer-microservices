using ConfigurationService.Application.Commands;
using ConfigurationService.Application.Interfaces;
using ConfigurationService.Application.Queries;
using ConfigurationService.Domain.Entities;
using MediatR;

namespace ConfigurationService.Application.Handlers;

public class BulkUpsertConfigurationsHandler : IRequestHandler<BulkUpsertConfigurationsCommand, IEnumerable<ConfigurationItem>>
{
    private readonly IConfigurationManager _manager;

    public BulkUpsertConfigurationsHandler(IConfigurationManager manager)
    {
        _manager = manager;
    }

    public async Task<IEnumerable<ConfigurationItem>> Handle(BulkUpsertConfigurationsCommand request, CancellationToken cancellationToken)
    {
        var results = new List<ConfigurationItem>();

        foreach (var item in request.Items)
        {
            // Try to get existing config
            var existing = await _manager.GetConfigurationAsync(item.Key, request.Environment);

            if (existing != null)
            {
                // Update existing
                var updated = await _manager.UpdateConfigurationAsync(
                    existing.Id,
                    item.Value,
                    request.UpdatedBy,
                    request.ChangeReason
                );
                results.Add(updated);
            }
            else
            {
                // Create new
                var newItem = new ConfigurationItem
                {
                    Key = item.Key,
                    Value = item.Value,
                    Environment = request.Environment,
                    Description = item.Description,
                    CreatedBy = request.UpdatedBy
                };
                var created = await _manager.CreateConfigurationAsync(newItem);
                results.Add(created);
            }
        }

        return results;
    }
}

public class UpdateFeatureFlagHandler : IRequestHandler<UpdateFeatureFlagCommand, FeatureFlag>
{
    private readonly IFeatureFlagManager _manager;

    public UpdateFeatureFlagHandler(IFeatureFlagManager manager)
    {
        _manager = manager;
    }

    public async Task<FeatureFlag> Handle(UpdateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        return await _manager.UpdateFeatureFlagAsync(request.Id, request.IsEnabled, request.RolloutPercentage);
    }
}

public class DeleteFeatureFlagHandler : IRequestHandler<DeleteFeatureFlagCommand, bool>
{
    private readonly IFeatureFlagManager _manager;

    public DeleteFeatureFlagHandler(IFeatureFlagManager manager)
    {
        _manager = manager;
    }

    public async Task<bool> Handle(DeleteFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        await _manager.DeleteFeatureFlagAsync(request.Id);
        return true;
    }
}

public class GetAllFeatureFlagsHandler : IRequestHandler<GetAllFeatureFlagsQuery, IEnumerable<FeatureFlag>>
{
    private readonly IFeatureFlagManager _manager;

    public GetAllFeatureFlagsHandler(IFeatureFlagManager manager)
    {
        _manager = manager;
    }

    public async Task<IEnumerable<FeatureFlag>> Handle(GetAllFeatureFlagsQuery request, CancellationToken cancellationToken)
    {
        return await _manager.GetAllFeatureFlagsAsync(request.Environment);
    }
}

public class GetConfigurationsByCategoryHandler : IRequestHandler<GetConfigurationsByCategoryQuery, IEnumerable<ConfigurationItem>>
{
    private readonly IConfigurationManager _manager;

    public GetConfigurationsByCategoryHandler(IConfigurationManager manager)
    {
        _manager = manager;
    }

    public async Task<IEnumerable<ConfigurationItem>> Handle(GetConfigurationsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var allConfigs = await _manager.GetAllConfigurationsAsync(request.Environment, request.TenantId);
        return allConfigs.Where(c => c.Key.StartsWith(request.Category + "."));
    }
}

public class GetConfigurationHistoryHandler : IRequestHandler<GetConfigurationHistoryQuery, IEnumerable<ConfigurationHistory>>
{
    private readonly IConfigurationManager _manager;

    public GetConfigurationHistoryHandler(IConfigurationManager manager)
    {
        _manager = manager;
    }

    public async Task<IEnumerable<ConfigurationHistory>> Handle(GetConfigurationHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _manager.GetConfigurationHistoryAsync(request.ConfigurationItemId);
    }
}
