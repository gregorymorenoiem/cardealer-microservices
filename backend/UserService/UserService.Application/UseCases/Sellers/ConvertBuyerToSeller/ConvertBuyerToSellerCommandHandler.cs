using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Metrics;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using CarDealer.Contracts.Events.Seller;

namespace UserService.Application.UseCases.Sellers.ConvertBuyerToSeller;

public class ConvertBuyerToSellerCommandHandler
    : IRequestHandler<ConvertBuyerToSellerCommand, SellerConversionResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ISellerProfileRepository _sellerProfileRepository;
    private readonly ISellerConversionRepository _conversionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IAuditServiceClient _auditClient;
    private readonly UserServiceMetrics _metrics;
    private readonly ILogger<ConvertBuyerToSellerCommandHandler> _logger;

    public ConvertBuyerToSellerCommandHandler(
        IUserRepository userRepository,
        ISellerProfileRepository sellerProfileRepository,
        ISellerConversionRepository conversionRepository,
        IEventPublisher eventPublisher,
        IAuditServiceClient auditClient,
        UserServiceMetrics metrics,
        ILogger<ConvertBuyerToSellerCommandHandler> logger)
    {
        _userRepository = userRepository;
        _sellerProfileRepository = sellerProfileRepository;
        _conversionRepository = conversionRepository;
        _eventPublisher = eventPublisher;
        _auditClient = auditClient;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<SellerConversionResultDto> Handle(
        ConvertBuyerToSellerCommand command,
        CancellationToken cancellationToken)
    {
        _metrics.RecordSellerConversionRequested();

        // ──────────────────────────────────────────────────────────
        // 1. Idempotency check — if same key was already processed, return stored result
        // ──────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            var existingConversion = await _conversionRepository.GetByIdempotencyKeyAsync(command.IdempotencyKey);
            if (existingConversion != null)
            {
                _logger.LogInformation(
                    "Idempotent request detected for user {UserId} with key {IdempotencyKey}",
                    command.UserId, command.IdempotencyKey);

                var existingProfile = await _sellerProfileRepository.GetByIdAsync(existingConversion.SellerProfileId);
                return MapToResult(existingConversion, existingProfile);
            }
        }

        // ──────────────────────────────────────────────────────────
        // 2. Fetch and validate user
        // ──────────────────────────────────────────────────────────
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User {command.UserId} not found");
        }

        // ──────────────────────────────────────────────────────────
        // 3. REJECT Dealer → Seller conversion (CONVERSION_NOT_ALLOWED)
        // ──────────────────────────────────────────────────────────
        if (user.AccountType == AccountType.Dealer || user.AccountType == AccountType.DealerEmployee)
        {
            _metrics.RecordSellerConversionFailed("dealer_rejected");
            _logger.LogWarning(
                "User {UserId} attempted buyer-to-dealer conversion (AccountType={AccountType}). Rejected.",
                command.UserId, user.AccountType);

            throw new InvalidOperationException("CONVERSION_NOT_ALLOWED");
        }

        // ──────────────────────────────────────────────────────────
        // 4. Already a seller? Return existing profile (idempotent behavior)
        // ──────────────────────────────────────────────────────────
        var existingSellerProfile = await _sellerProfileRepository.GetByUserIdAsync(command.UserId);
        if (existingSellerProfile != null)
        {
            _logger.LogInformation(
                "User {UserId} already has seller profile {SellerProfileId}. Returning existing.",
                command.UserId, existingSellerProfile.Id);

            var existingConv = await _conversionRepository.GetByUserIdAsync(command.UserId);
            if (existingConv != null)
            {
                return MapToResult(existingConv, existingSellerProfile);
            }

            // Seller profile exists but no conversion record — create one for tracking
            return new SellerConversionResultDto
            {
                ConversionId = Guid.Empty,
                UserId = command.UserId,
                SellerProfileId = existingSellerProfile.Id,
                Source = "existing",
                Status = "Approved",
                PreviousAccountType = user.AccountType.ToString(),
                NewAccountType = AccountType.Seller.ToString(),
                PendingVerification = existingSellerProfile.VerificationStatus != SellerVerificationStatus.Verified,
                RequestedAt = existingSellerProfile.CreatedAt,
                CompletedAt = existingSellerProfile.CreatedAt,
                SellerProfile = MapProfileToDto(existingSellerProfile)
            };
        }

        // ──────────────────────────────────────────────────────────
        // 5. Create SellerProfile from user data
        // ──────────────────────────────────────────────────────────
        var request = command.Request;
        var sellerProfile = new SellerProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            Address = user.BusinessAddress ?? string.Empty,
            City = user.City ?? string.Empty,
            State = user.Province ?? string.Empty,
            Country = "DO",
            Bio = request.Bio,
            VerificationStatus = SellerVerificationStatus.PendingReview,
            IsActive = true,
            AcceptsOffers = request.AcceptsOffers,
            ShowPhone = request.ShowPhone,
            ShowLocation = request.ShowLocation,
            PreferredContactMethod = request.PreferredContactMethod,
            SellerType = SellerType.Seller,
            DisplayName = user.FullName,
            MaxActiveListings = 3,
            CanSellHighValue = false,
            CreatedAt = DateTime.UtcNow
        };

        await _sellerProfileRepository.AddAsync(sellerProfile);

        // ──────────────────────────────────────────────────────────
        // 6. Create conversion tracking record
        // ──────────────────────────────────────────────────────────
        var previousAccountType = user.AccountType;
        var conversion = new SellerConversion
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SellerProfileId = sellerProfile.Id,
            Source = "conversion",
            PreviousAccountType = previousAccountType,
            NewAccountType = AccountType.Seller,
            Status = SellerConversionStatus.Approved,
            IdempotencyKey = command.IdempotencyKey,
            IpAddress = command.IpAddress,
            UserAgent = command.UserAgent,
            RequestedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        await _conversionRepository.CreateAsync(conversion);

        // ──────────────────────────────────────────────────────────
        // 7. Update user account type
        // ──────────────────────────────────────────────────────────
        user.AccountType = AccountType.Seller;
        user.UserIntent = UserIntent.Sell;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // ──────────────────────────────────────────────────────────
        // 8. Publish domain events (NO raw PII in events)
        // ──────────────────────────────────────────────────────────
        try
        {
            var conversionEvent = SellerConversionRequestedEvent.Create(
                userId: user.Id,
                conversionId: conversion.Id,
                sellerProfileId: sellerProfile.Id,
                source: "conversion",
                previousAccountType: previousAccountType.ToString());

            await _eventPublisher.PublishAsync(conversionEvent, cancellationToken);

            var createdEvent = SellerCreatedEvent.Create(
                userId: user.Id,
                sellerProfileId: sellerProfile.Id,
                source: "conversion");

            await _eventPublisher.PublishAsync(createdEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            // Events are best-effort — don't fail the conversion
            _logger.LogWarning(ex, "Failed to publish seller conversion events for user {UserId}", user.Id);
        }

        // ──────────────────────────────────────────────────────────
        // 9. Audit log
        // ──────────────────────────────────────────────────────────
        try
        {
            await _auditClient.LogSellerConversionAsync(
                user.Id, sellerProfile.Id, previousAccountType.ToString(), "system");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log audit for seller conversion of user {UserId}", user.Id);
        }

        // ──────────────────────────────────────────────────────────
        // 10. Metrics
        // ──────────────────────────────────────────────────────────
        _metrics.RecordSellerConversionApproved();

        _logger.LogInformation(
            "User {UserId} converted from {PreviousAccountType} to Seller. SellerProfileId={SellerProfileId}, ConversionId={ConversionId}",
            user.Id, previousAccountType, sellerProfile.Id, conversion.Id);

        return MapToResult(conversion, sellerProfile);
    }

    private static SellerConversionResultDto MapToResult(SellerConversion conversion, SellerProfile? profile)
    {
        return new SellerConversionResultDto
        {
            ConversionId = conversion.Id,
            UserId = conversion.UserId,
            SellerProfileId = conversion.SellerProfileId,
            Source = conversion.Source,
            Status = conversion.Status.ToString(),
            PreviousAccountType = conversion.PreviousAccountType.ToString(),
            NewAccountType = conversion.NewAccountType.ToString(),
            PendingVerification = profile?.VerificationStatus != SellerVerificationStatus.Verified,
            RequestedAt = conversion.RequestedAt,
            CompletedAt = conversion.CompletedAt,
            SellerProfile = profile != null ? MapProfileToDto(profile) : null
        };
    }

    private static SellerProfileDto MapProfileToDto(SellerProfile profile)
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
            UpdatedAt = profile.UpdatedAt
        };
    }
}
