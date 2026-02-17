using FluentAssertions;
using Moq;
using StripePaymentService.Application.DTOs;
using StripePaymentService.Application.Features.Subscription.Commands;
using StripePaymentService.Domain.Entities;
using StripePaymentService.Domain.Interfaces;
using Xunit;

namespace StripePaymentService.Tests;

/// <summary>
/// Tests para CreateSubscription command
/// </summary>
public class CreateSubscriptionCommandTests
{
    [Fact]
    public async Task CreateSubscription_WithValidData_ShouldSucceed()
    {
        // Arrange
        var mockSubscriptionRepository = new Mock<IStripeSubscriptionRepository>();
        var mockCustomerRepository = new Mock<IStripeCustomerRepository>();
        var mockLogger = new Mock<ILogger<CreateSubscriptionCommandHandler>>();

        var customerId = Guid.NewGuid();
        var customer = new StripeCustomer
        {
            Id = customerId,
            StripeCustomerId = "cus_test123",
            UserId = Guid.NewGuid(),
            Email = "test@example.com"
        };

        var request = new CreateSubscriptionRequestDto
        {
            CustomerId = customerId,
            Currency = "USD",
            TrialDays = 14
        };

        var command = new CreateSubscriptionCommand(request);

        mockCustomerRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        mockSubscriptionRepository.Setup(r => r.CreateAsync(It.IsAny<StripeSubscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateSubscriptionCommandHandler(
            mockSubscriptionRepository.Object,
            mockCustomerRepository.Object,
            mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customerId);
        result.Currency.Should().Be("USD");
        mockSubscriptionRepository.Verify(r => r.CreateAsync(It.IsAny<StripeSubscription>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSubscription_WithNonExistentCustomer_ShouldThrow()
    {
        // Arrange
        var mockSubscriptionRepository = new Mock<IStripeSubscriptionRepository>();
        var mockCustomerRepository = new Mock<IStripeCustomerRepository>();
        var mockLogger = new Mock<ILogger<CreateSubscriptionCommandHandler>>();

        var customerId = Guid.NewGuid();
        var request = new CreateSubscriptionRequestDto
        {
            CustomerId = customerId,
            Currency = "USD"
        };

        var command = new CreateSubscriptionCommand(request);

        mockCustomerRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StripeCustomer?)null);

        var handler = new CreateSubscriptionCommandHandler(
            mockSubscriptionRepository.Object,
            mockCustomerRepository.Object,
            mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateSubscription_WithTrialDays_ShouldSet()
    {
        // Arrange
        var mockSubscriptionRepository = new Mock<IStripeSubscriptionRepository>();
        var mockCustomerRepository = new Mock<IStripeCustomerRepository>();
        var mockLogger = new Mock<ILogger<CreateSubscriptionCommandHandler>>();

        var customerId = Guid.NewGuid();
        var customer = new StripeCustomer
        {
            Id = customerId,
            StripeCustomerId = "cus_test123",
            UserId = Guid.NewGuid(),
            Email = "test@example.com"
        };

        var request = new CreateSubscriptionRequestDto
        {
            CustomerId = customerId,
            TrialDays = 30
        };

        var command = new CreateSubscriptionCommand(request);

        mockCustomerRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        mockSubscriptionRepository.Setup(r => r.CreateAsync(It.IsAny<StripeSubscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateSubscriptionCommandHandler(
            mockSubscriptionRepository.Object,
            mockCustomerRepository.Object,
            mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SubscriptionId.Should().NotBeEmpty();
    }
}
