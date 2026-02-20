namespace AdvertisingService.Application.DTOs;

public record BrandConfigDto(
    Guid Id,
    string BrandKey,
    string DisplayName,
    string? LogoUrl,
    string LogoInitials,
    int VehicleCount,
    int DisplayOrder,
    bool IsVisible,
    string Route
);
