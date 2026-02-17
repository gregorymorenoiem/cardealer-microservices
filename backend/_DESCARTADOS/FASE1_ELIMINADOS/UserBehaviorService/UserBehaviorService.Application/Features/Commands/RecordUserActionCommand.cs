using MediatR;
using UserBehaviorService.Application.DTOs;
using UserBehaviorService.Domain.Entities;
using UserBehaviorService.Domain.Interfaces;

namespace UserBehaviorService.Application.Features.Commands;

public record RecordUserActionCommand(
    Guid UserId,
    string ActionType,
    string ActionDetails,
    Guid? RelatedVehicleId = null,
    string? SearchQuery = null,
    string? SessionId = null,
    string? DeviceType = null
) : IRequest<UserActionDto>;

public class RecordUserActionHandler : IRequestHandler<RecordUserActionCommand, UserActionDto>
{
    private readonly IUserBehaviorRepository _repository;

    public RecordUserActionHandler(IUserBehaviorRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserActionDto> Handle(RecordUserActionCommand request, CancellationToken ct)
    {
        var action = new UserAction
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ActionType = request.ActionType,
            ActionDetails = request.ActionDetails,
            RelatedVehicleId = request.RelatedVehicleId,
            SearchQuery = request.SearchQuery,
            Timestamp = DateTime.UtcNow,
            SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
            DeviceType = request.DeviceType ?? "Desktop"
        };

        var created = await _repository.AddActionAsync(action, ct);

        // Actualizar perfil del usuario automáticamente
        await UpdateUserProfileFromAction(created, ct);

        return new UserActionDto(
            created.Id,
            created.UserId,
            created.ActionType,
            created.ActionDetails,
            created.RelatedVehicleId,
            created.SearchQuery,
            created.Timestamp,
            created.SessionId,
            created.DeviceType
        );
    }

    private async Task UpdateUserProfileFromAction(UserAction action, CancellationToken ct)
    {
        var profile = await _repository.GetByUserIdAsync(action.UserId, ct) 
                      ?? new UserBehaviorProfile { Id = Guid.NewGuid(), UserId = action.UserId, CreatedAt = DateTime.UtcNow };

        // Actualizar contadores
        switch (action.ActionType.ToLower())
        {
            case "search":
                profile.TotalSearches++;
                if (!string.IsNullOrEmpty(action.SearchQuery))
                {
                    profile.RecentSearchQueries.Insert(0, action.SearchQuery);
                    if (profile.RecentSearchQueries.Count > 10) profile.RecentSearchQueries = profile.RecentSearchQueries.Take(10).ToList();
                }
                break;
            case "vehicleview":
                profile.TotalVehicleViews++;
                if (action.RelatedVehicleId.HasValue)
                {
                    profile.RecentVehicleViews.Insert(0, action.RelatedVehicleId.Value);
                    if (profile.RecentVehicleViews.Count > 20) profile.RecentVehicleViews = profile.RecentVehicleViews.Take(20).ToList();
                }
                break;
            case "contact":
                profile.TotalContactRequests++;
                break;
            case "favorite":
                profile.TotalFavorites++;
                break;
            case "comparison":
                profile.TotalComparisons++;
                break;
        }

        profile.LastActivityAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        // Recalcular scores
        profile.EngagementScore = CalculateEngagementScore(profile);
        profile.PurchaseIntentScore = CalculatePurchaseIntentScore(profile);
        profile.UserSegment = DetermineUserSegment(profile);

        await _repository.CreateOrUpdateAsync(profile, ct);
    }

    private double CalculateEngagementScore(UserBehaviorProfile profile)
    {
        var score = 0.0;
        score += profile.TotalSearches * 1.0;
        score += profile.TotalVehicleViews * 2.0;
        score += profile.TotalFavorites * 3.0;
        score += profile.TotalComparisons * 4.0;
        score += profile.TotalContactRequests * 5.0;
        return Math.Min(100, score / 10); // Normalizar a 0-100
    }

    private double CalculatePurchaseIntentScore(UserBehaviorProfile profile)
    {
        var score = 0.0;
        score += profile.TotalContactRequests * 25.0;
        score += profile.TotalFavorites * 10.0;
        score += profile.TotalComparisons * 15.0;
        score += (profile.TotalSearches > 5 ? 10 : 0);
        score += (profile.TotalVehicleViews > 10 ? 15 : 0);
        return Math.Min(100, score);
    }

    private string DetermineUserSegment(UserBehaviorProfile profile)
    {
        if (profile.PurchaseIntentScore >= 70 && profile.TotalContactRequests >= 2)
            return "SeriousBuyer";
        if (profile.EngagementScore >= 60 && profile.TotalSearches >= 3)
            return "Researcher";
        if (profile.TotalVehicleViews >= 10)
            return "Browser";
        if (profile.TotalSearches < 2 && profile.TotalVehicleViews < 3)
            return "TireKicker";
        return "Casual";
    }
}
