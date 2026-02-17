using ConfigurationService.Application.Commands;
using ConfigurationService.Application.Interfaces;
using ConfigurationService.Domain.Entities;
using MediatR;

namespace ConfigurationService.Application.Handlers;

public class CreateConfigurationHandler : IRequestHandler<CreateConfigurationCommand, ConfigurationItem>
{
    private readonly IConfigurationManager _manager;

    public CreateConfigurationHandler(IConfigurationManager manager)
    {
        _manager = manager;
    }

    public async Task<ConfigurationItem> Handle(CreateConfigurationCommand request, CancellationToken cancellationToken)
    {
        var item = new ConfigurationItem
        {
            Key = request.Key,
            Value = request.Value,
            Environment = request.Environment,
            Description = request.Description,
            TenantId = request.TenantId,
            CreatedBy = request.CreatedBy
        };

        return await _manager.CreateConfigurationAsync(item);
    }
}

public class UpdateConfigurationHandler : IRequestHandler<UpdateConfigurationCommand, ConfigurationItem>
{
    private readonly IConfigurationManager _manager;

    public UpdateConfigurationHandler(IConfigurationManager manager)
    {
        _manager = manager;
    }

    public async Task<ConfigurationItem> Handle(UpdateConfigurationCommand request, CancellationToken cancellationToken)
    {
        return await _manager.UpdateConfigurationAsync(request.Id, request.Value, request.UpdatedBy, request.ChangeReason);
    }
}

public class DeleteConfigurationHandler : IRequestHandler<DeleteConfigurationCommand, bool>
{
    private readonly IConfigurationManager _manager;

    public DeleteConfigurationHandler(IConfigurationManager manager)
    {
        _manager = manager;
    }

    public async Task<bool> Handle(DeleteConfigurationCommand request, CancellationToken cancellationToken)
    {
        await _manager.DeleteConfigurationAsync(request.Id);
        return true;
    }
}

public class CreateFeatureFlagHandler : IRequestHandler<CreateFeatureFlagCommand, FeatureFlag>
{
    private readonly IFeatureFlagManager _manager;

    public CreateFeatureFlagHandler(IFeatureFlagManager manager)
    {
        _manager = manager;
    }

    public async Task<FeatureFlag> Handle(CreateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var flag = new FeatureFlag
        {
            Name = request.Name,
            Key = request.Key,
            IsEnabled = request.IsEnabled,
            Description = request.Description,
            Environment = request.Environment,
            TenantId = request.TenantId,
            RolloutPercentage = request.RolloutPercentage,
            CreatedBy = request.CreatedBy
        };

        return await _manager.CreateFeatureFlagAsync(flag);
    }
}

public class CreateSecretHandler : IRequestHandler<CreateSecretCommand, EncryptedSecret>
{
    private readonly ISecretManager _manager;

    public CreateSecretHandler(ISecretManager manager)
    {
        _manager = manager;
    }

    public async Task<EncryptedSecret> Handle(CreateSecretCommand request, CancellationToken cancellationToken)
    {
        return await _manager.CreateSecretAsync(
            request.Key,
            request.PlainValue,
            request.Environment,
            request.CreatedBy,
            request.Description,
            request.TenantId,
            request.ExpiresAt
        );
    }
}
