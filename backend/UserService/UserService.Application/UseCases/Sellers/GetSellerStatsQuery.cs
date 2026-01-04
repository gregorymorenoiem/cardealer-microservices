using MediatR;
using UserService.Application.DTOs;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Application.UseCases.Sellers.GetSellerStats;

public record GetSellerStatsQuery(Guid SellerId) : IRequest<SellerStatsDto>;

public class GetSellerStatsQueryHandler : IRequestHandler<GetSellerStatsQuery, SellerStatsDto>
{
    private readonly ISellerProfileRepository _sellerProfileRepository;

    public GetSellerStatsQueryHandler(ISellerProfileRepository sellerProfileRepository)
    {
        _sellerProfileRepository = sellerProfileRepository;
    }

    public async Task<SellerStatsDto> Handle(GetSellerStatsQuery request, CancellationToken cancellationToken)
    {
        var profile = await _sellerProfileRepository.GetByIdAsync(request.SellerId);
        if (profile == null)
        {
            throw new NotFoundException($"Seller profile {request.SellerId} not found");
        }

        return new SellerStatsDto
        {
            SellerId = profile.Id,
            TotalListings = profile.TotalListings,
            ActiveListings = profile.ActiveListings,
            TotalSales = profile.TotalSales,
            AverageRating = profile.AverageRating,
            TotalReviews = profile.TotalReviews,
            ResponseTimeMinutes = profile.ResponseTimeMinutes,
            MaxActiveListings = profile.MaxActiveListings,
            CanSellHighValue = profile.CanSellHighValue,
            MemberSinceDays = (int)(DateTime.UtcNow - profile.CreatedAt).TotalDays
        };
    }
}
