using MediatR;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Application.UseCases.Dealers.GetDealer;

public record GetDealerQuery(Guid DealerId) : IRequest<DealerDto>;

public class GetDealerQueryHandler : IRequestHandler<GetDealerQuery, DealerDto>
{
    private readonly IDealerRepository _dealerRepository;

    public GetDealerQueryHandler(IDealerRepository dealerRepository)
    {
        _dealerRepository = dealerRepository;
    }

    public async Task<DealerDto> Handle(GetDealerQuery request, CancellationToken cancellationToken)
    {
        var dealer = await _dealerRepository.GetByIdAsync(request.DealerId);
        if (dealer == null)
        {
            throw new NotFoundException($"Dealer {request.DealerId} not found");
        }

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
            UpdatedAt = dealer.UpdatedAt,
            Employees = dealer.Employees?.Select(e => new DealerEmployeeDto
            {
                Id = e.Id,
                UserId = e.UserId,
                DealerId = e.DealerId,
                Role = e.DealerRole.ToString(),
                Status = e.Status.ToString(),
                Permissions = e.Permissions,
                InvitationDate = e.InvitationDate,
                ActivationDate = e.ActivationDate
            }).ToList()
        };
    }
}

public record GetDealerByOwnerQuery(Guid OwnerUserId) : IRequest<DealerDto?>;

public class GetDealerByOwnerQueryHandler : IRequestHandler<GetDealerByOwnerQuery, DealerDto?>
{
    private readonly IDealerRepository _dealerRepository;

    public GetDealerByOwnerQueryHandler(IDealerRepository dealerRepository)
    {
        _dealerRepository = dealerRepository;
    }

    public async Task<DealerDto?> Handle(GetDealerByOwnerQuery request, CancellationToken cancellationToken)
    {
        var dealer = await _dealerRepository.GetByOwnerIdAsync(request.OwnerUserId);
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
            VerificationStatus = dealer.VerificationStatus,
            AverageRating = dealer.AverageRating,
            TotalReviews = dealer.TotalReviews,
            ActiveListings = dealer.ActiveListings,
            IsActive = dealer.IsActive,
            CreatedAt = dealer.CreatedAt,
            UpdatedAt = dealer.UpdatedAt
        };
    }
}
