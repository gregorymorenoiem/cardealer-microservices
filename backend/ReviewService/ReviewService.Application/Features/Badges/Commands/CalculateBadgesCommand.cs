using MediatR;
using ReviewService.Application.DTOs;

namespace ReviewService.Application.Features.Badges.Commands;

/// <summary>
/// Comando para calcular y otorgar badges automáticamente
/// </summary>
public record CalculateBadgesCommand(
    Guid? SellerId = null) : IRequest<BadgeCalculationResultDto>;  // Si SellerId es null, calcula para todos