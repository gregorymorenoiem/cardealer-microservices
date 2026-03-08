using Xunit;
using Moq;
using FluentAssertions;
using AuthService.Application.Features.Auth.Commands.Register;
using AuthService.Application.DTOs.Auth;
using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using AuthService.Shared.Exceptions;

namespace AuthService.Tests.Unit.Handlers.Auth;

/// <summary>
/// Sprint 5 – Unit tests for RegisterCommandHandler.
/// Covers: successful registration, duplicate email, token generation,
/// verification email, event publishing, AccountType mapping.
/// </summary>
public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtGenerator> _jwtGeneratorMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IVerificationTokenRepository> _verificationTokenRepositoryMock;
    private readonly Mock<IAuthNotificationService> _notificationServiceMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<IRequestContext> _requestContextMock;

    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtGeneratorMock = new Mock<IJwtGenerator>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _verificationTokenRepositoryMock = new Mock<IVerificationTokenRepository>();
        _notificationServiceMock = new Mock<IAuthNotificationService>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _requestContextMock = new Mock<IRequestContext>();

        // Default request context
        _requestContextMock.Setup(x => x.IpAddress).Returns("127.0.0.1");

        // Default password hasher
        _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed-password-123");

        // Default JWT generator
        _jwtGeneratorMock.Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>())).Returns("access-token-123");
        _jwtGeneratorMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token-123");

        // Default: no existing user
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        _handler = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtGeneratorMock.Object,
            _refreshTokenRepositoryMock.Object,
            _verificationTokenRepositoryMock.Object,
            _notificationServiceMock.Object,
            _eventPublisherMock.Object,
            _requestContextMock.Object
        );
    }

    #region Helpers

    private static RegisterCommand CreateValidCommand(
        string email = "newuser@example.com",
        string password = "StrongPassword123!",
        string? firstName = "John",
        string? lastName = "Doe",
        AccountType accountType = AccountType.Buyer)
    {
        return new RegisterCommand(
            UserName: null,
            Email: email,
            Password: password,
            FirstName: firstName,
            LastName: lastName,
            Phone: "+18091234567",
            AcceptTerms: true,
            AccountType: accountType
        );
    }

    #endregion

    #region Successful Registration

    [Fact]
    public async Task Handle_ValidCommand_ReturnsRegisterResponse()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("newuser@example.com");
        result.AccessToken.Should().Be("access-token-123");
        result.RefreshToken.Should().Be("refresh-token-123");
    }

    [Fact]
    public async Task Handle_ValidCommand_HashesPassword()
    {
        // Arrange
        var command = CreateValidCommand(password: "MySecurePassword123!");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordHasherMock.Verify(x => x.Hash("MySecurePassword123!"), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesUserToRepository()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<ApplicationUser>(u =>
                    u.Email == "newuser@example.com" &&
                    u.FirstName == "John" &&
                    u.LastName == "Doe"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesRefreshToken()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Domain.Entities.RefreshToken>(rt => rt.Token == "refresh-token-123"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesVerificationToken()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _verificationTokenRepositoryMock.Verify(
            x => x.AddAsync(It.Is<VerificationToken>(vt =>
                vt.Type == VerificationTokenType.EmailVerification)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SendsEmailConfirmation()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(
            x => x.SendEmailConfirmationAsync("newuser@example.com", It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesUserRegisteredEvent()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _eventPublisherMock.Verify(
            x => x.PublishAsync(
                It.Is<CarDealer.Contracts.Events.Auth.UserRegisteredEvent>(e =>
                    e.Email == "newuser@example.com" &&
                    e.FirstName == "John" &&
                    e.LastName == "Doe"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Duplicate Email

    [Fact]
    public async Task Handle_EmailAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        var existingUser = new ApplicationUser("existing", "existing@example.com", "hashed");
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var command = CreateValidCommand(email: "existing@example.com");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    #endregion

    #region AccountType Mapping

    [Theory]
    [InlineData(AccountType.Buyer)]
    [InlineData(AccountType.Seller)]
    [InlineData(AccountType.Dealer)]
    public async Task Handle_SetsCorrectAccountType(AccountType accountType)
    {
        // Arrange
        var command = CreateValidCommand(accountType: accountType);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<ApplicationUser>(u => u.AccountType == accountType),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SellerAccountType_SetsUserIntentToSell()
    {
        // Arrange
        var command = CreateValidCommand(accountType: AccountType.Seller);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<ApplicationUser>(u => u.UserIntent == UserIntent.Sell),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Phone Number

    [Fact]
    public async Task Handle_WithPhoneNumber_SetsPhoneOnUser()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<ApplicationUser>(u => u.PhoneNumber == "+18091234567"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Username Fallback

    [Fact]
    public async Task Handle_NoUserName_UsesEmailPrefixAsUserName()
    {
        // Arrange
        var command = new RegisterCommand(
            UserName: null,
            Email: "testprefix@example.com",
            Password: "StrongPassword123!"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.UserName.Should().Be("testprefix");
    }

    #endregion
}
