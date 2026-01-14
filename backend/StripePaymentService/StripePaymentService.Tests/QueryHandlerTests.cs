using FluentAssertions;
using Moq;
using StripePaymentService.Application.Features.PaymentIntent.Queries;
using StripePaymentService.Application.Features.Subscription.Queries;
using StripePaymentService.Domain.Entities;
using StripePaymentService.Domain.Interfaces;
using Xunit;

namespace StripePaymentService.Tests;

/// <summary>
/// Tests para Query Handlers
/// </summary>
public class QueryHandlerTests
{
    [Fact]
    public async Task GetPaymentIntent_WithValidId_ShouldReturnDto()
    {
        // Arrange
        var mockRepository = new Mock<IStripePaymentIntentRepository>();
        var mockLogger = new Mock<ILogger<GetPaymentIntentQueryHandler>>();

        var paymentIntentId = Guid.NewGuid();
        var paymentIntent = new StripePaymentIntent
        {
            Id = paymentIntentId,
            StripePaymentIntentId = "pi_test123",
            Amount = 10000,
            Currency = "USD",
            Status = "succeeded"
        };

        var query = new GetPaymentIntentQuery(paymentIntentId);

        mockRepository.Setup(r => r.GetByIdAsync(paymentIntentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentIntent);

        var handler = new GetPaymentIntentQueryHandler(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PaymentIntentId.Should().Be(paymentIntentId);
        result.Amount.Should().Be(10000);
    }

    [Fact]
    public async Task GetPaymentIntent_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var mockRepository = new Mock<IStripePaymentIntentRepository>();
        var mockLogger = new Mock<ILogger<GetPaymentIntentQueryHandler>>();

        var paymentIntentId = Guid.NewGuid();
        var query = new GetPaymentIntentQuery(paymentIntentId);

        mockRepository.Setup(r => r.GetByIdAsync(paymentIntentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StripePaymentIntent?)null);

        var handler = new GetPaymentIntentQueryHandler(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ListSubscriptions_WithValidCustomerId_ShouldReturnList()
    {
        // Arrange
        var mockRepository = new Mock<IStripeSubscriptionRepository>();
        var mockLogger = new Mock<ILogger<ListSubscriptionsQueryHandler>>();

        var customerId = Guid.NewGuid();
        var subscriptions = new List<StripeSubscription>
        {
            new() { Id = Guid.NewGuid(), CustomerId = customerId, Status = "active", Amount = 10000 },
            new() { Id = Guid.NewGuid(), CustomerId = customerId, Status = "active", Amount = 5000 }
        };

        var query = new ListSubscriptionsQuery(customerId);

        mockRepository.Setup(r => r.GetByCustomerIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        var handler = new ListSubscriptionsQueryHandler(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.All(s => s.CustomerId == customerId).Should().BeTrue();
    }

    [Fact]
    public async Task ListSubscriptions_WithPagination_ShouldRespectPageSize()
    {
        // Arrange
        var mockRepository = new Mock<IStripeSubscriptionRepository>();
        var mockLogger = new Mock<ILogger<ListSubscriptionsQueryHandler>>();

        var customerId = Guid.NewGuid();
        var subscriptions = new List<StripeSubscription>();
        for (int i = 0; i < 25; i++)
        {
            subscriptions.Add(new StripeSubscription
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Status = "active",
                Amount = 10000 + (i * 1000)
            });
        }

        var query = new ListSubscriptionsQuery(customerId, page: 2, pageSize: 10);

        mockRepository.Setup(r => r.GetByCustomerIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        var handler = new ListSubscriptionsQueryHandler(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(10); // 10 items per page
    }
}
