using AdvertisingService.Application.DTOs;
using MediatR;

namespace AdvertisingService.Application.Features.HomepageConfig.Commands.UpdateBrand;

public record UpdateBrandCommand(
    string BrandKey,
    string? DisplayName,
    string? LogoUrl,
    string? LogoInitials,
    int? VehicleCount,
    int? DisplayOrder,
    bool? IsVisible,
    string? Route
) : IRequest<BrandConfigDto>;
