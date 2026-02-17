namespace UserBehaviorService.Application.DTOs;

public record UserBehaviorProfileDto(
    Guid Id,
    Guid UserId,
    string UserSegment,
    double EngagementScore,
    double PurchaseIntentScore,
    List<string> PreferredMakes,
    List<string> PreferredModels,
    List<int> PreferredYears,
    decimal? PreferredPriceMin,
    decimal? PreferredPriceMax,
    List<string> PreferredBodyTypes,
    List<string> PreferredFuelTypes,
    List<string> PreferredTransmissions,
    int TotalSearches,
    int TotalVehicleViews,
    int TotalContactRequests,
    int TotalFavorites,
    int TotalComparisons,
    int TotalSessions,
    TimeSpan TotalTimeSpent,
    List<string> RecentSearchQueries,
    List<Guid> RecentVehicleViews,
    DateTime? LastActivityAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record UserActionDto(
    Guid Id,
    Guid UserId,
    string ActionType,
    string ActionDetails,
    Guid? RelatedVehicleId,
    string? SearchQuery,
    DateTime Timestamp,
    string SessionId,
    string DeviceType
);

public record UserSegmentDto(
    string SegmentName,
    string Description,
    double MinEngagementScore,
    double MaxEngagementScore,
    double MinPurchaseIntentScore,
    double MaxPurchaseIntentScore,
    int MinActions,
    string Color,
    string Icon
);

public record UserBehaviorSummaryDto(
    int TotalUsers,
    int ActiveUsers7Days,
    int ActiveUsers30Days,
    Dictionary<string, int> SegmentDistribution,
    Dictionary<string, int> TopMakes,
    Dictionary<string, int> TopModels,
    decimal AveragePriceMin,
    decimal AveragePriceMax
);

public record RecordActionRequest(
    Guid UserId,
    string ActionType,
    string ActionDetails,
    Guid? RelatedVehicleId = null,
    string? SearchQuery = null,
    string? SessionId = null,
    string? DeviceType = null
);

public record UpdatePreferencesRequest(
    Guid UserId,
    List<string>? PreferredMakes = null,
    List<string>? PreferredModels = null,
    List<int>? PreferredYears = null,
    decimal? PreferredPriceMin = null,
    decimal? PreferredPriceMax = null,
    List<string>? PreferredBodyTypes = null
);
