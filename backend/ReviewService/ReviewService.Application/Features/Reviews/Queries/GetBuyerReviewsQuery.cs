using MediatR;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Base;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Features.Reviews.Queries;

/// <summary>
/// Obtener reviews escritas por un comprador específico
/// </summary>
public record GetBuyerReviewsQuery : IRequest<Result<PagedReviewsDto>>
{
    public Guid BuyerId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Handler para obtener las reviews de un comprador
/// </summary>
public class GetBuyerReviewsQueryHandler : IRequestHandler<GetBuyerReviewsQuery, Result<PagedReviewsDto>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetBuyerReviewsQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<PagedReviewsDto>> Handle(GetBuyerReviewsQuery request, CancellationToken cancellationToken)
    {
        var allReviews = await _reviewRepository.GetByBuyerIdAsync(request.BuyerId);

        var totalCount = allReviews.Count();
        var pagedReviews = allReviews
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var reviewDtos = pagedReviews.Select(r => new ReviewDto
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
            TrustScore = r.TrustScore,
            WasAutoRequested = r.WasAutoRequested,
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
