using AdvertisingService.Domain.Enums;
using MediatR;

namespace AdvertisingService.Application.Features.Rotation.Commands.RefreshRotation;

public record RefreshRotationCommand(AdPlacementType? Section = null) : IRequest<bool>;
