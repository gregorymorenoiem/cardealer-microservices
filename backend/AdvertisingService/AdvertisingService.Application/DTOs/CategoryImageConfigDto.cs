namespace AdvertisingService.Application.DTOs;

public record CategoryImageConfigDto(
    Guid Id,
    string CategoryKey,
    string DisplayName,
    string? Description,
    string ImageUrl,
    string? IconUrl,
    string Gradient,
    int VehicleCount,
    bool IsTrending,
    int DisplayOrder,
    bool IsVisible,
    string Route
);
