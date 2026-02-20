using AdvertisingService.Application.DTOs;
using MediatR;

namespace AdvertisingService.Application.Features.HomepageConfig.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    string CategoryKey,
    string? DisplayName,
    string? Description,
    string? ImageUrl,
    string? IconUrl,
    string? Gradient,
    int? VehicleCount,
    bool? IsTrending,
    int? DisplayOrder,
    bool? IsVisible,
    string? Route
) : IRequest<CategoryImageConfigDto>;
