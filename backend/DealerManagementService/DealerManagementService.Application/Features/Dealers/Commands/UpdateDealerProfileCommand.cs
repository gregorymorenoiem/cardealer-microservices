using DealerManagementService.Application.DTOs;
using DealerManagementService.Domain.Interfaces;
using MediatR;

namespace DealerManagementService.Application.Features.Dealers.Commands;

public record UpdateDealerProfileCommand(
    Guid DealerId,
    UpdateProfileRequest Request
) : IRequest<PublicDealerProfileDto>;

public class UpdateDealerProfileCommandHandler : IRequestHandler<UpdateDealerProfileCommand, PublicDealerProfileDto>
{
    private readonly IDealerRepository _dealerRepository;

    public UpdateDealerProfileCommandHandler(IDealerRepository dealerRepository)
    {
        _dealerRepository = dealerRepository;
    }

    public async Task<PublicDealerProfileDto> Handle(UpdateDealerProfileCommand request, CancellationToken cancellationToken)
    {
        var dealer = await _dealerRepository.GetByIdAsync(request.DealerId, cancellationToken);
        
        if (dealer == null)
            throw new Exception("Dealer not found");

        // Update fields
        if (request.Request.Slogan != null)
            dealer.Slogan = request.Request.Slogan;
        
        if (request.Request.AboutUs != null)
            dealer.AboutUs = request.Request.AboutUs;
        
        if (request.Request.Specialties != null)
            dealer.Specialties = request.Request.Specialties;
        
        if (request.Request.SupportedBrands != null)
            dealer.SupportedBrands = request.Request.SupportedBrands;
        
        if (request.Request.LogoUrl != null)
            dealer.LogoUrl = request.Request.LogoUrl;
        
        if (request.Request.BannerUrl != null)
            dealer.BannerUrl = request.Request.BannerUrl;
        
        if (request.Request.FacebookUrl != null)
            dealer.FacebookUrl = request.Request.FacebookUrl;
        
        if (request.Request.InstagramUrl != null)
            dealer.InstagramUrl = request.Request.InstagramUrl;
        
        if (request.Request.TwitterUrl != null)
            dealer.TwitterUrl = request.Request.TwitterUrl;
        
        if (request.Request.YouTubeUrl != null)
            dealer.YouTubeUrl = request.Request.YouTubeUrl;
        
        if (request.Request.WhatsAppNumber != null)
            dealer.WhatsAppNumber = request.Request.WhatsAppNumber;
        
        if (request.Request.ShowPhoneOnProfile.HasValue)
            dealer.ShowPhoneOnProfile = request.Request.ShowPhoneOnProfile.Value;
        
        if (request.Request.ShowEmailOnProfile.HasValue)
            dealer.ShowEmailOnProfile = request.Request.ShowEmailOnProfile.Value;
        
        if (request.Request.AcceptsTradeIns.HasValue)
            dealer.AcceptsTradeIns = request.Request.AcceptsTradeIns.Value;
        
        if (request.Request.OffersFinancing.HasValue)
            dealer.OffersFinancing = request.Request.OffersFinancing.Value;
        
        if (request.Request.OffersWarranty.HasValue)
            dealer.OffersWarranty = request.Request.OffersWarranty.Value;
        
        if (request.Request.OffersHomeDelivery.HasValue)
            dealer.OffersHomeDelivery = request.Request.OffersHomeDelivery.Value;
        
        if (request.Request.MetaTitle != null)
            dealer.MetaTitle = request.Request.MetaTitle;
        
        if (request.Request.MetaDescription != null)
            dealer.MetaDescription = request.Request.MetaDescription;
        
        if (request.Request.MetaKeywords != null)
            dealer.MetaKeywords = request.Request.MetaKeywords;

        // Generate slug if business name changed
        if (string.IsNullOrWhiteSpace(dealer.Slug))
        {
            dealer.Slug = dealer.GenerateSlug();
            
            // Ensure unique slug
            int suffix = 1;
            string originalSlug = dealer.Slug;
            while (await _dealerRepository.SlugExistsAsync(dealer.Slug, dealer.Id, cancellationToken))
            {
                dealer.Slug = $"{originalSlug}-{suffix}";
                suffix++;
            }
        }

        dealer.UpdatedAt = DateTime.UtcNow;
        
        await _dealerRepository.UpdateProfileAsync(dealer, cancellationToken);

        return new PublicDealerProfileDto(
            dealer.Id,
            dealer.BusinessName,
            dealer.Slogan,
            dealer.Description,
            dealer.AboutUs,
            dealer.LogoUrl,
            dealer.BannerUrl,
            dealer.EstablishedDate,
            dealer.Slug,
            dealer.City,
            dealer.Province,
            dealer.IsTrustedDealer,
            dealer.IsFoundingMember,
            dealer.TrustedDealerSince,
            dealer.AverageRating,
            dealer.TotalReviews,
            dealer.TotalSales,
            dealer.CurrentActiveListings,
            dealer.Specialties,
            dealer.SupportedBrands,
            new PublicContactInfo(
                dealer.ShowPhoneOnProfile ? dealer.Phone : null,
                dealer.ShowEmailOnProfile ? dealer.Email : null,
                dealer.Website,
                dealer.WhatsAppNumber,
                dealer.ShowPhoneOnProfile,
                dealer.ShowEmailOnProfile
            ),
            new List<DealerFeature>
            {
                new("Acepta Trade-Ins", "swap-horizontal", dealer.AcceptsTradeIns),
                new("Ofrece Financiamiento", "cash", dealer.OffersFinancing),
                new("Ofrece Garantía", "shield-checkmark", dealer.OffersWarranty),
                new("Entrega a Domicilio", "car", dealer.OffersHomeDelivery)
            },
            new List<PublicLocationDto>(),
            new SocialMediaLinks(
                dealer.FacebookUrl,
                dealer.InstagramUrl,
                dealer.TwitterUrl,
                dealer.YouTubeUrl
            ),
            new SEOMetadata(
                dealer.MetaTitle,
                dealer.MetaDescription,
                dealer.MetaKeywords
            )
        );
    }
}
