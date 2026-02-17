using MediatR;
using SpyneIntegrationService.Application.DTOs;

namespace SpyneIntegrationService.Application.Features.Images.Queries;

/// <summary>
/// Query to get transformation status
/// </summary>
public record GetTransformationStatusQuery(Guid TransformationId) : IRequest<ImageTransformationDto?>;
