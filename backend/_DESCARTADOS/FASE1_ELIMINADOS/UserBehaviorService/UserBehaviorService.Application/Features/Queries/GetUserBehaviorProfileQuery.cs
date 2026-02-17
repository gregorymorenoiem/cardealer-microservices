using MediatR;
using UserBehaviorService.Application.DTOs;
using UserBehaviorService.Domain.Interfaces;

namespace UserBehaviorService.Application.Features.Queries;

public record GetUserBehaviorProfileQuery(Guid UserId) : IRequest<UserBehaviorProfileDto?>;

public class GetUserBehaviorProfileHandler : IRequestHandler<GetUserBehaviorProfileQuery, UserBehaviorProfileDto?>
{
    private readonly IUserBehaviorRepository _repository;

    public GetUserBehaviorProfileHandler(IUserBehaviorRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserBehaviorProfileDto?> Handle(GetUserBehaviorProfileQuery request, CancellationToken ct)
    {
        var profile = await _repository.GetByUserIdAsync(request.UserId, ct);
        if (profile == null) return null;

        return new UserBehaviorProfileDto(
            profile.Id,
            profile.UserId,
            profile.UserSegment,
            profile.EngagementScore,
            profile.PurchaseIntentScore,
            profile.PreferredMakes,
            profile.PreferredModels,
            profile.PreferredYears,
            profile.PreferredPriceMin,
            profile.PreferredPriceMax,
            profile.PreferredBodyTypes,
            profile.PreferredFuelTypes,
            profile.PreferredTransmissions,
            profile.TotalSearches,
            profile.TotalVehicleViews,
            profile.TotalContactRequests,
            profile.TotalFavorites,
            profile.TotalComparisons,
            profile.TotalSessions,
            profile.TotalTimeSpent,
            profile.RecentSearchQueries,
            profile.RecentVehicleViews,
            profile.LastActivityAt,
            profile.CreatedAt,
            profile.UpdatedAt
        );
    }
}

public record GetUserActionsQuery(Guid UserId, int Limit = 50) : IRequest<List<UserActionDto>>;

public class GetUserActionsHandler : IRequestHandler<GetUserActionsQuery, List<UserActionDto>>
{
    private readonly IUserBehaviorRepository _repository;

    public GetUserActionsHandler(IUserBehaviorRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserActionDto>> Handle(GetUserActionsQuery request, CancellationToken ct)
    {
        var actions = await _repository.GetUserActionsAsync(request.UserId, request.Limit, ct);
        return actions.Select(a => new UserActionDto(
            a.Id,
            a.UserId,
            a.ActionType,
            a.ActionDetails,
            a.RelatedVehicleId,
            a.SearchQuery,
            a.Timestamp,
            a.SessionId,
            a.DeviceType
        )).ToList();
    }
}

public record GetBehaviorSummaryQuery() : IRequest<UserBehaviorSummaryDto>;

public class GetBehaviorSummaryHandler : IRequestHandler<GetBehaviorSummaryQuery, UserBehaviorSummaryDto>
{
    private readonly IUserBehaviorRepository _repository;

    public GetBehaviorSummaryHandler(IUserBehaviorRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserBehaviorSummaryDto> Handle(GetBehaviorSummaryQuery request, CancellationToken ct)
    {
        var segmentDistribution = await _repository.GetSegmentDistributionAsync(ct);
        var topMakes = await _repository.GetTopPreferredMakesAsync(10, ct);
        var topModels = await _repository.GetTopPreferredModelsAsync(10, ct);
        var avgPriceRange = await _repository.GetAveragePriceRangeAsync(ct);
        var activeUsers7 = await _repository.GetActiveUsersCountAsync(7, ct);
        var activeUsers30 = await _repository.GetActiveUsersCountAsync(30, ct);

        return new UserBehaviorSummaryDto(
            segmentDistribution.Values.Sum(),
            activeUsers7,
            activeUsers30,
            segmentDistribution,
            topMakes,
            topModels,
            avgPriceRange.Min,
            avgPriceRange.Max
        );
    }
}
