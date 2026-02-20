using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Metrics;
using UserService.Application.UseCases.Sellers.ConvertBuyerToSeller;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using Xunit;

namespace UserService.Tests.Application;

public class ConvertBuyerToSellerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ISellerProfileRepository> _sellerProfileRepositoryMock;
    private readonly Mock<ISellerConversionRepository> _conversionRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<IAuditServiceClient> _auditClientMock;
    private readonly UserServiceMetrics _metrics;
    private readonly Mock<ILogger<ConvertBuyerToSellerCommandHandler>> _loggerMock;
    private readonly ConvertBuyerToSellerCommandHandler _handler;

    public ConvertBuyerToSellerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _sellerProfileRepositoryMock = new Mock<ISellerProfileRepository>();
        _conversionRepositoryMock = new Mock<ISellerConversionRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _auditClientMock = new Mock<IAuditServiceClient>();
        _metrics = new UserServiceMetrics();
        _loggerMock = new Mock<ILogger<ConvertBuyerToSellerCommandHandler>>();

        _handler = new ConvertBuyerToSellerCommandHandler(
            _userRepositoryMock.Object,
            _sellerProfileRepositoryMock.Object,
            _conversionRepositoryMock.Object,
            _eventPublisherMock.Object,
            _auditClientMock.Object,
            _metrics,
            _loggerMock.Object);
    }

    private static User CreateBuyerUser(Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Email = "buyer@test.com",
            FirstName = "Test",
            LastName = "Buyer",
            PhoneNumber = "8091234567",
            AccountType = AccountType.Buyer,
            IsActive = true,
            City = "Santo Domingo",
            Province = "Distrito Nacional"
        };
    }

    private static ConvertBuyerToSellerRequest CreateValidRequest()
    {
        return new ConvertBuyerToSellerRequest
        {
            AcceptTerms = true,
            PreferredContactMethod = "whatsapp",
            AcceptsOffers = true,
            ShowPhone = true,
            ShowLocation = true,
            Bio = "Vendedor de confianza"
        };
    }

    // ─────────────────────────────────────────────────────────────
    // TEST 1: Buyer → Seller SUCCESS
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_BuyerToSeller_Success_CreatesProfileAndReturnsResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateBuyerUser(userId);
        var request = CreateValidRequest();

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _sellerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((SellerProfile?)null);
        _sellerProfileRepositoryMock.Setup(r => r.AddAsync(It.IsAny<SellerProfile>()))
            .ReturnsAsync((SellerProfile p) => p);
        _conversionRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<SellerConversion>()))
            .ReturnsAsync((SellerConversion c) => c);

        var command = new ConvertBuyerToSellerCommand(userId, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Source.Should().Be("conversion");
        result.Status.Should().Be("Approved");
        result.PreviousAccountType.Should().Be("Buyer");
        result.NewAccountType.Should().Be("Seller");
        result.SellerProfile.Should().NotBeNull();
        result.SellerProfile!.FullName.Should().Be("Test Buyer");
        result.SellerProfile.Email.Should().Be("buyer@test.com");

        // Verify DB row created
        _sellerProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SellerProfile>()), Times.Once);
        _conversionRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<SellerConversion>()), Times.Once);

        // Verify user account type updated
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            u.AccountType == AccountType.Seller && u.UserIntent == UserIntent.Sell)), Times.Once);

        // Verify events published
        _eventPublisherMock.Verify(p => p.PublishAsync(
            It.IsAny<CarDealer.Contracts.Events.Seller.SellerConversionRequestedEvent>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _eventPublisherMock.Verify(p => p.PublishAsync(
            It.IsAny<CarDealer.Contracts.Events.Seller.SellerCreatedEvent>(),
            It.IsAny<CancellationToken>()), Times.Once);

        // Verify audit call
        _auditClientMock.Verify(a => a.LogSellerConversionAsync(
            userId, It.IsAny<Guid>(), "Buyer", "system"), Times.Once);
    }

    // ─────────────────────────────────────────────────────────────
    // TEST 2: Already a seller — idempotent, returns existing
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_AlreadySeller_ReturnsExistingProfile_Idempotent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateBuyerUser(userId);
        user.AccountType = AccountType.Seller;

        var existingProfile = new SellerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = "Test Buyer",
            Email = "buyer@test.com",
            Phone = "8091234567",
            Address = string.Empty,
            City = "Santo Domingo",
            State = "DN",
            Country = "DO",
            VerificationStatus = SellerVerificationStatus.PendingReview,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var existingConversion = new SellerConversion
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SellerProfileId = existingProfile.Id,
            Source = "conversion",
            PreviousAccountType = AccountType.Buyer,
            NewAccountType = AccountType.Seller,
            Status = SellerConversionStatus.Approved,
            RequestedAt = DateTime.UtcNow.AddDays(-5),
            CompletedAt = DateTime.UtcNow.AddDays(-5)
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _sellerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existingProfile);
        _conversionRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existingConversion);

        var command = new ConvertBuyerToSellerCommand(userId, CreateValidRequest());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.SellerProfileId.Should().Be(existingProfile.Id);
        result.ConversionId.Should().Be(existingConversion.Id);
        result.Source.Should().Be("conversion");

        // Verify NO new profile or conversion was created
        _sellerProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SellerProfile>()), Times.Never);
        _conversionRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<SellerConversion>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    // TEST 3: Dealer → Seller REJECTED (CONVERSION_NOT_ALLOWED)
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DealerToSeller_ThrowsConversionNotAllowed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateBuyerUser(userId);
        user.AccountType = AccountType.Dealer;

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var command = new ConvertBuyerToSellerCommand(userId, CreateValidRequest());

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CONVERSION_NOT_ALLOWED");

        // Verify nothing was created
        _sellerProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SellerProfile>()), Times.Never);
        _conversionRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<SellerConversion>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DealerEmployeeToSeller_ThrowsConversionNotAllowed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateBuyerUser(userId);
        user.AccountType = AccountType.DealerEmployee;

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var command = new ConvertBuyerToSellerCommand(userId, CreateValidRequest());

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CONVERSION_NOT_ALLOWED");
    }

    // ─────────────────────────────────────────────────────────────
    // TEST 4: Idempotency key duplicate returns same response
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DuplicateIdempotencyKey_ReturnsSameResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sellerProfileId = Guid.NewGuid();
        var conversionId = Guid.NewGuid();
        var idempotencyKey = "idem-key-123";

        var existingConversion = new SellerConversion
        {
            Id = conversionId,
            UserId = userId,
            SellerProfileId = sellerProfileId,
            Source = "conversion",
            PreviousAccountType = AccountType.Buyer,
            NewAccountType = AccountType.Seller,
            Status = SellerConversionStatus.Approved,
            IdempotencyKey = idempotencyKey,
            RequestedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var existingProfile = new SellerProfile
        {
            Id = sellerProfileId,
            UserId = userId,
            FullName = "Test Buyer",
            Email = "buyer@test.com",
            Phone = "8091234567",
            Address = string.Empty,
            City = "Santo Domingo",
            State = "DN",
            Country = "DO",
            VerificationStatus = SellerVerificationStatus.Verified,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _conversionRepositoryMock.Setup(r => r.GetByIdempotencyKeyAsync(idempotencyKey))
            .ReturnsAsync(existingConversion);
        _sellerProfileRepositoryMock.Setup(r => r.GetByIdAsync(sellerProfileId))
            .ReturnsAsync(existingProfile);

        var command = new ConvertBuyerToSellerCommand(userId, CreateValidRequest(), idempotencyKey);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConversionId.Should().Be(conversionId);
        result.SellerProfileId.Should().Be(sellerProfileId);
        result.Status.Should().Be("Approved");

        // Verify no new records created
        _sellerProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SellerProfile>()), Times.Never);
        _conversionRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<SellerConversion>()), Times.Never);
        _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    // TEST 5: User not found
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var command = new ConvertBuyerToSellerCommand(userId, CreateValidRequest());

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ─────────────────────────────────────────────────────────────
    // TEST 6: Guest user can convert to seller
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_GuestToSeller_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateBuyerUser(userId);
        user.AccountType = AccountType.Guest;

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _sellerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((SellerProfile?)null);
        _sellerProfileRepositoryMock.Setup(r => r.AddAsync(It.IsAny<SellerProfile>()))
            .ReturnsAsync((SellerProfile p) => p);
        _conversionRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<SellerConversion>()))
            .ReturnsAsync((SellerConversion c) => c);

        var command = new ConvertBuyerToSellerCommand(userId, CreateValidRequest());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PreviousAccountType.Should().Be("Guest");
        result.NewAccountType.Should().Be("Seller");
        result.Source.Should().Be("conversion");
    }

    // ─────────────────────────────────────────────────────────────
    // TEST 7: Event publish failure does not break conversion
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_EventPublishFails_ConversionStillSucceeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateBuyerUser(userId);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _sellerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((SellerProfile?)null);
        _sellerProfileRepositoryMock.Setup(r => r.AddAsync(It.IsAny<SellerProfile>()))
            .ReturnsAsync((SellerProfile p) => p);
        _conversionRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<SellerConversion>()))
            .ReturnsAsync((SellerConversion c) => c);

        // Simulate event publish failure — generic method throws for any event type
        _eventPublisherMock.Setup(p => p.PublishAsync(
            It.IsAny<CarDealer.Contracts.Events.Seller.SellerConversionRequestedEvent>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("RabbitMQ connection lost"));

        var command = new ConvertBuyerToSellerCommand(userId, CreateValidRequest());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — conversion should still succeed
        result.Should().NotBeNull();
        result.Status.Should().Be("Approved");
        _sellerProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SellerProfile>()), Times.Once);
    }
}
