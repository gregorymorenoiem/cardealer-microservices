using System;
using System.Collections.Generic;

namespace RecommendationService.Application.DTOs;

public record RecommendationDto(
    Guid Id,
    Guid UserId,
    Guid VehicleId,
    string Type,
    double Score,
    string Reason,
    Dictionary<string, object> Metadata,
    DateTime CreatedAt,
    DateTime? ViewedAt,
    DateTime? ClickedAt,
    bool IsRelevant
);

public record UserPreferenceDto(
    Guid Id,
    Guid UserId,
    List<string> PreferredMakes,
    List<string> PreferredModels,
    List<string> PreferredBodyTypes,
    List<string> PreferredFuelTypes,
    List<string> PreferredTransmissions,
    int? MinYear,
    int? MaxYear,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? MinMileage,
    int? MaxMileage,
    List<string> PreferredColors,
    List<string> PreferredFeatures,
    double Confidence,
    int TotalVehiclesViewed,
    int TotalSearches,
    int TotalFavorites,
    int TotalContacts,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record VehicleInteractionDto(
    Guid Id,
    Guid UserId,
    Guid VehicleId,
    string Type,
    DateTime CreatedAt,
    int DurationSeconds,
    string? Source
);

public record CreateRecommendationRequest(
    Guid UserId,
    Guid VehicleId,
    string Type,
    double Score,
    string Reason
);

public record TrackInteractionRequest(
    Guid UserId,
    Guid VehicleId,
    string Type,
    int DurationSeconds = 0,
    string? Source = null
);

public record GenerateRecommendationsRequest(
    Guid UserId,
    int Limit = 10
);

public record GetSimilarVehiclesRequest(
    Guid VehicleId,
    int Limit = 10
);

public record RecommendationListResponse(
    List<RecommendationDto> Recommendations,
    int TotalCount
);
