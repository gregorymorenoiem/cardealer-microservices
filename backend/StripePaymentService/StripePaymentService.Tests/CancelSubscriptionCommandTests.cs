using FluentAssertions;
using Moq;
using StripePaymentService.Application.Features.Subscription.Commands;
using StripePaymentService.Domain.Entities;
using StripePaymentService.Domain.Interfaces;
using Xunit;

namespace StripePaymentService.Tests;

/// <summary>
/// Tests para CancelSubscription command
/// </summary>
public class CancelSubscriptionCommandTests
{
    [Fact]
    public async Task CancelSubscription_WithValidId_ShouldSucceed()
    {
        // Arrange
        var mockRepository = new Mock<IStripeSubscriptionRepository>();
        var mockLogger = new Mock<ILogger<CancelSubscriptionCommandHandler>>();

        var subscriptionId = Guid.NewGuid();
        var subscription = new StripeSubscription
        {
            Id = subscriptionId,
            StripeSubscriptionId = "sub_test123",
            CustomerId = Guid.NewGuid(),
            Status = "active"
        };

        var command = new CancelSubscriptionCommand(subscriptionId, "User requested");

        mockRepository.Setup(r => r.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        mockRepository.Setup(r => r.UpdateAsync(It.IsAny<StripeSubscription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CancelSubscriptionCommandHandler(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        mockRepository.Verify(r => r.UpdateAsync(It.IsAny<StripeSubscription>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelSubscription_WithNonExistentId_ShouldThrow()
    {
        // Arrange
        var mockRepository = new Mock<IStripeSubscriptionRepository>();
        var mockLogger = new Mock<ILogger<CancelSubscriptionCommandHandler>>();

        var subscriptionId = Guid.NewGuid();
        var command = new CancelSubscriptionCommand(subscriptionId);

        mockRepository.Setup(r => r.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StripeSubscription?)null);

        var handler = new CancelSubscriptionCommandHandler(mockRepository.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CancelSubscription_ShouldUpdateStatus()
    {
        // Arrange
        var mockRepository = new Mock<IStripeSubscriptionRepository>();
        var mockLogger = new Mock<ILogger<CancelSubscriptionCommandHandler>>();

        var subscriptionId = Guid.NewGuid();
        var subscription = new StripeSubscription
        {
            Id = subscriptionId,
            StripeSubscriptionId = "sub_test123",
            CustomerId = Guid.NewGuid(),
            Status = "active"
        };

        var command = new CancelSubscriptionCommand(subscriptionId);

        mockRepository.Setup(r => r.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        mockRepository.Setup(r => r.UpdateAsync(It.IsAny<StripeSubscription>(), It.IsAny<CancellationToken>()))
            .Callback<StripeSubscription, CancellationToken>((sub, ct) =>
            {
                sub.Status.Should().Be("canceled");
                sub.CanceledAt.Should().NotBeNull();
            })
            .Returns(Task.CompletedTask);

        var handler = new CancelSubscriptionCommandHandler(mockRepository.Object, mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        mockRepository.Verify(r => r.UpdateAsync(It.IsAny<StripeSubscription>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
