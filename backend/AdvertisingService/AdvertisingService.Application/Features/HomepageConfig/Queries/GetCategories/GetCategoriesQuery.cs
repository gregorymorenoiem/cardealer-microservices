using AdvertisingService.Application.DTOs;
using MediatR;

namespace AdvertisingService.Application.Features.HomepageConfig.Queries.GetCategories;

public record GetCategoriesQuery(bool IncludeHidden = false) : IRequest<List<CategoryImageConfigDto>>;
