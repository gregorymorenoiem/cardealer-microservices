using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Interfaces;
using MediatR;

namespace AdvertisingService.Application.Features.HomepageConfig.Queries.GetBrands;

public class GetBrandsQueryHandler : IRequestHandler<GetBrandsQuery, List<BrandConfigDto>>
{
    private readonly IBrandConfigRepository _brandRepo;

    public GetBrandsQueryHandler(IBrandConfigRepository brandRepo) => _brandRepo = brandRepo;

    public async Task<List<BrandConfigDto>> Handle(GetBrandsQuery request, CancellationToken ct)
    {
        var brands = request.IncludeHidden
            ? await _brandRepo.GetAllAsync(ct)
            : await _brandRepo.GetAllVisibleAsync(ct);

        return brands.Select(b => new BrandConfigDto(
            b.Id, b.BrandKey, b.DisplayName, b.LogoUrl, b.LogoInitials,
            b.VehicleCount, b.DisplayOrder, b.IsVisible, b.Route
        )).ToList();
    }
}
