using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Api.Controllers;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using Xunit;

namespace UserService.Tests.Controllers;

/// <summary>
/// Tests for SellerProfileController - Testing SELLER-001 to SELLER-005 processes
/// </summary>
public class SellerProfileControllerTests
{
    private readonly Mock<ISellerProfileRepository> _mockSellerProfileRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly Mock<IVehiclesSaleServiceClient> _mockVehiclesClient;
    private readonly Mock<IReviewServiceClient> _mockReviewClient;
    private readonly Mock<ILogger<SellerProfileController>> _mockLogger;
    private readonly SellerProfileController _controller;

    public SellerProfileControllerTests()
    {
        _mockSellerProfileRepository = new Mock<ISellerProfileRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEventPublisher = new Mock<IEventPublisher>();
        _mockVehiclesClient = new Mock<IVehiclesSaleServiceClient>();
        _mockReviewClient = new Mock<IReviewServiceClient>();
        _mockLogger = new Mock<ILogger<SellerProfileController>>();
        
        _controller = new SellerProfileController(
            _mockSellerProfileRepository.Object,
            _mockUserRepository.Object,
            _mockEventPublisher.Object,
            _mockVehiclesClient.Object,
            _mockReviewClient.Object,
            _mockLogger.Object);
    }

    #region SELLER-001: View Public Profile Tests

    [Fact]
    public async Task GetSellerProfile_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var profile = CreateTestSellerProfile(sellerId);
        var badges = new List<SellerBadgeAssignment>();
        
        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(profile);
        _mockSellerProfileRepository.Setup(r => r.GetBadgesAsync(sellerId))
            .ReturnsAsync(badges);
        _mockSellerProfileRepository.Setup(r => r.UpdateLastActiveAsync(sellerId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.GetSellerProfile(sellerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSellerProfile_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync((SellerProfile?)null);

        // Act
        var result = await _controller.GetSellerProfile(sellerId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetSellerProfile_WithDeletedProfile_ReturnsNotFound()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var profile = CreateTestSellerProfile(sellerId);
        profile.IsDeleted = true;
        
        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetSellerProfile(sellerId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetSellerListings_WithValidSeller_ReturnsListingsResponse()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var profile = CreateTestSellerProfile(sellerId);
        
        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetSellerListings(sellerId, 1, 12, null);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<SellerListingsResponse>();
    }

    [Fact]
    public async Task GetSellerReviews_WithValidSeller_ReturnsReviewsResponse()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var profile = CreateTestSellerProfile(sellerId);
        
        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetSellerReviews(sellerId, 1, 10, null);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<SellerReviewsResponse>();
    }

    #endregion

    #region SELLER-003: Contact Preferences Tests

    [Fact]
    public async Task GetSellerContactPreferences_WithExistingPreferences_ReturnsPreferences()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var profile = CreateTestSellerProfile(sellerId);
        var preferences = CreateTestContactPreferences(sellerId);
        
        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(profile);
        _mockSellerProfileRepository.Setup(r => r.GetContactPreferencesAsync(sellerId))
            .ReturnsAsync(preferences);

        // Act
        var result = await _controller.GetSellerContactPreferences(sellerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSellerContactPreferences_WithNoPreferences_ReturnsDefaultPreferences()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var profile = CreateTestSellerProfile(sellerId);
        
        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(profile);
        _mockSellerProfileRepository.Setup(r => r.GetContactPreferencesAsync(sellerId))
            .ReturnsAsync((ContactPreferences?)null);

        // Act
        var result = await _controller.GetSellerContactPreferences(sellerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSellerContactPreferences_WithInactiveSeller_ReturnsNotFound()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var profile = CreateTestSellerProfile(sellerId);
        profile.IsActive = false;
        
        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetSellerContactPreferences(sellerId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region SELLER-004: Statistics Tests

    [Fact]
    public async Task GetSellerStats_WithValidSeller_ReturnsPublicStats()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var profile = CreateTestSellerProfile(sellerId);
        profile.ResponseRate = 95;
        profile.TotalListings = 50;
        profile.ActiveListings = 12;
        profile.TotalSales = 38;
        
        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetSellerStats(sellerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var stats = okResult!.Value as SellerPublicStatsDto;
        stats!.ResponseRate.Should().Be(95);
        stats.TotalListings.Should().Be(50);
        stats.ActiveListings.Should().Be(12);
        stats.SoldCount.Should().Be(38);
    }

    [Fact]
    public async Task GetSellerStats_WithInactiveSeller_ReturnsNotFound()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var profile = CreateTestSellerProfile(sellerId);
        profile.IsActive = false;
        
        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetSellerStats(sellerId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region SELLER-005: Badges Tests

    [Fact]
    public void SellerBadge_Enum_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<SellerBadge>().Should().Contain(SellerBadge.Verified);
        Enum.GetValues<SellerBadge>().Should().Contain(SellerBadge.TopSeller);
        Enum.GetValues<SellerBadge>().Should().Contain(SellerBadge.FastResponder);
        Enum.GetValues<SellerBadge>().Should().Contain(SellerBadge.TrustedSeller);
        Enum.GetValues<SellerBadge>().Should().Contain(SellerBadge.FounderMember);
    }

    [Fact]
    public void SellerVerificationStatus_Enum_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<SellerVerificationStatus>().Should().Contain(SellerVerificationStatus.NotSubmitted);
        Enum.GetValues<SellerVerificationStatus>().Should().Contain(SellerVerificationStatus.PendingReview);
        Enum.GetValues<SellerVerificationStatus>().Should().Contain(SellerVerificationStatus.Verified);
        Enum.GetValues<SellerVerificationStatus>().Should().Contain(SellerVerificationStatus.Rejected);
    }

    [Fact]
    public void AssignBadgeRequest_ShouldHaveCorrectStructure()
    {
        // Arrange & Act
        var request = new AssignBadgeRequest
        {
            SellerProfileId = Guid.NewGuid(),
            Badge = SellerBadge.FastResponder,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            Reason = "Achieved 95%+ response rate"
        };

        // Assert
        request.Badge.Should().Be(SellerBadge.FastResponder);
        request.Reason.Should().NotBeNullOrEmpty();
        request.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    #endregion

    #region DTO Validation Tests

    [Fact]
    public void SellerPublicProfileDto_ShouldMapCorrectly()
    {
        // Arrange & Act
        var dto = new SellerPublicProfileDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            DisplayName = "Test Seller",
            Type = SellerType.Seller,
            Bio = "Test bio",
            City = "Santo Domingo",
            Province = "Distrito Nacional",
            MemberSince = DateTime.UtcNow.AddMonths(-6),
            IsVerified = true,
            Badges = new List<string> { "Verified", "FastResponder" },
            Stats = new SellerPublicStatsDto()
        };

        // Assert
        dto.DisplayName.Should().Be("Test Seller");
        dto.Type.Should().Be(SellerType.Seller);
        dto.IsVerified.Should().BeTrue();
        dto.Badges.Should().HaveCount(2);
    }

    [Fact]
    public void SellerMyStatsDto_ShouldHaveAllRequiredProperties()
    {
        // Arrange & Act
        var stats = new SellerMyStatsDto
        {
            SellerId = Guid.NewGuid(),
            TotalListings = 50,
            ActiveListings = 12,
            PendingListings = 2,
            SoldListings = 38,
            ExpiredListings = 0,
            TotalViews = 10000,
            ViewsThisMonth = 2500,
            ViewsChange = 15,
            TotalInquiries = 300,
            InquiriesThisMonth = 75,
            UnrespondedInquiries = 3,
            TotalValue = 5000000m,
            AveragePrice = 100000m,
            AverageRating = 4.8,
            ReviewCount = 45,
            ResponseTimeMinutes = 120,
            ResponseRate = 98,
            MaxActiveListings = 50,
            RemainingListings = 38,
            CanSellHighValue = true,
            Badges = new List<SellerBadgeDto>(),
            VerificationStatus = SellerVerificationStatus.Verified
        };

        // Assert
        stats.TotalListings.Should().Be(50);
        stats.ActiveListings.Should().Be(12);
        stats.ViewsThisMonth.Should().Be(2500);
        stats.ResponseRate.Should().Be(98);
        stats.VerificationStatus.Should().Be(SellerVerificationStatus.Verified);
    }

    [Fact]
    public void ContactPreferencesDto_ShouldHaveAllChannelOptions()
    {
        // Arrange & Act
        var dto = new ContactPreferencesDto
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            AllowPhoneCalls = true,
            AllowWhatsApp = true,
            AllowEmail = false,
            AllowInAppChat = true,
            ContactHoursStart = "09:00",
            ContactHoursEnd = "18:00",
            ContactDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" },
            ShowPhoneNumber = false,
            ShowWhatsAppNumber = true,
            ShowEmail = false,
            PreferredContactMethod = "WhatsApp"
        };

        // Assert
        dto.AllowWhatsApp.Should().BeTrue();
        dto.AllowEmail.Should().BeFalse();
        dto.ContactDays.Should().HaveCount(5);
        dto.PreferredContactMethod.Should().Be("WhatsApp");
    }

    [Fact]
    public void UpdateContactPreferencesRequest_ShouldAcceptPartialUpdates()
    {
        // Arrange & Act
        var request = new UpdateContactPreferencesRequest
        {
            AllowWhatsApp = true,
            ShowPhoneNumber = false
        };

        // Assert - All other properties should be null (optional)
        request.AllowWhatsApp.Should().BeTrue();
        request.ShowPhoneNumber.Should().BeFalse();
        request.AllowPhoneCalls.Should().BeNull();
        request.ContactDays.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private static SellerProfile CreateTestSellerProfile(Guid sellerId)
    {
        return new SellerProfile
        {
            Id = sellerId,
            UserId = Guid.NewGuid(),
            SellerType = SellerType.Seller,
            DisplayName = "Test Seller",
            FullName = "Test Seller Full Name",
            Bio = "Test seller bio",
            City = "Santo Domingo",
            State = "Distrito Nacional",
            Country = "DO",
            Phone = "809-555-1234",
            Email = "test@seller.com",
            Address = "Test Address",
            IsActive = true,
            VerificationStatus = SellerVerificationStatus.NotSubmitted,
            ResponseRate = 90,
            ViewsThisMonth = 1000,
            LeadsThisMonth = 50,
            TotalListings = 25,
            ActiveListings = 10,
            TotalSales = 15,
            AverageRating = 4.5m,
            TotalReviews = 20,
            ResponseTimeMinutes = 60,
            MaxActiveListings = 50,
            CanSellHighValue = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };
    }

    private static ContactPreferences CreateTestContactPreferences(Guid sellerId)
    {
        return new ContactPreferences
        {
            Id = Guid.NewGuid(),
            SellerProfileId = sellerId,
            AllowPhoneCalls = true,
            AllowWhatsApp = true,
            AllowInAppChat = true,
            AllowEmail = true,
            ShowPhoneNumber = false,
            ShowWhatsAppNumber = true,
            ShowEmail = false,
            ContactHoursStart = TimeSpan.FromHours(9),
            ContactHoursEnd = TimeSpan.FromHours(18),
            ContactDays = "Monday,Tuesday,Wednesday,Thursday,Friday",
            RequireVerifiedBuyers = false,
            BlockAnonymousContacts = false
        };
    }

    #endregion
}
