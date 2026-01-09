using MediatR;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Base;

namespace ReviewService.Application.Features.Reviews.Commands;

/// <summary>
/// Command para actualizar una review existente
/// </summary>
public record UpdateReviewCommand : IRequest<Result<ReviewDto>>
{
    public Guid ReviewId { get; init; }
    public Guid BuyerId { get; init; } // Para validar que solo el autor puede editar
    public int Rating { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}