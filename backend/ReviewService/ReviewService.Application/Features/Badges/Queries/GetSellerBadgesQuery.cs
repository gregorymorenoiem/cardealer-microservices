using MediatR;
using ReviewService.Application.DTOs;

namespace ReviewService.Application.Features.Badges.Queries;

/// <summary>
/// Query para obtener badges activos de un vendedor
/// </summary>
public record GetSellerBadgesQuery(Guid SellerId) : IRequest<List<SellerBadgeDto>>;