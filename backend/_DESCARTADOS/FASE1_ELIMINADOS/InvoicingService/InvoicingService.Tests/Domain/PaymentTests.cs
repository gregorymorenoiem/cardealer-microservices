using FluentAssertions;
using InvoicingService.Domain.Entities;
using Xunit;

namespace InvoicingService.Tests.Domain;

public class PaymentTests
{
    private static readonly Guid TestDealerId = Guid.NewGuid();
    private static readonly Guid TestInvoiceId = Guid.NewGuid();
    private static readonly Guid TestCustomerId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    [Fact]
    public void Payment_Creation_SetsCorrectDefaults()
    {
        // Arrange & Act
        var payment = new Payment(
            TestDealerId,
            "PAY-2024-000001",
            TestInvoiceId,
            TestCustomerId,
            1000m,
            "MXN",
            PaymentMethod.CreditCard,
            DateTime.UtcNow,
            TestUserId);

        // Assert
        payment.Id.Should().NotBeEmpty();
        payment.DealerId.Should().Be(TestDealerId);
        payment.PaymentNumber.Should().Be("PAY-2024-000001");
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Amount.Should().Be(1000m);
        payment.Method.Should().Be(PaymentMethod.CreditCard);
    }

    [Fact]
    public void Payment_MarkAsProcessing_ChangesStatus()
    {
        // Arrange
        var payment = CreateTestPayment();

        // Act
        payment.MarkAsProcessing();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Processing);
    }

    [Fact]
    public void Payment_Complete_ChangesStatusToCompleted()
    {
        // Arrange
        var payment = CreateTestPayment();
        payment.MarkAsProcessing();

        // Act
        payment.Complete("TXN-12345");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.TransactionId.Should().Be("TXN-12345");
    }

    [Fact]
    public void Payment_Fail_ChangesStatusToFailed()
    {
        // Arrange
        var payment = CreateTestPayment();

        // Act
        payment.Fail("Insufficient funds");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void Payment_Cancel_ChangesStatusToCancelled()
    {
        // Arrange
        var payment = CreateTestPayment();

        // Act
        payment.Cancel();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Cancelled);
    }

    [Fact]
    public void Payment_Refund_UpdatesRefundedAmount()
    {
        // Arrange
        var payment = CreateTestPayment();
        payment.Complete();

        // Act
        payment.Refund(500m, "Customer request");

        // Assert
        payment.RefundedAmount.Should().Be(500m);
        payment.RefundReason.Should().Be("Customer request");
        payment.RefundedAt.Should().NotBeNull();
    }

    [Fact]
    public void Payment_FullRefund_ChangesStatusToRefunded()
    {
        // Arrange
        var payment = CreateTestPayment();
        payment.Complete();

        // Act
        payment.Refund(1000m, "Full refund");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.RefundedAmount.Should().Be(1000m);
    }

    [Fact]
    public void Payment_Refund_ThrowsIfExceedsAmount()
    {
        // Arrange
        var payment = CreateTestPayment();
        payment.Complete();

        // Act & Assert
        var action = () => payment.Refund(1500m, "Too much");
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Payment_Refund_ThrowsIfNotCompleted()
    {
        // Arrange
        var payment = CreateTestPayment();

        // Act & Assert
        var action = () => payment.Refund(500m, "Test");
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Payment_Complete_ThrowsIfAlreadyRefunded()
    {
        // Arrange
        var payment = CreateTestPayment();
        payment.Complete();
        payment.Refund(1000m, "Full refund");

        // Act & Assert
        var action = () => payment.Complete();
        action.Should().Throw<InvalidOperationException>();
    }

    private Payment CreateTestPayment()
    {
        return new Payment(
            TestDealerId,
            "PAY-2024-000001",
            TestInvoiceId,
            TestCustomerId,
            1000m,
            "MXN",
            PaymentMethod.CreditCard,
            DateTime.UtcNow,
            TestUserId);
    }
}
