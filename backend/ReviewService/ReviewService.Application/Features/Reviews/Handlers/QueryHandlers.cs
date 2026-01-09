using MediatR;
using ReviewService.Application.Features.Reviews.Queries;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Interfaces;
using ReviewService.Domain.Base;

namespace ReviewService.Application.Features.Reviews.Handlers;

/// <summary>
/// Handler para GetSellerReviewsQuery
/// </summary>
public class GetSellerReviewsQueryHandler : IRequestHandler<GetSellerReviewsQuery, Result<PagedReviewsDto>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetSellerReviewsQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<PagedReviewsDto>> Handle(GetSellerReviewsQuery request, CancellationToken cancellationToken)
    {
        // Obtener reviews paginadas
        var (reviews, totalCount) = await _reviewRepository.GetBySellerIdAsync(
            request.SellerId,
            request.Page,
            request.PageSize,
            request.OnlyApproved);

        // Convertir a DTOs
        var reviewDtos = reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            BuyerId = r.BuyerId,
            SellerId = r.SellerId,
            VehicleId = r.VehicleId,
            OrderId = r.OrderId,
            Rating = r.Rating,
            Title = r.Title,
            Content = r.Content,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            BuyerName = r.BuyerName,
            BuyerPhotoUrl = r.BuyerPhotoUrl,
            HelpfulVotes = r.HelpfulVotes,
            TotalVotes = r.TotalVotes,
            CreatedAt = r.CreatedAt,
            Response = r.Response != null ? new ReviewResponseDto
            {
                Id = r.Response.Id,
                ReviewId = r.Response.ReviewId,
                SellerId = r.Response.SellerId,
                Content = r.Response.Content,
                SellerName = r.Response.SellerName,
                CreatedAt = r.Response.CreatedAt
            } : null
        });

        // Calcular paginación
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        var result = new PagedReviewsDto
        {
            Reviews = reviewDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1
        };

        return Result<PagedReviewsDto>.Success(result);
    }
}

/// <summary>
/// Handler para GetReviewSummaryQuery
/// </summary>
public class GetReviewSummaryQueryHandler : IRequestHandler<GetReviewSummaryQuery, Result<ReviewSummaryDto>>
{
    private readonly IReviewSummaryRepository _summaryRepository;

    public GetReviewSummaryQueryHandler(IReviewSummaryRepository summaryRepository)
    {
        _summaryRepository = summaryRepository;
    }

    public async Task<Result<ReviewSummaryDto>> Handle(GetReviewSummaryQuery request, CancellationToken cancellationToken)
    {
        // Obtener o crear summary
        var summary = await _summaryRepository.GetOrCreateBySellerIdAsync(request.SellerId);

        // Convertir a DTO
        var dto = new ReviewSummaryDto
        {
            SellerId = summary.SellerId,
            TotalReviews = summary.TotalReviews,
            AverageRating = summary.AverageRating,
            RatingDistribution = summary.GetRatingDistribution(),
            PositivePercentage = summary.PositivePercentage,
            VerifiedPurchaseReviews = summary.VerifiedPurchaseReviews,
            LastReviewDate = summary.LastReviewDate
        };

        return Result<ReviewSummaryDto>.Success(dto);
    }
}

/// <summary>
/// Handler para GetReviewByIdQuery
/// </summary>
public class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, Result<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetReviewByIdQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<ReviewDto>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId);

        if (review == null)
        {
            return Result<ReviewDto>.Failure("Review no encontrada");
        }

        var dto = new ReviewDto
        {
            Id = review.Id,
            BuyerId = review.BuyerId,
            SellerId = review.SellerId,
            VehicleId = review.VehicleId,
            OrderId = review.OrderId,
            Rating = review.Rating,
            Title = review.Title,
            Content = review.Content,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            BuyerName = review.BuyerName,
            BuyerPhotoUrl = review.BuyerPhotoUrl,
            HelpfulVotes = review.HelpfulVotes,
            TotalVotes = review.TotalVotes,
            CreatedAt = review.CreatedAt,
            Response = review.Response != null ? new ReviewResponseDto
            {
                Id = review.Response.Id,
                ReviewId = review.Response.ReviewId,
                SellerId = review.Response.SellerId,
                Content = review.Response.Content,
                SellerName = review.Response.SellerName,
                CreatedAt = review.Response.CreatedAt
            } : null
        };

        return Result<ReviewDto>.Success(dto);
    }
}