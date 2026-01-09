using MediatR;
using ReviewService.Application.DTOs;
using CarDealer.Shared.Application.Interfaces;

namespace ReviewService.Application.Features.Reviews.Queries;

/// &lt;summary&gt;
/// Query para obtener estadísticas de reviews de un vendedor
/// &lt;/summary&gt;
public record GetReviewSummaryQuery : IRequest&lt;Result&lt;ReviewSummaryDto&gt;&gt;
{
    public Guid SellerId { get; init; }
}