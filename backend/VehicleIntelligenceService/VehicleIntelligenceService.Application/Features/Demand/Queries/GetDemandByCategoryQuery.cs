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
            new("SUV", "VeryHigh", 85, 18, 4500, 320, DateTime.UtcNow),
            new("Sedan", "High", 72, 25, 3800, 450, DateTime.UtcNow),
            new("Camioneta", "VeryHigh", 88, 14, 5200, 280, DateTime.UtcNow),
            new("Deportivo", "Medium", 58, 45, 1200, 85, DateTime.UtcNow),
            new("Electrico", "High", 75, 22, 2800, 120, DateTime.UtcNow),
            new("Hatchback", "Medium", 52, 35, 2100, 180, DateTime.UtcNow),
            new("Minivan", "Low", 38, 55, 800, 95, DateTime.UtcNow),
            new("Lujo", "Medium", 62, 42, 1500, 65, DateTime.UtcNow),
            new("Compacto", "High", 70, 28, 3200, 240, DateTime.UtcNow),
            new("Pickup", "VeryHigh", 82, 16, 4800, 350, DateTime.UtcNow)
        };

        return Task.FromResult(result);
    }
}
