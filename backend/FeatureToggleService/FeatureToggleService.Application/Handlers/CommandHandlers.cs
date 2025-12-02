using FeatureToggleService.Application.Commands;
using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using FeatureToggleService.Domain.Interfaces;
using MediatR;

namespace FeatureToggleService.Application.Handlers;

public class CreateFeatureFlagHandler : IRequestHandler<CreateFeatureFlagCommand, FeatureFlag>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagHistoryRepository _historyRepository;

    public CreateFeatureFlagHandler(
        IFeatureFlagRepository repository,
        IFeatureFlagHistoryRepository historyRepository)
    {
        _repository = repository;
        _historyRepository = historyRepository;
    }

    public async Task<FeatureFlag> Handle(CreateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.ExistsAsync(request.Key, cancellationToken))
        {
            throw new InvalidOperationException($"Feature flag with key '{request.Key}' already exists");
        }

        var flag = new FeatureFlag
        {
            Key = request.Key,
            Name = request.Name,
            Description = request.Description,
            Environment = request.Environment,
            Tags = request.Tags,
            Status = FlagStatus.Draft,
            CreatedBy = request.CreatedBy
        };

        await _repository.AddAsync(flag, cancellationToken);

        var history = new FeatureFlagHistory
        {
            FeatureFlagId = flag.Id,
            ChangeType = ChangeType.Created,
            NewValue = "Created",
            ChangedBy = request.CreatedBy,
            ChangedAt = DateTime.UtcNow
        };
        await _historyRepository.AddAsync(history, cancellationToken);

        return flag;
    }
}

public class UpdateFeatureFlagHandler : IRequestHandler<UpdateFeatureFlagCommand, FeatureFlag?>
{
    private readonly IFeatureFlagRepository _repository;

    public UpdateFeatureFlagHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    public async Task<FeatureFlag?> Handle(UpdateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (flag == null) return null;

        if (request.Name != null) flag.Name = request.Name;
        if (request.Description != null) flag.Description = request.Description;
        if (request.Tags != null) flag.Tags = request.Tags;
        if (request.Environment.HasValue) flag.Environment = request.Environment.Value;
        if (request.ExpiresAt.HasValue) flag.ExpiresAt = request.ExpiresAt;

        flag.UpdatedAt = DateTime.UtcNow;
        flag.ModifiedBy = request.ModifiedBy;

        return await _repository.UpdateAsync(flag, cancellationToken);
    }
}

public class DeleteFeatureFlagHandler : IRequestHandler<DeleteFeatureFlagCommand, bool>
{
    private readonly IFeatureFlagRepository _repository;

    public DeleteFeatureFlagHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeleteAsync(request.Id, cancellationToken);
    }
}

public class EnableFeatureFlagHandler : IRequestHandler<EnableFeatureFlagCommand, FeatureFlag?>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagHistoryRepository _historyRepository;

    public EnableFeatureFlagHandler(
        IFeatureFlagRepository repository,
        IFeatureFlagHistoryRepository historyRepository)
    {
        _repository = repository;
        _historyRepository = historyRepository;
    }

    public async Task<FeatureFlag?> Handle(EnableFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (flag == null) return null;

        var previousStatus = flag.Status;
        flag.Enable(request.ModifiedBy);

        await _repository.UpdateAsync(flag, cancellationToken);

        var history = new FeatureFlagHistory
        {
            FeatureFlagId = flag.Id,
            ChangeType = ChangeType.Enabled,
            PreviousValue = previousStatus.ToString(),
            NewValue = flag.Status.ToString(),
            ChangedBy = request.ModifiedBy,
            ChangedAt = DateTime.UtcNow
        };
        await _historyRepository.AddAsync(history, cancellationToken);

        return flag;
    }
}

public class DisableFeatureFlagHandler : IRequestHandler<DisableFeatureFlagCommand, FeatureFlag?>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagHistoryRepository _historyRepository;

    public DisableFeatureFlagHandler(
        IFeatureFlagRepository repository,
        IFeatureFlagHistoryRepository historyRepository)
    {
        _repository = repository;
        _historyRepository = historyRepository;
    }

    public async Task<FeatureFlag?> Handle(DisableFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (flag == null) return null;

        var previousStatus = flag.Status;
        flag.Disable(request.ModifiedBy);

        await _repository.UpdateAsync(flag, cancellationToken);

        var history = new FeatureFlagHistory
        {
            FeatureFlagId = flag.Id,
            ChangeType = ChangeType.Disabled,
            PreviousValue = previousStatus.ToString(),
            NewValue = flag.Status.ToString(),
            ChangedBy = request.ModifiedBy,
            ChangedAt = DateTime.UtcNow
        };
        await _historyRepository.AddAsync(history, cancellationToken);

        return flag;
    }
}

public class ArchiveFeatureFlagHandler : IRequestHandler<ArchiveFeatureFlagCommand, FeatureFlag?>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagHistoryRepository _historyRepository;

    public ArchiveFeatureFlagHandler(
        IFeatureFlagRepository repository,
        IFeatureFlagHistoryRepository historyRepository)
    {
        _repository = repository;
        _historyRepository = historyRepository;
    }

    public async Task<FeatureFlag?> Handle(ArchiveFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (flag == null) return null;

        var previousStatus = flag.Status;
        flag.Archive(request.ModifiedBy);

        await _repository.UpdateAsync(flag, cancellationToken);

        var history = new FeatureFlagHistory
        {
            FeatureFlagId = flag.Id,
            ChangeType = ChangeType.Archived,
            PreviousValue = previousStatus.ToString(),
            NewValue = flag.Status.ToString(),
            ChangedBy = request.ModifiedBy,
            ChangedAt = DateTime.UtcNow
        };
        await _historyRepository.AddAsync(history, cancellationToken);

        return flag;
    }
}

public class RestoreFeatureFlagHandler : IRequestHandler<RestoreFeatureFlagCommand, FeatureFlag?>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagHistoryRepository _historyRepository;

    public RestoreFeatureFlagHandler(
        IFeatureFlagRepository repository,
        IFeatureFlagHistoryRepository historyRepository)
    {
        _repository = repository;
        _historyRepository = historyRepository;
    }

    public async Task<FeatureFlag?> Handle(RestoreFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (flag == null) return null;

        var previousStatus = flag.Status;
        flag.Restore(request.ModifiedBy);

        await _repository.UpdateAsync(flag, cancellationToken);

        var history = new FeatureFlagHistory
        {
            FeatureFlagId = flag.Id,
            ChangeType = ChangeType.Restored,
            PreviousValue = previousStatus.ToString(),
            NewValue = flag.Status.ToString(),
            ChangedBy = request.ModifiedBy,
            ChangedAt = DateTime.UtcNow
        };
        await _historyRepository.AddAsync(history, cancellationToken);

        return flag;
    }
}

public class TriggerKillSwitchHandler : IRequestHandler<TriggerKillSwitchCommand, FeatureFlag?>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagHistoryRepository _historyRepository;

    public TriggerKillSwitchHandler(
        IFeatureFlagRepository repository,
        IFeatureFlagHistoryRepository historyRepository)
    {
        _repository = repository;
        _historyRepository = historyRepository;
    }

    public async Task<FeatureFlag?> Handle(TriggerKillSwitchCommand request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (flag == null) return null;

        flag.TriggerKillSwitch(request.TriggeredBy, request.Reason);

        await _repository.UpdateAsync(flag, cancellationToken);

        var history = new FeatureFlagHistory
        {
            FeatureFlagId = flag.Id,
            ChangeType = ChangeType.KillSwitch,
            PreviousValue = "Active",
            NewValue = "Kill Switch Triggered",
            ChangedBy = request.TriggeredBy,
            Reason = request.Reason,
            ChangedAt = DateTime.UtcNow
        };
        await _historyRepository.AddAsync(history, cancellationToken);

        return flag;
    }
}

public class SetRolloutPercentageHandler : IRequestHandler<SetRolloutPercentageCommand, FeatureFlag?>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagHistoryRepository _historyRepository;

    public SetRolloutPercentageHandler(
        IFeatureFlagRepository repository,
        IFeatureFlagHistoryRepository historyRepository)
    {
        _repository = repository;
        _historyRepository = historyRepository;
    }

    public async Task<FeatureFlag?> Handle(SetRolloutPercentageCommand request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (flag == null) return null;

        var previousPercentage = flag.RolloutPercentage;
        flag.SetRolloutPercentage(request.Percentage, request.ModifiedBy);

        await _repository.UpdateAsync(flag, cancellationToken);

        var history = new FeatureFlagHistory
        {
            FeatureFlagId = flag.Id,
            ChangeType = ChangeType.RolloutChange,
            PreviousValue = $"{previousPercentage}%",
            NewValue = $"{request.Percentage}%",
            ChangedBy = request.ModifiedBy,
            ChangedAt = DateTime.UtcNow
        };
        await _historyRepository.AddAsync(history, cancellationToken);

        return flag;
    }
}

public class AddTargetUsersHandler : IRequestHandler<AddTargetUsersCommand, FeatureFlag?>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagHistoryRepository _historyRepository;

    public AddTargetUsersHandler(
        IFeatureFlagRepository repository,
        IFeatureFlagHistoryRepository historyRepository)
    {
        _repository = repository;
        _historyRepository = historyRepository;
    }

    public async Task<FeatureFlag?> Handle(AddTargetUsersCommand request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (flag == null) return null;

        flag.AddTargetUsers(request.UserIds, request.ModifiedBy);

        await _repository.UpdateAsync(flag, cancellationToken);

        var history = new FeatureFlagHistory
        {
            FeatureFlagId = flag.Id,
            ChangeType = ChangeType.TargetingChange,
            NewValue = $"Added users: {string.Join(", ", request.UserIds)}",
            ChangedBy = request.ModifiedBy,
            ChangedAt = DateTime.UtcNow
        };
        await _historyRepository.AddAsync(history, cancellationToken);

        return flag;
    }
}

public class RemoveTargetUsersHandler : IRequestHandler<RemoveTargetUsersCommand, FeatureFlag?>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagHistoryRepository _historyRepository;

    public RemoveTargetUsersHandler(
        IFeatureFlagRepository repository,
        IFeatureFlagHistoryRepository historyRepository)
    {
        _repository = repository;
        _historyRepository = historyRepository;
    }

    public async Task<FeatureFlag?> Handle(RemoveTargetUsersCommand request, CancellationToken cancellationToken)
    {
        var flag = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (flag == null) return null;

        flag.RemoveTargetUsers(request.UserIds, request.ModifiedBy);

        await _repository.UpdateAsync(flag, cancellationToken);

        var history = new FeatureFlagHistory
        {
            FeatureFlagId = flag.Id,
            ChangeType = ChangeType.TargetingChange,
            NewValue = $"Removed users: {string.Join(", ", request.UserIds)}",
            ChangedBy = request.ModifiedBy,
            ChangedAt = DateTime.UtcNow
        };
        await _historyRepository.AddAsync(history, cancellationToken);

        return flag;
    }
}
