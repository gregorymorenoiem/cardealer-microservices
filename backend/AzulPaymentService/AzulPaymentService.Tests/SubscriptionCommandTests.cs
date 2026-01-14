using Xunit;
using FluentAssertions;
using Moq;
using AzulPaymentService.Domain.Entities;
using AzulPaymentService.Domain.Enums;
using AzulPaymentService.Domain.Interfaces;
using AzulPaymentService.Application.DTOs;
using AzulPaymentService.Application.Features.Subscription.Commands;
using Serilog;

namespace AzulPaymentService.Tests;

/// <summary>
/// Tests para operaciones de suscripción
/// </summary>
public class SubscriptionCommandTests
{
    private readonly Mock<IAzulSubscriptionRepository> _mockSubscriptionRepo;
    private readonly Mock<ILogger<CreateSubscriptionCommandHandler>> _mockLogger;

    public SubscriptionCommandTests()
    {
        _mockSubscriptionRepo = new Mock<IAzulSubscriptionRepository>();
        _mockLogger = new Mock<ILogger<CreateSubscriptionCommandHandler>>();
    }

    [Fact]
    public async Task CreateSubscriptionCommand_WithValidData_ReturnsSuccessResponse()
    {
        // Arrange
        var subscriptionRequest = new SubscriptionRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 500m,
            Currency = "DOP",
            Frequency = "Monthly",
            StartDate = DateTime.UtcNow.AddDays(1),
            CardNumber = "4111111111111111",
            CardExpiryMonth = "12",
            CardExpiryYear = "2025",
            CardCVV = "123",
            PlanName = "Pro Plan"
        };

        var handler = new CreateSubscriptionCommandHandler(_mockSubscriptionRepo.Object, _mockLogger.Object);
        var command = new CreateSubscriptionCommand(subscriptionRequest);

        _mockSubscriptionRepo
            .Setup(x => x.CreateAsync(It.IsAny<AzulSubscription>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulSubscription sub, CancellationToken ct) => sub);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.Amount.Should().Be(500m);
        result.Frequency.Should().Be("Monthly");
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task CreateSubscriptionCommand_WithMonthlyFrequency_CalculatesCorrectNextChargeDate()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var subscriptionRequest = new SubscriptionRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 300m,
            Currency = "DOP",
            Frequency = "Monthly",
            StartDate = startDate,
            CardToken = "tok_visa"
        };

        var handler = new CreateSubscriptionCommandHandler(_mockSubscriptionRepo.Object, _mockLogger.Object);
        var command = new CreateSubscriptionCommand(subscriptionRequest);

        _mockSubscriptionRepo
            .Setup(x => x.CreateAsync(It.IsAny<AzulSubscription>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulSubscription sub, CancellationToken ct) => sub);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.NextChargeDate.Should().Be(startDate.AddMonths(1));
    }

    [Fact]
    public async Task CreateSubscriptionCommand_WithAnnualFrequency_CalculatesCorrectNextChargeDate()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var subscriptionRequest = new SubscriptionRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 3000m,
            Currency = "DOP",
            Frequency = "Annual",
            StartDate = startDate,
            CardToken = "tok_visa"
        };

        var handler = new CreateSubscriptionCommandHandler(_mockSubscriptionRepo.Object, _mockLogger.Object);
        var command = new CreateSubscriptionCommand(subscriptionRequest);

        _mockSubscriptionRepo
            .Setup(x => x.CreateAsync(It.IsAny<AzulSubscription>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulSubscription sub, CancellationToken ct) => sub);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.NextChargeDate.Should().Be(startDate.AddYears(1));
    }

    [Fact]
    public async Task CreateSubscriptionCommand_WithEndDate_ShouldIncludeIt()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = startDate.AddMonths(6);

        var subscriptionRequest = new SubscriptionRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 250m,
            Currency = "DOP",
            Frequency = "Monthly",
            StartDate = startDate,
            EndDate = endDate,
            CardToken = "tok_visa"
        };

        var handler = new CreateSubscriptionCommandHandler(_mockSubscriptionRepo.Object, _mockLogger.Object);
        var command = new CreateSubscriptionCommand(subscriptionRequest);

        _mockSubscriptionRepo
            .Setup(x => x.CreateAsync(It.IsAny<AzulSubscription>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulSubscription sub, CancellationToken ct) => sub);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EndDate.Should().Be(endDate);
    }

    [Fact]
    public async Task CreateSubscriptionCommand_ShouldCallRepositoryCreateAsync()
    {
        // Arrange
        var subscriptionRequest = new SubscriptionRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "DOP",
            Frequency = "Weekly",
            StartDate = DateTime.UtcNow.AddDays(1),
            CardToken = "tok_visa"
        };

        var handler = new CreateSubscriptionCommandHandler(_mockSubscriptionRepo.Object, _mockLogger.Object);
        var command = new CreateSubscriptionCommand(subscriptionRequest);

        _mockSubscriptionRepo
            .Setup(x => x.CreateAsync(It.IsAny<AzulSubscription>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulSubscription sub, CancellationToken ct) => sub);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockSubscriptionRepo.Verify(
            x => x.CreateAsync(It.IsAny<AzulSubscription>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
