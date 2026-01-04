using MediatR;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Application.UseCases.Sellers.GetSellerProfile;

public record GetSellerProfileQuery(Guid SellerId) : IRequest<SellerProfileDto>;

public class GetSellerProfileQueryHandler : IRequestHandler<GetSellerProfileQuery, SellerProfileDto>
{
    private readonly ISellerProfileRepository _sellerProfileRepository;

    public GetSellerProfileQueryHandler(ISellerProfileRepository sellerProfileRepository)
    {
        _sellerProfileRepository = sellerProfileRepository;
    }

    public async Task<SellerProfileDto> Handle(GetSellerProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _sellerProfileRepository.GetByIdAsync(request.SellerId);
        if (profile == null)
        {
            throw new NotFoundException($"Seller profile {request.SellerId} not found");
        }

        return MapToDto(profile);
    }

    private static SellerProfileDto MapToDto(SellerProfile profile)
    {
        return new SellerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullName = profile.FullName,
            DateOfBirth = profile.DateOfBirth,
            Nationality = profile.Nationality,
            Bio = profile.Bio,
            AvatarUrl = profile.AvatarUrl,
            Phone = profile.Phone,
            AlternatePhone = profile.AlternatePhone,
            WhatsApp = profile.WhatsApp,
            Email = profile.Email,
            Address = profile.Address,
            City = profile.City,
            State = profile.State,
            ZipCode = profile.ZipCode,
            Country = profile.Country,
            Latitude = profile.Latitude,
            Longitude = profile.Longitude,
            VerificationStatus = profile.VerificationStatus,
            VerifiedAt = profile.VerifiedAt,
            RejectionReason = profile.RejectionReason,
            TotalListings = profile.TotalListings,
            ActiveListings = profile.ActiveListings,
            TotalSales = profile.TotalSales,
            AverageRating = profile.AverageRating,
            TotalReviews = profile.TotalReviews,
            ResponseTimeMinutes = profile.ResponseTimeMinutes,
            IsActive = profile.IsActive,
            AcceptsOffers = profile.AcceptsOffers,
            ShowPhone = profile.ShowPhone,
            ShowLocation = profile.ShowLocation,
            PreferredContactMethod = profile.PreferredContactMethod,
            MaxActiveListings = profile.MaxActiveListings,
            CanSellHighValue = profile.CanSellHighValue,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt,
            IdentityDocuments = profile.IdentityDocuments?.Select(d => new IdentityDocumentDto
            {
                Id = d.Id,
                SellerProfileId = d.SellerProfileId,
                DocumentType = d.DocumentType,
                DocumentNumber = d.DocumentNumber,
                IssuingCountry = d.IssuingCountry,
                IssueDate = d.IssueDate,
                ExpiryDate = d.ExpiryDate,
                Status = d.Status,
                VerifiedAt = d.VerifiedAt,
                UploadedAt = d.UploadedAt
            }).ToList()
        };
    }
}

public record GetSellerProfileByUserQuery(Guid UserId) : IRequest<SellerProfileDto?>;

public class GetSellerProfileByUserQueryHandler : IRequestHandler<GetSellerProfileByUserQuery, SellerProfileDto?>
{
    private readonly ISellerProfileRepository _sellerProfileRepository;

    public GetSellerProfileByUserQueryHandler(ISellerProfileRepository sellerProfileRepository)
    {
        _sellerProfileRepository = sellerProfileRepository;
    }

    public async Task<SellerProfileDto?> Handle(GetSellerProfileByUserQuery request, CancellationToken cancellationToken)
    {
        var profile = await _sellerProfileRepository.GetByUserIdAsync(request.UserId);
        if (profile == null)
        {
            return null;
        }

        return new SellerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullName = profile.FullName,
            DateOfBirth = profile.DateOfBirth,
            Nationality = profile.Nationality,
            Bio = profile.Bio,
            AvatarUrl = profile.AvatarUrl,
            Phone = profile.Phone,
            AlternatePhone = profile.AlternatePhone,
            WhatsApp = profile.WhatsApp,
            Email = profile.Email,
            Address = profile.Address,
            City = profile.City,
            State = profile.State,
            ZipCode = profile.ZipCode,
            Country = profile.Country,
            Latitude = profile.Latitude,
            Longitude = profile.Longitude,
            VerificationStatus = profile.VerificationStatus,
            AverageRating = profile.AverageRating,
            TotalReviews = profile.TotalReviews,
            ActiveListings = profile.ActiveListings,
            TotalSales = profile.TotalSales,
            IsActive = profile.IsActive,
            ShowPhone = profile.ShowPhone,
            ShowLocation = profile.ShowLocation,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }
}
