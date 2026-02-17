using MediatR;
using SpyneIntegrationService.Application.DTOs;

namespace SpyneIntegrationService.Application.Features.Images.Queries;

/// <summary>
/// Query to get all transformations for a vehicle
/// </summary>
public record GetVehicleTransformationsQuery(Guid VehicleId) : IRequest<List<ImageTransformationDto>>;
