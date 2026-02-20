using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Enums;
using MediatR;

namespace AdvertisingService.Application.Features.Rotation.Queries.GetRotationConfig;

public record GetRotationConfigQuery(AdPlacementType Section) : IRequest<RotationConfigDto?>;
