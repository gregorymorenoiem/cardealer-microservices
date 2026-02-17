using FeatureToggleService.Application.Interfaces;
using FeatureToggleService.Application.Queries;
using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using FeatureToggleService.Domain.Interfaces;
using MediatR;

namespace FeatureToggleService.Application.Handlers;

public class GetFeatureFlagByIdQueryHandler : IRequestHandler<GetFeatureFlagByIdQuery, FeatureFlag?>
{
    private readonly IFeatureFlagRepository _repository;

    public GetFeatureFlagByIdQueryHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    public async Task<FeatureFlag?> Handle(GetFeatureFlagByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}

public class GetFeatureFlagByKeyQueryHandler : IRequestHandler<GetFeatureFlagByKeyQuery, FeatureFlag?>
{
    private readonly IFeatureFlagRepository _repository;

    public GetFeatureFlagByKeyQueryHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    public async Task<FeatureFlag?> Handle(GetFeatureFlagByKeyQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByKeyAsync(request.Key, cancellationToken);
    }
}

public class GetAllFeatureFlagsQueryHandler : IRequestHandler<GetAllFeatureFlagsQuery, IEnumerable<FeatureFlag>>
{
    private readonly IFeatureFlagRepository _repository;

    public GetAllFeatureFlagsQueryHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<FeatureFlag>> Handle(GetAllFeatureFlagsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}

public class GetFeatureFlagsByEnvironmentQueryHandler : IRequestHandler<GetFeatureFlagsByEnvironmentQuery, IEnumerable<FeatureFlag>>
{
    private readonly IFeatureFlagRepository _repository;

    public GetFeatureFlagsByEnvironmentQueryHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<FeatureFlag>> Handle(GetFeatureFlagsByEnvironmentQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByEnvironmentAsync(request.Environment, cancellationToken);
    }
}

public class GetFeatureFlagsByStatusQueryHandler : IRequestHandler<GetFeatureFlagsByStatusQuery, IEnumerable<FeatureFlag>>
{
    private readonly IFeatureFlagRepository _repository;

    public GetFeatureFlagsByStatusQueryHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<FeatureFlag>> Handle(GetFeatureFlagsByStatusQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByStatusAsync(request.Status, cancellationToken);
    }
}

public class GetFeatureFlagsByTagQueryHandler : IRequestHandler<GetFeatureFlagsByTagQuery, IEnumerable<FeatureFlag>>
{
    private readonly IFeatureFlagRepository _repository;

    public GetFeatureFlagsByTagQueryHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<FeatureFlag>> Handle(GetFeatureFlagsByTagQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByTagAsync(request.Tag, cancellationToken);
    }
}

public class GetActiveFeatureFlagsQueryHandler : IRequestHandler<GetActiveFeatureFlagsQuery, IEnumerable<FeatureFlag>>
{
    private readonly IFeatureFlagRepository _repository;

    public GetActiveFeatureFlagsQueryHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<FeatureFlag>> Handle(GetActiveFeatureFlagsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetActiveAsync(cancellationToken);
    }
}

public class GetExpiredFeatureFlagsQueryHandler : IRequestHandler<GetExpiredFeatureFlagsQuery, IEnumerable<FeatureFlag>>
{
    private readonly IFeatureFlagRepository _repository;

    public GetExpiredFeatureFlagsQueryHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<FeatureFlag>> Handle(GetExpiredFeatureFlagsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetExpiredAsync(cancellationToken);
    }
}

public class EvaluateFeatureFlagQueryHandler : IRequestHandler<EvaluateFeatureFlagQuery, bool>
{
    private readonly IFeatureFlagEvaluator _evaluator;

    public EvaluateFeatureFlagQueryHandler(IFeatureFlagEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    public async Task<bool> Handle(EvaluateFeatureFlagQuery request, CancellationToken cancellationToken)
    {
        return await _evaluator.EvaluateAsync(request.FlagKey, request.Context, cancellationToken);
    }
}

public class EvaluateMultipleFeatureFlagsQueryHandler : IRequestHandler<EvaluateMultipleFeatureFlagsQuery, Dictionary<string, bool>>
{
    private readonly IFeatureFlagEvaluator _evaluator;

    public EvaluateMultipleFeatureFlagsQueryHandler(IFeatureFlagEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    public async Task<Dictionary<string, bool>> Handle(EvaluateMultipleFeatureFlagsQuery request, CancellationToken cancellationToken)
    {
        return await _evaluator.EvaluateMultipleAsync(request.FlagKeys, request.Context, cancellationToken);
    }
}

public class GetFeatureFlagHistoryQueryHandler : IRequestHandler<GetFeatureFlagHistoryQuery, IEnumerable<FeatureFlagHistory>>
{
    private readonly IFeatureFlagHistoryRepository _historyRepository;

    public GetFeatureFlagHistoryQueryHandler(IFeatureFlagHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository;
    }

    public async Task<IEnumerable<FeatureFlagHistory>> Handle(GetFeatureFlagHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _historyRepository.GetByFlagIdAsync(request.FlagId, cancellationToken: cancellationToken);
    }
}

public class GetFeatureFlagStatsQueryHandler : IRequestHandler<GetFeatureFlagStatsQuery, FeatureFlagStats>
{
    private readonly IFeatureFlagRepository _repository;

    public GetFeatureFlagStatsQueryHandler(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }

    public async Task<FeatureFlagStats> Handle(GetFeatureFlagStatsQuery request, CancellationToken cancellationToken)
    {
        var flags = (await _repository.GetAllAsync(cancellationToken)).ToList();

        var stats = new FeatureFlagStats
        {
            TotalFlags = flags.Count,
            ActiveFlags = flags.Count(f => f.Status == FlagStatus.Active && f.IsEnabled),
            InactiveFlags = flags.Count(f => f.Status == FlagStatus.Inactive || !f.IsEnabled),
            DraftFlags = flags.Count(f => f.Status == FlagStatus.Draft),
            ArchivedFlags = flags.Count(f => f.Status == FlagStatus.Archived),
            ExpiredFlags = flags.Count(f => f.IsExpired),
            KillSwitchActiveFlags = flags.Count(f => f.KillSwitchTriggered),
            FlagsByEnvironment = flags
                .GroupBy(f => f.Environment.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            FlagsByTag = flags
                .SelectMany(f => f.Tags)
                .GroupBy(t => t)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return stats;
    }
}
