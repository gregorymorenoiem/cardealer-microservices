using MediatR;
using SpyneIntegrationService.Application.DTOs;

namespace SpyneIntegrationService.Application.Features.Videos.Queries;

/// <summary>
/// Query to get video generation status
/// </summary>
public record GetVideoStatusQuery(Guid VideoId) : IRequest<VideoGenerationDto?>;
