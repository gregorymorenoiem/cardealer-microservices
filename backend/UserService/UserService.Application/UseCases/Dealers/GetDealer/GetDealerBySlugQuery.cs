using MediatR;
using UserService.Application.DTOs;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.Dealers.GetDealer;

/// <summary>
/// Query to get a dealer by its URL-friendly slug.
/// Used by the public dealer profile page: /dealers/[slug]
/// </summary>
public record GetDealerBySlugQuery(string Slug) : IRequest<DealerDto?>;

public class GetDealerBySlugQueryHandler : IRequestHandler<GetDealerBySlugQuery, DealerDto?>
{
    private readonly IDealerRepository _dealerRepository;

    public GetDealerBySlugQueryHandler(IDealerRepository dealerRepository)
    {
        _dealerRepository = dealerRepository;
    }

    public async Task<DealerDto?> Handle(GetDealerBySlugQuery request, CancellationToken cancellationToken)
    {
        var dealer = await _dealerRepository.GetBySlugAsync(request.Slug);
        if (dealer == null)
        {
            return null;
        }

        return new DealerDto
        {
            Id = dealer.Id,
            OwnerUserId = dealer.OwnerUserId,
            BusinessName = dealer.BusinessName,
            TradeName = dealer.TradeName,
            Slug = dealer.Slug,
            Description = dealer.Description,
            DealerType = dealer.DealerType,
            Email = dealer.Email,
            Phone = dealer.Phone,
            WhatsApp = dealer.WhatsApp,
            Website = dealer.Website,
            Address = dealer.Address,
            City = dealer.City,
            State = dealer.State,
            ZipCode = dealer.ZipCode,
            Country = dealer.Country,
            Latitude = dealer.Latitude,
            Longitude = dealer.Longitude,
            LogoUrl = dealer.LogoUrl,
            BannerUrl = dealer.BannerUrl,
            PrimaryColor = dealer.PrimaryColor,
            VerificationStatus = dealer.VerificationStatus,
            VerifiedAt = dealer.VerifiedAt,
            TotalListings = dealer.TotalListings,
            ActiveListings = dealer.ActiveListings,
            TotalSales = dealer.TotalSales,
            AverageRating = dealer.AverageRating,
            TotalReviews = dealer.TotalReviews,
            ResponseTimeMinutes = dealer.ResponseTimeMinutes,
            IsActive = dealer.IsActive,
            AcceptsFinancing = dealer.AcceptsFinancing,
            AcceptsTradeIn = dealer.AcceptsTradeIn,
            OffersWarranty = dealer.OffersWarranty,
            HomeDelivery = dealer.HomeDelivery,
            BusinessHours = dealer.BusinessHours,
            SocialMediaLinks = dealer.SocialMediaLinks,
            MaxListings = dealer.MaxListings,
            IsFeatured = dealer.IsFeatured,
            FeaturedUntil = dealer.FeaturedUntil,
            CreatedAt = dealer.CreatedAt,
            UpdatedAt = dealer.UpdatedAt,
        };
    }
}
