using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Interfaces;
using MediatR;

namespace AdvertisingService.Application.Features.HomepageConfig.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryImageConfigDto>>
{
    private readonly ICategoryConfigRepository _categoryRepo;

    public GetCategoriesQueryHandler(ICategoryConfigRepository categoryRepo) => _categoryRepo = categoryRepo;

    public async Task<List<CategoryImageConfigDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var categories = request.IncludeHidden
            ? await _categoryRepo.GetAllAsync(ct)
            : await _categoryRepo.GetAllVisibleAsync(ct);

        return categories.Select(c => new CategoryImageConfigDto(
            c.Id, c.CategoryKey, c.DisplayName, c.Description, c.ImageUrl,
            c.IconUrl, c.Gradient, c.VehicleCount, c.IsTrending, c.DisplayOrder,
            c.IsVisible, c.Route
        )).ToList();
    }
}
