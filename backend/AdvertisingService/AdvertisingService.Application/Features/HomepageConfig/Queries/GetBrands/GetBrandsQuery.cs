using AdvertisingService.Application.DTOs;
using MediatR;

namespace AdvertisingService.Application.Features.HomepageConfig.Queries.GetBrands;

public record GetBrandsQuery(bool IncludeHidden = false) : IRequest<List<BrandConfigDto>>;
