using FluentAssertions;
using Moq;
using StripePaymentService.Application.DTOs;
using StripePaymentService.Application.Features.PaymentIntent.Commands;
using StripePaymentService.Domain.Entities;
using StripePaymentService.Domain.Interfaces;
using Xunit;

namespace StripePaymentService.Tests;

/// <summary>
/// Tests para CreatePaymentIntent command
/// </summary>
public class CreatePaymentIntentCommandTests
{
    [Fact]
    public async Task CreatePaymentIntent_WithValidData_ShouldSucceed()
    {
        // Arrange
        var mockRepository = new Mock<IStripePaymentIntentRepository>();
        var mockLogger = new Mock<ILogger<CreatePaymentIntentCommandHandler>>();

        var request = new CreatePaymentIntentRequestDto
        {
            Amount = 10000,
            Currency = "USD",
            CustomerEmail = "test@example.com",
            CustomerName = "John Doe",
            Description = "Test payment"
        };

        var command = new CreatePaymentIntentCommand(request);

        mockRepository.Setup(r => r.CreateAsync(It.IsAny<StripePaymentIntent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentIntentCommandHandler(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(10000);
        result.Currency.Should().Be("USD");
        mockRepository.Verify(r => r.CreateAsync(It.IsAny<StripePaymentIntent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentIntent_WithInvalidAmount_ShouldThrow()
    {
        // Arrange
        var mockRepository = new Mock<IStripePaymentIntentRepository>();
        var mockLogger = new Mock<ILogger<CreatePaymentIntentCommandHandler>>();

        var request = new CreatePaymentIntentRequestDto
        {
            Amount = -100, // Monto negativo
            Currency = "USD"
        };

        var command = new CreatePaymentIntentCommand(request);
        var handler = new CreatePaymentIntentCommandHandler(mockRepository.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreatePaymentIntent_ShouldGenerateClientSecret()
    {
        // Arrange
        var mockRepository = new Mock<IStripePaymentIntentRepository>();
        var mockLogger = new Mock<ILogger<CreatePaymentIntentCommandHandler>>();

        var request = new CreatePaymentIntentRequestDto
        {
            Amount = 5000,
            Currency = "USD"
        };

        var command = new CreatePaymentIntentCommand(request);
        mockRepository.Setup(r => r.CreateAsync(It.IsAny<StripePaymentIntent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentIntentCommandHandler(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ClientSecret.Should().NotBeNullOrEmpty();
        result.ClientSecret.Should().StartWith("pi_");
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("DOP")]
    [InlineData("EUR")]
    public async Task CreatePaymentIntent_WithDifferentCurrencies_ShouldSucceed(string currency)
    {
        // Arrange
        var mockRepository = new Mock<IStripePaymentIntentRepository>();
        var mockLogger = new Mock<ILogger<CreatePaymentIntentCommandHandler>>();

        var request = new CreatePaymentIntentRequestDto
        {
            Amount = 10000,
            Currency = currency
        };

        var command = new CreatePaymentIntentCommand(request);
        mockRepository.Setup(r => r.CreateAsync(It.IsAny<StripePaymentIntent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreatePaymentIntentCommandHandler(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Currency.Should().Be(currency);
    }
}
