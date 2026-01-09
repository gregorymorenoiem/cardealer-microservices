using MediatR;
using VehicleIntelligenceService.Application.DTOs;

namespace VehicleIntelligenceService.Application.Features.Demand.Queries;

public record GetDemandByCategoryQuery : IRequest<List<CategoryDemandDto>>;
