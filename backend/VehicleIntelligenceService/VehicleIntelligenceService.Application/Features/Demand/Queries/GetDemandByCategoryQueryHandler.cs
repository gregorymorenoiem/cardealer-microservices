using MediatR;
using VehicleIntelligenceService.Application.DTOs;
using VehicleIntelligenceService.Domain.Interfaces;

namespace VehicleIntelligenceService.Application.Features.Demand.Queries;

public class GetDemandByCategoryQueryHandler(ICategoryDemandRepository repository)
    : IRequestHandler<GetDemandByCategoryQuery, List<CategoryDemandDto>>
{
    public async Task<List<CategoryDemandDto>> Handle(GetDemandByCategoryQuery request, CancellationToken cancellationToken)
    {
        var data = await repository.GetAllAsync(cancellationToken);
        return data
            .OrderByDescending(d => d.DemandScore)
            .Select(d => new CategoryDemandDto(d.Category, d.DemandScore, d.Trend, d.UpdatedAt))
            .ToList();
    }
}
