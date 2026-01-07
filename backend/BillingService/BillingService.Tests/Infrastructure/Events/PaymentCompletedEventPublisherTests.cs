using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BillingService.Infrastructure.Events;
using CarDealer.Contracts.Events.Billing;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace BillingService.Tests.Infrastructure.Events;

/// <summary>
/// Unit tests para PaymentCompletedEventPublisher
/// </summary>
public class PaymentCompletedEventPublisherTests
{
    private readonly Mock<IConnection> _connectionMock;
    private readonly Mock<IModel> _channelMock;
    private readonly Mock<ILogger<PaymentCompletedEventPublisher>> _loggerMock;
    private readonly PaymentCompletedEventPublisher _publisher;

    public PaymentCompletedEventPublisherTests()
    {
        _connectionMock = new Mock<IConnection>();
        _channelMock = new Mock<IModel>();
        _loggerMock = new Mock<ILogger<PaymentCompletedEventPublisher>>();

        _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

        // ✅ PROVEN FIX: Setup Channel CreateBasicProperties (REQUIRED for all tests)
        var mockProperties = new Mock<IBasicProperties>();
        mockProperties.SetupAllProperties();
        _channelMock.Setup(ch => ch.CreateBasicProperties()).Returns(mockProperties.Object);

        _publisher = new PaymentCompletedEventPublisher(_connectionMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task PublishAsync_ValidEvent_PublishesToRabbitMQ()
    {
        // Arrange
        var @event = new PaymentCompletedEvent
        {
            EventId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserEmail = "customer@example.com",
            UserName = "John Doe",
            Amount = 99.00m,
            Currency = "USD",
            Description = "Premium Plan - Monthly",
            SubscriptionPlan = "Premium",
            StripePaymentIntentId = "pi_1234567890abcdef",
            PaidAt = DateTime.UtcNow,
            OccurredAt = DateTime.UtcNow
        };

        byte[]? publishedBody = null;
        string? publishedRoutingKey = null;

        _channelMock
            .Setup(ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()))
            .Callback<string, string, bool, IBasicProperties, ReadOnlyMemory<byte>>(
                (exchange, routingKey, mandatory, properties, body) =>
                {
                    publishedRoutingKey = routingKey;
                    publishedBody = body.ToArray();
                });

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        _channelMock.Verify(
            ch => ch.BasicPublish(
                "cardealer.events",
                "payment.completed",
                false,
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()),
            Times.Once);

        publishedRoutingKey.Should().Be("payment.completed");
        publishedBody.Should().NotBeNull();

        var bodyString = Encoding.UTF8.GetString(publishedBody!);
        bodyString.Should().Contain(@event.UserEmail);
        bodyString.Should().Contain(@event.UserName);
        bodyString.Should().Contain(@event.Amount.ToString());
        bodyString.Should().Contain(@event.StripePaymentIntentId);
    }

    [Fact]
    public async Task PublishAsync_PaymentWithAllFields_SerializesCorrectly()
    {
        // Arrange
        var @event = new PaymentCompletedEvent
        {
            EventId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserEmail = "vip@example.com",
            UserName = "Jane Smith",
            Amount = 299.00m,
            Currency = "EUR",
            Description = "Enterprise Plan - Annual",
            SubscriptionPlan = "Enterprise",
            StripePaymentIntentId = "pi_enterprise_2024",
            PaidAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            OccurredAt = DateTime.UtcNow
        };

        byte[]? publishedBody = null;

        _channelMock
            .Setup(ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()))
            .Callback<string, string, bool, IBasicProperties, ReadOnlyMemory<byte>>(
                (_, _, _, _, body) => publishedBody = body.ToArray());

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        publishedBody.Should().NotBeNull();
        var bodyString = Encoding.UTF8.GetString(publishedBody!);
        bodyString.Should().Contain("vip@example.com");
        bodyString.Should().Contain("Jane Smith");
        bodyString.Should().Contain("299");
        bodyString.Should().Contain("EUR");
        bodyString.Should().Contain("Enterprise");
        bodyString.Should().Contain("pi_enterprise_2024");
    }

    [Fact]
    public async Task PublishAsync_RabbitMQUnavailable_ThrowsException()
    {
        // Arrange
        var @event = new PaymentCompletedEvent
        {
            EventId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            UserName = "Test User",
            Amount = 49.99m,
            Currency = "USD",
            StripePaymentIntentId = "pi_test123"
        };

        _channelMock
            .Setup(ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()))
            .Throws(new InvalidOperationException("Connection to RabbitMQ lost"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _publisher.PublishAsync(@event, CancellationToken.None));
    }

    [Fact]
    public async Task PublishAsync_LogsPaymentDetails()
    {
        // Arrange
        var @event = new PaymentCompletedEvent
        {
            EventId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserEmail = "logger@example.com",
            UserName = "Logger Test",
            Amount = 19.99m,
            Currency = "USD",
            StripePaymentIntentId = "pi_log_test"
        };

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("PaymentCompletedEvent") ||
                    v.ToString()!.Contains(@event.PaymentId.ToString())),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(9999.99)]
    [InlineData(0)]
    public async Task PublishAsync_DifferentAmounts_PublishesAll(decimal amount)
    {
        // Arrange
        var @event = new PaymentCompletedEvent
        {
            EventId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserEmail = "amount@test.com",
            UserName = "Amount Test",
            Amount = amount,
            Currency = "USD",
            StripePaymentIntentId = "pi_amount_test"
        };

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        _channelMock.Verify(
            ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("MXN")]
    public async Task PublishAsync_DifferentCurrencies_SerializesCorrectly(string currency)
    {
        // Arrange
        var @event = new PaymentCompletedEvent
        {
            EventId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserEmail = "currency@test.com",
            UserName = "Currency Test",
            Amount = 100.00m,
            Currency = currency,
            StripePaymentIntentId = "pi_currency_test"
        };

        byte[]? publishedBody = null;

        _channelMock
            .Setup(ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()))
            .Callback<string, string, bool, IBasicProperties, ReadOnlyMemory<byte>>(
                (_, _, _, _, body) => publishedBody = body.ToArray());

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        var bodyString = Encoding.UTF8.GetString(publishedBody!);
        bodyString.Should().Contain(currency);
    }

    [Fact]
    public async Task PublishAsync_MultiplePayments_PublishesInSequence()
    {
        // Arrange
        var events = new[]
        {
            new PaymentCompletedEvent { EventId = Guid.NewGuid(), PaymentId = Guid.NewGuid(), UserId = Guid.NewGuid(), UserEmail = "user1@test.com", UserName = "User 1", Amount = 29.99m, Currency = "USD", StripePaymentIntentId = "pi_1" },
            new PaymentCompletedEvent { EventId = Guid.NewGuid(), PaymentId = Guid.NewGuid(), UserId = Guid.NewGuid(), UserEmail = "user2@test.com", UserName = "User 2", Amount = 49.99m, Currency = "USD", StripePaymentIntentId = "pi_2" },
            new PaymentCompletedEvent { EventId = Guid.NewGuid(), PaymentId = Guid.NewGuid(), UserId = Guid.NewGuid(), UserEmail = "user3@test.com", UserName = "User 3", Amount = 99.99m, Currency = "USD", StripePaymentIntentId = "pi_3" }
        };

        // Act
        foreach (var evt in events)
        {
            await _publisher.PublishAsync(evt, CancellationToken.None);
        }

        // Assert
        _channelMock.Verify(
            ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task PublishAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _publisher.PublishAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task PublishAsync_EmptyStripePaymentIntentId_StillPublishes()
    {
        // Arrange
        var @event = new PaymentCompletedEvent
        {
            EventId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserEmail = "empty@test.com",
            UserName = "Empty Test",
            Amount = 25.00m,
            Currency = "USD",
            StripePaymentIntentId = string.Empty // Vacío pero válido
        };

        // Act
        await _publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        _channelMock.Verify(
            ch => ch.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()),
            Times.Once,
            "Publisher debe publicar incluso con datos incompletos");
    }
}
