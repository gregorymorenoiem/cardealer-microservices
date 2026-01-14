using Xunit;
using FluentAssertions;
using AzulPaymentService.Domain.Entities;
using AzulPaymentService.Domain.Enums;

namespace AzulPaymentService.Tests;

/// <summary>
/// Tests para entidades del dominio
/// </summary>
public class DomainEntityTests
{
    [Fact]
    public void AzulTransaction_ShouldBeCreatedWithValidData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        // Act
        var transaction = new AzulTransaction
        {
            Id = transactionId,
            UserId = userId,
            Amount = 1000m,
            Currency = "DOP",
            Status = TransactionStatus.Approved,
            PaymentMethod = PaymentMethod.CreditCard
        };

        // Assert
        transaction.Id.Should().Be(transactionId);
        transaction.UserId.Should().Be(userId);
        transaction.Amount.Should().Be(1000m);
        transaction.Status.Should().Be(TransactionStatus.Approved);
    }

    [Fact]
    public void AzulSubscription_ShouldBeCreatedWithValidData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(1);

        // Act
        var subscription = new AzulSubscription
        {
            Id = subscriptionId,
            UserId = userId,
            Amount = 500m,
            Currency = "DOP",
            Frequency = SubscriptionFrequency.Monthly,
            StartDate = startDate,
            Status = "Active"
        };

        // Assert
        subscription.Id.Should().Be(subscriptionId);
        subscription.UserId.Should().Be(userId);
        subscription.Amount.Should().Be(500m);
        subscription.Frequency.Should().Be(SubscriptionFrequency.Monthly);
    }

    [Fact]
    public void AzulWebhookEvent_ShouldBeCreatedWithValidData()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var payload = @"{""type"": ""transaction.approved""}";

        // Act
        var webhookEvent = new AzulWebhookEvent
        {
            Id = eventId,
            EventType = "transaction.approved",
            PayloadJson = payload,
            IsValidated = true
        };

        // Assert
        webhookEvent.Id.Should().Be(eventId);
        webhookEvent.EventType.Should().Be("transaction.approved");
        webhookEvent.IsValidated.Should().BeTrue();
    }

    [Fact]
    public void TransactionStatus_ShouldHaveAllExpectedValues()
    {
        // Assert
        TransactionStatus.Pending.Should().Be(TransactionStatus.Pending);
        TransactionStatus.Approved.Should().Be(TransactionStatus.Approved);
        TransactionStatus.Declined.Should().Be(TransactionStatus.Declined);
        TransactionStatus.Refunded.Should().Be(TransactionStatus.Refunded);
    }

    [Fact]
    public void PaymentMethod_ShouldHaveAllExpectedValues()
    {
        // Assert
        PaymentMethod.CreditCard.Should().Be(PaymentMethod.CreditCard);
        PaymentMethod.DebitCard.Should().Be(PaymentMethod.DebitCard);
        PaymentMethod.ACH.Should().Be(PaymentMethod.ACH);
        PaymentMethod.MobilePayment.Should().Be(PaymentMethod.MobilePayment);
    }

    [Fact]
    public void SubscriptionFrequency_ShouldHaveAllExpectedValues()
    {
        // Assert
        SubscriptionFrequency.Daily.Should().Be(SubscriptionFrequency.Daily);
        SubscriptionFrequency.Weekly.Should().Be(SubscriptionFrequency.Weekly);
        SubscriptionFrequency.Monthly.Should().Be(SubscriptionFrequency.Monthly);
        SubscriptionFrequency.Annual.Should().Be(SubscriptionFrequency.Annual);
    }
}
