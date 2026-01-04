using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Application.UseCases.Sellers.UpdateSellerProfile;

public record UpdateSellerProfileCommand(Guid SellerId, UpdateSellerProfileRequest Request) : IRequest<SellerProfileDto>;

public class UpdateSellerProfileCommandHandler : IRequestHandler<UpdateSellerProfileCommand, SellerProfileDto>
{
    private readonly ISellerProfileRepository _sellerProfileRepository;
    private readonly ILogger<UpdateSellerProfileCommandHandler> _logger;

    public UpdateSellerProfileCommandHandler(
        ISellerProfileRepository sellerProfileRepository,
        ILogger<UpdateSellerProfileCommandHandler> logger)
    {
        _sellerProfileRepository = sellerProfileRepository;
        _logger = logger;
    }

    public async Task<SellerProfileDto> Handle(UpdateSellerProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = await _sellerProfileRepository.GetByIdAsync(command.SellerId);
        if (profile == null)
        {
            throw new NotFoundException($"Seller profile {command.SellerId} not found");
        }

        var request = command.Request;

        // Update only provided fields
        if (request.FullName != null) profile.FullName = request.FullName;
        if (request.Bio != null) profile.Bio = request.Bio;
        if (request.DateOfBirth.HasValue) profile.DateOfBirth = request.DateOfBirth;
        if (request.Nationality != null) profile.Nationality = request.Nationality;
        if (request.AvatarUrl != null) profile.AvatarUrl = request.AvatarUrl;

        // Contact
        if (request.Phone != null) profile.Phone = request.Phone;
        if (request.AlternatePhone != null) profile.AlternatePhone = request.AlternatePhone;
        if (request.WhatsApp != null) profile.WhatsApp = request.WhatsApp;

        // Location
        if (request.Address != null) profile.Address = request.Address;
        if (request.City != null) profile.City = request.City;
        if (request.State != null) profile.State = request.State;
        if (request.ZipCode != null) profile.ZipCode = request.ZipCode;
        if (request.Country != null) profile.Country = request.Country;
        if (request.Latitude.HasValue) profile.Latitude = request.Latitude.Value;
        if (request.Longitude.HasValue) profile.Longitude = request.Longitude.Value;

        // Preferences
        if (request.AcceptsOffers.HasValue) profile.AcceptsOffers = request.AcceptsOffers.Value;
        if (request.ShowPhone.HasValue) profile.ShowPhone = request.ShowPhone.Value;
        if (request.ShowLocation.HasValue) profile.ShowLocation = request.ShowLocation.Value;
        if (request.PreferredContactMethod != null) profile.PreferredContactMethod = request.PreferredContactMethod;
        if (request.IsActive.HasValue) profile.IsActive = request.IsActive.Value;

        profile.UpdatedAt = DateTime.UtcNow;

        await _sellerProfileRepository.UpdateAsync(profile);

        _logger.LogInformation("Updated seller profile {ProfileId}", profile.Id);

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
            UpdatedAt = profile.UpdatedAt
        };
    }
}
