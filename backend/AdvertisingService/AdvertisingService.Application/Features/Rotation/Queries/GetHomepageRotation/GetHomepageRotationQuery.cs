using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Enums;
using MediatR;

namespace AdvertisingService.Application.Features.Rotation.Queries.GetHomepageRotation;

public record GetHomepageRotationQuery(AdPlacementType Section) : IRequest<HomepageRotationDto?>;
