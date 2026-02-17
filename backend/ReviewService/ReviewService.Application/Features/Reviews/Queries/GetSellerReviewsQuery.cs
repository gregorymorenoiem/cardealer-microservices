using MediatR;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Base;

namespace ReviewService.Application.Features.Reviews.Queries;

/// <summary>
/// Query para obtener reviews de un vendedor con paginación y filtros
/// </summary>
public record GetSellerReviewsQuery : IRequest<Result<PagedReviewsDto>>
{
    public Guid SellerId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public ReviewFiltersDto? Filters { get; init; }
    public bool OnlyApproved { get; init; } = true;
}