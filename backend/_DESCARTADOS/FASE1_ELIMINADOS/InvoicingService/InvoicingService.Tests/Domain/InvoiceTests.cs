using FluentAssertions;
using InvoicingService.Domain.Entities;
using Xunit;

namespace InvoicingService.Tests.Domain;

public class InvoiceTests
{
    private static readonly Guid TestDealerId = Guid.NewGuid();
    private static readonly Guid TestCustomerId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    [Fact]
    public void Invoice_Creation_SetsCorrectDefaults()
    {
        // Arrange & Act
        var invoice = new Invoice(
            TestDealerId,
            "INV-2024-000001",
            InvoiceType.Standard,
            TestCustomerId,
            "John Doe",
            "john@example.com",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            "MXN",
            16m,
            TestUserId);

        // Assert
        invoice.Id.Should().NotBeEmpty();
        invoice.DealerId.Should().Be(TestDealerId);
        invoice.InvoiceNumber.Should().Be("INV-2024-000001");
        invoice.Type.Should().Be(InvoiceType.Standard);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.CustomerName.Should().Be("John Doe");
        invoice.CustomerEmail.Should().Be("john@example.com");
        invoice.Currency.Should().Be("MXN");
        invoice.TaxRate.Should().Be(16m);
        invoice.CreatedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void Invoice_Send_ChangesStatusToSent()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Act
        invoice.Send();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Sent);
    }

    [Fact]
    public void Invoice_Send_ThrowsIfNotDraft()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.Send();

        // Act & Assert
        var action = () => invoice.Send();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Invoice_RecordPayment_UpdatesPaidAmount()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        AddItemToInvoice(invoice);
        invoice.Send();

        // Act
        invoice.RecordPayment(500m);

        // Assert
        invoice.PaidAmount.Should().Be(500m);
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
    }

    [Fact]
    public void Invoice_RecordFullPayment_ChangesStatusToPaid()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        AddItemToInvoice(invoice);
        invoice.Send();
        var total = invoice.Total;

        // Act
        invoice.RecordPayment(total);

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public void Invoice_AddItem_RecalculatesTotals()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        var item = new InvoiceItem(
            invoice.Id,
            "Test Product",
            2m,
            "pcs",
            1000m,
            1);

        // Act
        invoice.AddItem(item);

        // Assert
        invoice.Subtotal.Should().Be(2000m);
        invoice.TaxAmount.Should().Be(320m); // 16% tax
        invoice.Total.Should().Be(2320m);
    }

    [Fact]
    public void Invoice_ApplyDiscount_RecalculatesTotals()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        AddItemToInvoice(invoice);

        // Act
        invoice.ApplyDiscount(200m);

        // Assert
        invoice.DiscountAmount.Should().Be(200m);
        invoice.TaxAmount.Should().Be(288m); // 16% of (2000 - 200)
        invoice.Total.Should().Be(2088m); // 2000 - 200 + 288
    }

    [Fact]
    public void Invoice_Cancel_ThrowsIfPaid()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        AddItemToInvoice(invoice);
        invoice.Send();
        invoice.RecordPayment(invoice.Total);

        // Act & Assert
        var action = () => invoice.Cancel();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Invoice_Refund_ThrowsIfNotPaid()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.Send();

        // Act & Assert
        var action = () => invoice.Refund();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Invoice_BalanceDue_CalculatesCorrectly()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        AddItemToInvoice(invoice);
        invoice.Send();
        invoice.RecordPayment(1000m);

        // Act
        var balance = invoice.BalanceDue;

        // Assert
        balance.Should().Be(invoice.Total - 1000m);
    }

    private Invoice CreateTestInvoice()
    {
        return new Invoice(
            TestDealerId,
            "INV-2024-000001",
            InvoiceType.Standard,
            TestCustomerId,
            "John Doe",
            "john@example.com",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            "MXN",
            16m,
            TestUserId);
    }

    private void AddItemToInvoice(Invoice invoice)
    {
        var item = new InvoiceItem(
            invoice.Id,
            "Test Product",
            2m,
            "pcs",
            1000m,
            1);
        invoice.AddItem(item);
    }
}
