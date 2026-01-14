using Xunit;
using FluentAssertions;
using Moq;
using AzulPaymentService.Domain.Entities;
using AzulPaymentService.Domain.Enums;
using AzulPaymentService.Domain.Interfaces;
using AzulPaymentService.Application.DTOs;
using AzulPaymentService.Application.Features.Charge.Commands;
using Serilog;

namespace AzulPaymentService.Tests;

/// <summary>
/// Tests para operaciones de cobro
/// </summary>
public class ChargeCommandTests
{
    private readonly Mock<IAzulTransactionRepository> _mockTransactionRepo;
    private readonly Mock<ILogger<ChargeCommandHandler>> _mockLogger;

    public ChargeCommandTests()
    {
        _mockTransactionRepo = new Mock<IAzulTransactionRepository>();
        _mockLogger = new Mock<ILogger<ChargeCommandHandler>>();
    }

    [Fact]
    public async Task ChargeCommand_WithValidData_ReturnsSuccessResponse()
    {
        // Arrange
        var chargeRequest = new ChargeRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 1000m,
            Currency = "DOP",
            Description = "Test charge",
            CardNumber = "4111111111111111",
            CardExpiryMonth = "12",
            CardExpiryYear = "2025",
            CardCVV = "123",
            CardholderName = "Test User"
        };

        var handler = new ChargeCommandHandler(_mockTransactionRepo.Object, _mockLogger.Object);
        var command = new ChargeCommand(chargeRequest);

        _mockTransactionRepo
            .Setup(x => x.CreateAsync(It.IsAny<AzulTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulTransaction tx, CancellationToken ct) => tx);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.Amount.Should().Be(1000m);
        result.Currency.Should().Be("DOP");
    }

    [Fact]
    public async Task ChargeCommand_WithNegativeAmount_ThrowsException()
    {
        // Arrange
        var chargeRequest = new ChargeRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = -100m,
            Currency = "DOP"
        };

        var handler = new ChargeCommandHandler(_mockTransactionRepo.Object, _mockLogger.Object);
        var command = new ChargeCommand(chargeRequest);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ChargeCommand_ShouldCallRepositoryCreateAsync()
    {
        // Arrange
        var chargeRequest = new ChargeRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 500m,
            Currency = "DOP",
            CardNumber = "4111111111111111",
            CardExpiryMonth = "12",
            CardExpiryYear = "2025",
            CardCVV = "123"
        };

        var handler = new ChargeCommandHandler(_mockTransactionRepo.Object, _mockLogger.Object);
        var command = new ChargeCommand(chargeRequest);

        _mockTransactionRepo
            .Setup(x => x.CreateAsync(It.IsAny<AzulTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulTransaction tx, CancellationToken ct) => tx);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockTransactionRepo.Verify(
            x => x.CreateAsync(It.IsAny<AzulTransaction>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChargeCommand_WithToken_ShouldSucceed()
    {
        // Arrange
        var chargeRequest = new ChargeRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 750m,
            Currency = "DOP",
            CardToken = "tok_visa_4242"
        };

        var handler = new ChargeCommandHandler(_mockTransactionRepo.Object, _mockLogger.Object);
        var command = new ChargeCommand(chargeRequest);

        _mockTransactionRepo
            .Setup(x => x.CreateAsync(It.IsAny<AzulTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulTransaction tx, CancellationToken ct) => tx);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
    }
}
