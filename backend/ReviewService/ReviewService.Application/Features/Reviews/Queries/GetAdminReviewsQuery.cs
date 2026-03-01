using MediatR;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Base;

namespace ReviewService.Application.Features.Reviews.Queries;

/// <summary>
/// Query para obtener todas las reviews con paginación y filtros (admin)
/// </summary>
public record GetAdminReviewsQuery : IRequest<Result<AdminReviewListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public string? Status { get; init; }
}

/// <summary>
/// Query para obtener estadísticas globales de reviews (admin)
/// </summary>
public record GetAdminReviewStatsQuery : IRequest<Result<AdminReviewStatsDto>>;
