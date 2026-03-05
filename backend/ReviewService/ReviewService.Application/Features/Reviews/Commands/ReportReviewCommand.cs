using MediatR;
using Microsoft.Extensions.Logging;
using ReviewService.Domain.Base;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Features.Reviews.Commands;

/// <summary>
/// Reportar/Flagear una review para moderación
/// </summary>
public record ReportReviewCommand : IRequest<Result<bool>>
{
    public Guid ReviewId { get; init; }
    public Guid ReportedByUserId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Handler para reportar una review
/// </summary>
public class ReportReviewCommandHandler : IRequestHandler<ReportReviewCommand, Result<bool>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<ReportReviewCommandHandler> _logger;

    public ReportReviewCommandHandler(
        IReviewRepository reviewRepository,
        ILogger<ReportReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ReportReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId);

        if (review == null)
        {
            return Result<bool>.Failure("Review no encontrada");
        }

        // Marcar la review como flagged para moderación
        review.IsFlagged = true;
        review.FlagReason = request.Reason?.Trim() ?? "Reportada por usuario";
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review);

        _logger.LogInformation(
            "Review {ReviewId} reported by user {UserId}. Reason: {Reason}",
            request.ReviewId, request.ReportedByUserId, request.Reason);

        return Result<bool>.Success(true);
    }
}
