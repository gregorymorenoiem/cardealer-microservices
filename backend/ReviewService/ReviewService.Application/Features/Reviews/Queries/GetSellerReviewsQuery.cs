using MediatR;
using ReviewService.Application.DTOs;
using CarDealer.Shared.Application.Interfaces;

namespace ReviewService.Application.Features.Reviews.Queries;

/// &lt;summary&gt;
/// Query para obtener reviews de un vendedor con paginación y filtros
/// &lt;/summary&gt;
public record GetSellerReviewsQuery : IRequest&lt;Result&lt;PagedReviewsDto&gt;&gt;
{
    public Guid SellerId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public ReviewFiltersDto? Filters { get; init; }
    public bool OnlyApproved { get; init; } = true;
}