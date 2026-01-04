using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.Dealers.CreateDealer;

public record CreateDealerCommand(CreateDealerRequest Request) : IRequest<DealerDto>;

public class CreateDealerCommandHandler : IRequestHandler<CreateDealerCommand, DealerDto>
{
    private readonly IDealerRepository _dealerRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CreateDealerCommandHandler> _logger;

    public CreateDealerCommandHandler(
        IDealerRepository dealerRepository,
        IUserRepository userRepository,
        ILogger<CreateDealerCommandHandler> logger)
    {
        _dealerRepository = dealerRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<DealerDto> Handle(CreateDealerCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Verify owner exists
        var owner = await _userRepository.GetByIdAsync(request.OwnerUserId);
        if (owner == null)
        {
            throw new InvalidOperationException($"User {request.OwnerUserId} not found");
        }

        // Check if user already has a dealer
        var existingDealer = await _dealerRepository.GetByOwnerIdAsync(request.OwnerUserId);
        if (existingDealer != null)
        {
            throw new InvalidOperationException("User already has a dealer account");
        }

        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            OwnerUserId = request.OwnerUserId,
            BusinessName = request.BusinessName,
            TradeName = request.TradeName,
            Description = request.Description,
            DealerType = request.DealerType,
            
            // Contact
            Email = request.Email,
            Phone = request.Phone,
            WhatsApp = request.WhatsApp,
            Website = request.Website,
            
            // Location
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            Country = request.Country,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            
            // Legal
            BusinessRegistrationNumber = request.BusinessRegistrationNumber,
            TaxId = request.TaxId,
            DealerLicenseNumber = request.DealerLicenseNumber,
            LicenseExpiryDate = request.LicenseExpiryDate,
            
            // Branding
            LogoUrl = request.LogoUrl,
            BannerUrl = request.BannerUrl,
            
            // Defaults
            VerificationStatus = DealerVerificationStatus.Pending,
            IsActive = true,
            AverageRating = 0,
            TotalReviews = 0,
            TotalListings = 0,
            ActiveListings = 0,
            TotalSales = 0,
            MaxListings = 10,
            CreatedAt = DateTime.UtcNow
        };

        await _dealerRepository.AddAsync(dealer);

        _logger.LogInformation("Created dealer {DealerId} for user {UserId}", dealer.Id, request.OwnerUserId);

        return MapToDto(dealer);
    }

    private static DealerDto MapToDto(Dealer dealer)
    {
        return new DealerDto
        {
            Id = dealer.Id,
            OwnerUserId = dealer.OwnerUserId,
            BusinessName = dealer.BusinessName,
            TradeName = dealer.TradeName,
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
            BusinessRegistrationNumber = dealer.BusinessRegistrationNumber,
            TaxId = dealer.TaxId,
            DealerLicenseNumber = dealer.DealerLicenseNumber,
            LicenseExpiryDate = dealer.LicenseExpiryDate,
            VerificationStatus = dealer.VerificationStatus,
            VerifiedAt = dealer.VerifiedAt,
            RejectionReason = dealer.RejectionReason,
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
            UpdatedAt = dealer.UpdatedAt
        };
    }
}
