using DealerManagementService.Application.DTOs;
using DealerManagementService.Domain.Interfaces;
using MediatR;

namespace DealerManagementService.Application.Features.Dealers.Queries;

// Get Public Profile by Slug
public record GetPublicProfileQuery(string Slug) : IRequest<PublicDealerProfileDto?>;

public class GetPublicProfileQueryHandler : IRequestHandler<GetPublicProfileQuery, PublicDealerProfileDto?>
{
    private readonly IDealerRepository _dealerRepository;
    private readonly IDealerLocationRepository _dealerLocationRepository;

    public GetPublicProfileQueryHandler(
        IDealerRepository dealerRepository,
        IDealerLocationRepository dealerLocationRepository)
    {
        _dealerRepository = dealerRepository;
        _dealerLocationRepository = dealerLocationRepository;
    }

    public async Task<PublicDealerProfileDto?> Handle(GetPublicProfileQuery request, CancellationToken cancellationToken)
    {
        var dealer = await _dealerRepository.GetBySlugAsync(request.Slug, cancellationToken);
        
        if (dealer == null || dealer.Status != Domain.Entities.DealerStatus.Active)
            return null;

        var locations = await _dealerLocationRepository.GetByDealerIdAsync(dealer.Id, cancellationToken);
        
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
            locations.Select(MapToPublicLocationDto).ToList(),
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

    private static PublicLocationDto MapToPublicLocationDto(Domain.Entities.DealerLocation location)
    {
        return new PublicLocationDto(
            location.Id,
            location.Name,
            location.Type.ToString(),
            location.IsPrimary,
            location.Address,
            location.City,
            location.Province,
            location.Latitude,
            location.Longitude,
            location.Phone,
            location.Email,
            location.BusinessHours.Select(bh => new BusinessHoursDto(
                bh.DayOfWeek.ToString(),
                bh.IsOpen,
                bh.OpenTime?.ToString("HH:mm"),
                bh.CloseTime?.ToString("HH:mm"),
                bh.BreakStartTime?.ToString("HH:mm"),
                bh.BreakEndTime?.ToString("HH:mm"),
                bh.Notes,
                bh.GetFormattedHours()
            )).ToList(),
            location.HasShowroom,
            location.HasServiceCenter,
            location.HasParking,
            location.ParkingSpaces,
            location.IsActive
        );
    }
}

// Get Trusted Dealers
public record GetTrustedDealersQuery() : IRequest<List<PublicDealerProfileDto>>;

public class GetTrustedDealersQueryHandler : IRequestHandler<GetTrustedDealersQuery, List<PublicDealerProfileDto>>
{
    private readonly IDealerRepository _dealerRepository;

    public GetTrustedDealersQueryHandler(IDealerRepository dealerRepository)
    {
        _dealerRepository = dealerRepository;
    }

    public async Task<List<PublicDealerProfileDto>> Handle(GetTrustedDealersQuery request, CancellationToken cancellationToken)
    {
        var dealers = await _dealerRepository.GetTrustedDealersAsync(cancellationToken);
        
        return dealers.Select(d => new PublicDealerProfileDto(
            d.Id,
            d.BusinessName,
            d.Slogan,
            d.Description,
            d.AboutUs,
            d.LogoUrl,
            d.BannerUrl,
            d.EstablishedDate,
            d.Slug,
            d.City,
            d.Province,
            d.IsTrustedDealer,
            d.IsFoundingMember,
            d.TrustedDealerSince,
            d.AverageRating,
            d.TotalReviews,
            d.TotalSales,
            d.CurrentActiveListings,
            d.Specialties,
            d.SupportedBrands,
            new PublicContactInfo(null, null, d.Website, d.WhatsAppNumber, false, false),
            new List<DealerFeature>(),
            new List<PublicLocationDto>(),
            null,
            null
        )).ToList();
    }
}

// Get Profile Completion
public record GetProfileCompletionQuery(Guid DealerId) : IRequest<ProfileCompletionDto>;

public class GetProfileCompletionQueryHandler : IRequestHandler<GetProfileCompletionQuery, ProfileCompletionDto>
{
    private readonly IDealerRepository _dealerRepository;

    public GetProfileCompletionQueryHandler(IDealerRepository dealerRepository)
    {
        _dealerRepository = dealerRepository;
    }

    public async Task<ProfileCompletionDto> Handle(GetProfileCompletionQuery request, CancellationToken cancellationToken)
    {
        var dealer = await _dealerRepository.GetByIdAsync(request.DealerId, cancellationToken);
        
        if (dealer == null)
            throw new Exception("Dealer not found");

        var completionPercentage = dealer.GetProfileCompletionPercentage();
        var missingFields = new List<string>();
        var completedSections = new List<string>();

        // Check required fields
        if (string.IsNullOrWhiteSpace(dealer.BusinessName)) missingFields.Add("Nombre del Negocio");
        else completedSections.Add("Información Básica");

        if (string.IsNullOrWhiteSpace(dealer.Description)) missingFields.Add("Descripción");
        if (string.IsNullOrWhiteSpace(dealer.AboutUs)) missingFields.Add("Acerca de Nosotros");
        if (string.IsNullOrWhiteSpace(dealer.LogoUrl)) missingFields.Add("Logo");
        if (string.IsNullOrWhiteSpace(dealer.BannerUrl)) missingFields.Add("Banner");

        if (!dealer.Specialties.Any()) missingFields.Add("Especialidades");
        if (!dealer.SupportedBrands.Any()) missingFields.Add("Marcas que Vendes");

        if (!dealer.Locations.Any()) missingFields.Add("Ubicaciones/Sucursales");
        else completedSections.Add("Ubicaciones");

        if (string.IsNullOrWhiteSpace(dealer.FacebookUrl) && 
            string.IsNullOrWhiteSpace(dealer.InstagramUrl))
            missingFields.Add("Redes Sociales");
        else completedSections.Add("Redes Sociales");

        if (!dealer.AcceptsTradeIns && !dealer.OffersFinancing && !dealer.OffersWarranty)
            missingFields.Add("Servicios Ofrecidos");
        else completedSections.Add("Servicios");

        return new ProfileCompletionDto(
            completionPercentage,
            missingFields,
            completedSections
        );
    }
}
