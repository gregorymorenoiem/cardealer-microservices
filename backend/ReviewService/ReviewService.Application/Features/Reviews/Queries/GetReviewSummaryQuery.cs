using MediatR;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Base;

namespace ReviewService.Application.Features.Reviews.Queries;

/// <summary>
/// Query para obtener estadísticas de reviews de un vendedor
/// </summary>
public record GetReviewSummaryQuery : IRequest<Result<ReviewSummaryDto>>
{
    public Guid SellerId { get; init; }
}