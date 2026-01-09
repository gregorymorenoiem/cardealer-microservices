using MediatR;

namespace ReviewService.Application.Features.Reviews.Commands;

/// <summary>
/// Comando para que un vendedor responda a una review
/// </summary>
public record RespondToReviewCommand(
    Guid ReviewId,
    Guid SellerId,
    string ResponseText) : IRequest<Unit>;