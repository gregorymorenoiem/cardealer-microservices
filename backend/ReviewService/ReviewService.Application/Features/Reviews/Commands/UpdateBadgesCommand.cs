using MediatR;
using ReviewService.Domain.Base;
using ReviewService.Application.DTOs;

namespace ReviewService.Application.Features.Reviews.Commands;

/// <summary>
/// Sprint 15 - Actualizar badges de un vendedor basado en sus reviews
/// Se ejecuta automáticamente después de cada nueva review
/// </summary>
public record UpdateBadgesCommand : IRequest<Result<BadgeUpdateResultDto>>
{
    public Guid SellerId { get; init; }
}
