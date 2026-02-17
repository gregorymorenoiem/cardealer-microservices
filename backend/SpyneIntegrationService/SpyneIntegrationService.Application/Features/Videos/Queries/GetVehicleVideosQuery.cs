using MediatR;
using SpyneIntegrationService.Application.DTOs;

namespace SpyneIntegrationService.Application.Features.Videos.Queries;

/// <summary>
/// Query to get videos for a vehicle
/// </summary>
public record GetVehicleVideosQuery(Guid VehicleId) : IRequest<List<VideoGenerationDto>>;
