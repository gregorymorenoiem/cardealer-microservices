using MediatR;
using SpyneIntegrationService.Application.DTOs;

namespace SpyneIntegrationService.Application.Features.Spins.Queries;

/// <summary>
/// Query to get spin generation status
/// </summary>
public record GetSpinStatusQuery(Guid SpinId) : IRequest<SpinGenerationDto?>;
