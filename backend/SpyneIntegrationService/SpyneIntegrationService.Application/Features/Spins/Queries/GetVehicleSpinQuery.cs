using MediatR;
using SpyneIntegrationService.Application.DTOs;

namespace SpyneIntegrationService.Application.Features.Spins.Queries;

/// <summary>
/// Query to get spin for a vehicle
/// </summary>
public record GetVehicleSpinQuery(Guid VehicleId) : IRequest<SpinGenerationDto?>;
