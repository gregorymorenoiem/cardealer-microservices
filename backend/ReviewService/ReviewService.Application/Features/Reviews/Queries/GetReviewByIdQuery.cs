using MediatR;
using ReviewService.Application.DTOs;
using CarDealer.Shared.Application.Interfaces;

namespace ReviewService.Application.Features.Reviews.Queries;

/// &lt;summary&gt;
/// Query para obtener una review específica por ID
/// &lt;/summary&gt;
public record GetReviewByIdQuery : IRequest&lt;Result&lt;ReviewDto&gt;&gt;
{
    public Guid ReviewId { get; init; }
}