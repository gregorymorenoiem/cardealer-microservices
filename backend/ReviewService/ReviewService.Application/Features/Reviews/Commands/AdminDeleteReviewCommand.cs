using MediatR;
using ReviewService.Domain.Base;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Features.Reviews.Commands;

/// <summary>
/// Command para eliminar una review sin validación de propiedad (admin)
/// </summary>
public record AdminDeleteReviewCommand : IRequest<Result<bool>>
{
    public Guid ReviewId { get; init; }
}

/// <summary>
/// Handler para AdminDeleteReviewCommand
/// </summary>
public class AdminDeleteReviewCommandHandler : IRequestHandler<AdminDeleteReviewCommand, Result<bool>>
{
    private readonly IReviewRepository _reviewRepository;

    public AdminDeleteReviewCommandHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<bool>> Handle(AdminDeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
        if (review == null)
            return Result<bool>.Failure("Review no encontrada");

        await _reviewRepository.AdminDeleteAsync(request.ReviewId);
        return Result<bool>.Success(true);
    }
}
