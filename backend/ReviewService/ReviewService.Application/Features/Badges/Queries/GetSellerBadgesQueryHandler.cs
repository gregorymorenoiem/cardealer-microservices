using MediatR;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Features.Badges.Queries;

/// <summary>
/// Handler para obtener badges de vendedor
/// </summary>
public class GetSellerBadgesQueryHandler : IRequestHandler<GetSellerBadgesQuery, List<SellerBadgeDto>>
{
    private readonly ISellerBadgeRepository _sellerBadgeRepository;

    public GetSellerBadgesQueryHandler(ISellerBadgeRepository sellerBadgeRepository)
    {
        _sellerBadgeRepository = sellerBadgeRepository;
    }

    public async Task<List<SellerBadgeDto>> Handle(GetSellerBadgesQuery request, CancellationToken cancellationToken)
    {
        var badges = await _sellerBadgeRepository.GetActiveBySellerIdAsync(request.SellerId, cancellationToken);
        
        return badges.Select(badge => new SellerBadgeDto
        {
            Id = badge.Id,
            SellerId = badge.SellerId,
            BadgeType = badge.BadgeType.ToString(),
            BadgeDisplayName = GetBadgeDisplayName(badge.BadgeType),
            BadgeDescription = GetBadgeDescription(badge.BadgeType),
            BadgeColor = GetBadgeColor(badge.BadgeType),
            GrantedAt = badge.EarnedAt,
            IsActive = badge.IsActive,
            Notes = badge.Criteria
        }).ToList();
    }

    private string GetBadgeDisplayName(Domain.Entities.BadgeType badgeType)
    {
        return badgeType switch
        {
            Domain.Entities.BadgeType.TopRated => "Top Rated",
            Domain.Entities.BadgeType.TrustedDealer => "Trusted Dealer",
            Domain.Entities.BadgeType.FiveStarSeller => "Five Star Seller",
            Domain.Entities.BadgeType.QuickResponder => "Quick Responder",
            Domain.Entities.BadgeType.VerifiedProfessional => "Verified Professional",
            Domain.Entities.BadgeType.CustomerChoice => "Customer Choice",
            Domain.Entities.BadgeType.RisingStar => "Rising Star",
            Domain.Entities.BadgeType.VolumeLeader => "Volume Leader",
            Domain.Entities.BadgeType.ConsistencyWinner => "Consistency Winner",
            Domain.Entities.BadgeType.CommunityFavorite => "Community Favorite",
            _ => badgeType.ToString()
        };
    }

    private string GetBadgeDescription(Domain.Entities.BadgeType badgeType)
    {
        return badgeType switch
        {
            Domain.Entities.BadgeType.TopRated => "Consistently high ratings from customers",
            Domain.Entities.BadgeType.TrustedDealer => "Verified dealer with excellent reputation",
            Domain.Entities.BadgeType.FiveStarSeller => "Majority of reviews are 5 stars",
            Domain.Entities.BadgeType.QuickResponder => "Quick to respond to customer inquiries",
            Domain.Entities.BadgeType.VerifiedProfessional => "High volume dealer with consistent quality",
            Domain.Entities.BadgeType.CustomerChoice => "Highly recommended by customers",
            Domain.Entities.BadgeType.RisingStar => "New seller with exceptional start",
            Domain.Entities.BadgeType.VolumeLeader => "High-volume seller with proven track record",
            Domain.Entities.BadgeType.ConsistencyWinner => "Consistently delivers quality service",
            Domain.Entities.BadgeType.CommunityFavorite => "Goes above and beyond for customers",
            _ => "Achievement badge"
        };
    }

    private string GetBadgeColor(Domain.Entities.BadgeType badgeType)
    {
        return badgeType switch
        {
            Domain.Entities.BadgeType.TopRated => "gold",
            Domain.Entities.BadgeType.TrustedDealer => "blue",
            Domain.Entities.BadgeType.FiveStarSeller => "purple",
            Domain.Entities.BadgeType.QuickResponder => "green",
            Domain.Entities.BadgeType.VerifiedProfessional => "indigo",
            Domain.Entities.BadgeType.CustomerChoice => "pink",
            Domain.Entities.BadgeType.RisingStar => "emerald",
            Domain.Entities.BadgeType.VolumeLeader => "amber",
            Domain.Entities.BadgeType.ConsistencyWinner => "cyan",
            Domain.Entities.BadgeType.CommunityFavorite => "red",
            _ => "gray"
        };
    }
}