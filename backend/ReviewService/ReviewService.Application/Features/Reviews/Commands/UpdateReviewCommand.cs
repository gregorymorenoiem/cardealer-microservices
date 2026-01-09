using MediatR;
using ReviewService.Application.DTOs;
using CarDealer.Shared.Application.Interfaces;

namespace ReviewService.Application.Features.Reviews.Commands;

/// &lt;summary&gt;
/// Command para actualizar una review existente
/// &lt;/summary&gt;
public record UpdateReviewCommand : IRequest&lt;Result&lt;ReviewDto&gt;&gt;
{
    public Guid ReviewId { get; init; }
    public Guid BuyerId { get; init; } // Para validar que solo el autor puede editar
    public int Rating { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}