using MediatR;
using CarDealer.Shared.Application.Interfaces;

namespace ReviewService.Application.Features.Reviews.Commands;

/// &lt;summary&gt;
/// Command para eliminar una review
/// &lt;/summary&gt;
public record DeleteReviewCommand : IRequest&lt;Result&lt;bool&gt;&gt;
{
    public Guid ReviewId { get; init; }
    public Guid BuyerId { get; init; } // Para validar que solo el autor puede eliminar
}