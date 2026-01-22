using MediatR;
using VehicleIntelligenceService.Application.DTOs;

namespace VehicleIntelligenceService.Application.Features.Demand.Queries;

public record GetDemandByCategoryQuery : IRequest<List<CategoryDemandDto>>;

public class GetDemandByCategoryHandler : IRequestHandler<GetDemandByCategoryQuery, List<CategoryDemandDto>>
{
    public Task<List<CategoryDemandDto>> Handle(GetDemandByCategoryQuery request, CancellationToken cancellationToken)
    {
        // En producción, esto vendría de la base de datos/ML
        var result = new List<CategoryDemandDto>
        {
            new("SUV", 78, "up", DateTime.UtcNow),
            new("Sedan", 60, "stable", DateTime.UtcNow),
            new("Camioneta", 82, "up", DateTime.UtcNow),
            new("Deportivo", 65, "stable", DateTime.UtcNow),
            new("Electrico", 70, "up", DateTime.UtcNow),
            new("Hatchback", 55, "down", DateTime.UtcNow),
            new("Minivan", 45, "stable", DateTime.UtcNow),
            new("Lujo", 68, "up", DateTime.UtcNow)
        };

        return Task.FromResult(result);
    }
}
