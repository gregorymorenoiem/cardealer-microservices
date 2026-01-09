using MediatR;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Base;

namespace ReviewService.Application.Features.Reviews.Queries;

/// <summary>
/// Query para obtener una review específica por ID
/// </summary>
public record GetReviewByIdQuery : IRequest<Result<ReviewDto>>
{
    public Guid ReviewId { get; init; }
}