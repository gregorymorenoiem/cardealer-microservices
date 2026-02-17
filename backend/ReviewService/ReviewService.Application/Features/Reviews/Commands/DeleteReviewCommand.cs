using MediatR;
using ReviewService.Domain.Base;

namespace ReviewService.Application.Features.Reviews.Commands;

/// <summary>
/// Command para eliminar una review
/// </summary>
public record DeleteReviewCommand : IRequest<Result<bool>>
{
    public Guid ReviewId { get; init; }
    public Guid BuyerId { get; init; } // Para validar que solo el autor puede eliminar
}