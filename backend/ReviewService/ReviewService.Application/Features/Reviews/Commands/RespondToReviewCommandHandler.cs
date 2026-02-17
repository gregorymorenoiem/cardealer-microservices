using MediatR;
using Microsoft.Extensions.Logging;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Features.Reviews.Commands;

/// <summary>
/// Handler para respuesta de vendedor a review
/// </summary>
public class RespondToReviewCommandHandler : IRequestHandler<RespondToReviewCommand, Unit>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IReviewResponseRepository _reviewResponseRepository;
    private readonly ILogger<RespondToReviewCommandHandler> _logger;

    public RespondToReviewCommandHandler(
        IReviewRepository reviewRepository,
        IReviewResponseRepository reviewResponseRepository,
        ILogger<RespondToReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _reviewResponseRepository = reviewResponseRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(RespondToReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
        if (review == null)
        {
            throw new InvalidOperationException($"Review with ID {request.ReviewId} not found");
        }

        // Verificar que el vendedor sea el correcto
        if (review.SellerId != request.SellerId)
        {
            throw new UnauthorizedAccessException("You can only respond to reviews for your own listings");
        }

        // Verificar que no haya respondido ya
        var existingResponse = await _reviewResponseRepository.GetByReviewIdAsync(request.ReviewId);
        if (existingResponse != null)
        {
            throw new InvalidOperationException("This review already has a seller response");
        }

        // Crear la respuesta
        var response = new ReviewResponse
        {
            Id = Guid.NewGuid(),
            ReviewId = request.ReviewId,
            SellerId = request.SellerId,
            Content = request.ResponseText.Trim(),
            SellerName = "Vendedor", // TODO: obtener nombre real del UserService
            IsApproved = true,
            CreatedAt = DateTime.UtcNow
        };

        await _reviewResponseRepository.AddAsync(response);

        _logger.LogInformation("Seller {SellerId} responded to review {ReviewId}", request.SellerId, request.ReviewId);

        return Unit.Value;
    }
}