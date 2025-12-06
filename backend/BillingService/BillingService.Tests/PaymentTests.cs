using BillingService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BillingService.Tests;

public class PaymentTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesPayment()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var amount = 99.99m;
        var method = PaymentMethod.CreditCard;

        // Act
        var payment = new Payment(dealerId, amount, method);

        // Assert
        payment.Id.Should().NotBeEmpty();
        payment.DealerId.Should().Be(dealerId);
        payment.Amount.Should().Be(amount);
        payment.Method.Should().Be(method);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_WithZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var dealerId = Guid.NewGuid();

        // Act & Assert
        var act = () => new Payment(dealerId, 0m, PaymentMethod.CreditCard);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var dealerId = Guid.NewGuid();

        // Act & Assert
        var act = () => new Payment(dealerId, -50m, PaymentMethod.CreditCard);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkProcessing_ChangesStatusToProcessing()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), 99.99m, PaymentMethod.CreditCard);

        // Act
        payment.MarkProcessing();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Processing);
    }

    [Fact]
    public void MarkSucceeded_ChangesStatusToSucceeded()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), 99.99m, PaymentMethod.CreditCard);
        var receiptUrl = "https://example.com/receipt/123";

        // Act
        payment.MarkSucceeded(receiptUrl);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.ProcessedAt.Should().NotBeNull();
        payment.ReceiptUrl.Should().Be(receiptUrl);
    }

    [Fact]
    public void MarkFailed_ChangesStatusToFailed()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), 99.99m, PaymentMethod.CreditCard);
        var reason = "Insufficient funds";

        // Act
        payment.MarkFailed(reason);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be(reason);
    }

    [Fact]
    public void Refund_FullAmount_ChangesStatusToRefunded()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), 99.99m, PaymentMethod.CreditCard);
        payment.MarkSucceeded();
        var reason = "Customer request";

        // Act
        payment.Refund(99.99m, reason);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.RefundedAmount.Should().Be(99.99m);
        payment.RefundReason.Should().Be(reason);
        payment.RefundedAt.Should().NotBeNull();
    }

    [Fact]
    public void Refund_PartialAmount_ChangesStatusToPartiallyRefunded()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), 99.99m, PaymentMethod.CreditCard);
        payment.MarkSucceeded();
        var reason = "Partial refund";

        // Act
        payment.Refund(50m, reason);

        // Assert
        payment.Status.Should().Be(PaymentStatus.PartiallyRefunded);
        payment.RefundedAmount.Should().Be(50m);
    }

    [Fact]
    public void Refund_ExceedingAmount_ThrowsArgumentException()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), 99.99m, PaymentMethod.CreditCard);
        payment.MarkSucceeded();

        // Act & Assert
        var act = () => payment.Refund(100m, "Too much");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkDisputed_ChangesStatusToDisputed()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), 99.99m, PaymentMethod.CreditCard);
        payment.MarkSucceeded();

        // Act
        payment.MarkDisputed();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Disputed);
    }

    [Fact]
    public void SetStripeInfo_StoresStripeDetails()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), 99.99m, PaymentMethod.CreditCard);
        var paymentIntentId = "pi_123456789";
        var chargeId = "ch_987654321";
        var receiptUrl = "https://stripe.com/receipt/123";

        // Act
        payment.SetStripeInfo(paymentIntentId, chargeId, receiptUrl);

        // Assert
        payment.StripePaymentIntentId.Should().Be(paymentIntentId);
        payment.StripeChargeId.Should().Be(chargeId);
        payment.ReceiptUrl.Should().Be(receiptUrl);
    }
}
