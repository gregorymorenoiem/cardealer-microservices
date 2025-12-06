using BillingService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BillingService.Tests;

public class InvoiceTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInvoice()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var invoiceNumber = "INV-2024-0001";
        var subtotal = 100m;
        var taxAmount = 10m;
        var dueDate = DateTime.UtcNow.AddDays(30);

        // Act
        var invoice = new Invoice(dealerId, invoiceNumber, subtotal, taxAmount, dueDate);

        // Assert
        invoice.Id.Should().NotBeEmpty();
        invoice.DealerId.Should().Be(dealerId);
        invoice.InvoiceNumber.Should().Be(invoiceNumber);
        invoice.Subtotal.Should().Be(subtotal);
        invoice.TaxAmount.Should().Be(taxAmount);
        invoice.TotalAmount.Should().Be(110m);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_WithEmptyInvoiceNumber_ThrowsArgumentException()
    {
        // Arrange
        var dealerId = Guid.NewGuid();

        // Act & Assert
        var act = () => new Invoice(dealerId, "", 100m, 10m, DateTime.UtcNow.AddDays(30));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNegativeSubtotal_ThrowsArgumentException()
    {
        // Arrange
        var dealerId = Guid.NewGuid();

        // Act & Assert
        var act = () => new Invoice(dealerId, "INV-001", -100m, 10m, DateTime.UtcNow.AddDays(30));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Issue_ChangesStatusToIssued()
    {
        // Arrange
        var invoice = new Invoice(Guid.NewGuid(), "INV-001", 100m, 10m, DateTime.UtcNow.AddDays(30));

        // Act
        invoice.Issue();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void MarkSent_ChangesStatusToSent()
    {
        // Arrange
        var invoice = new Invoice(Guid.NewGuid(), "INV-001", 100m, 10m, DateTime.UtcNow.AddDays(30));
        invoice.Issue();

        // Act
        invoice.MarkSent();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Sent);
    }

    [Fact]
    public void RecordPayment_FullPayment_ChangesStatusToPaid()
    {
        // Arrange
        var invoice = new Invoice(Guid.NewGuid(), "INV-001", 100m, 10m, DateTime.UtcNow.AddDays(30));
        invoice.Issue();

        // Act
        invoice.RecordPayment(110m);

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.PaidAmount.Should().Be(110m);
        invoice.PaidDate.Should().NotBeNull();
    }

    [Fact]
    public void RecordPayment_PartialPayment_ChangesStatusToPartiallyPaid()
    {
        // Arrange
        var invoice = new Invoice(Guid.NewGuid(), "INV-001", 100m, 10m, DateTime.UtcNow.AddDays(30));
        invoice.Issue();

        // Act
        invoice.RecordPayment(50m);

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
        invoice.PaidAmount.Should().Be(50m);
    }

    [Fact]
    public void RecordPayment_ZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var invoice = new Invoice(Guid.NewGuid(), "INV-001", 100m, 10m, DateTime.UtcNow.AddDays(30));

        // Act & Assert
        var act = () => invoice.RecordPayment(0m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetBalanceDue_ReturnsCorrectBalance()
    {
        // Arrange
        var invoice = new Invoice(Guid.NewGuid(), "INV-001", 100m, 10m, DateTime.UtcNow.AddDays(30));
        invoice.RecordPayment(30m);

        // Act
        var balance = invoice.GetBalanceDue();

        // Assert
        balance.Should().Be(80m);
    }

    [Fact]
    public void MarkOverdue_ChangesStatusToOverdue()
    {
        // Arrange
        var invoice = new Invoice(Guid.NewGuid(), "INV-001", 100m, 10m, DateTime.UtcNow.AddDays(30));
        invoice.Issue();

        // Act
        invoice.MarkOverdue();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Overdue);
    }

    [Fact]
    public void Cancel_ChangesStatusToCancelled()
    {
        // Arrange
        var invoice = new Invoice(Guid.NewGuid(), "INV-001", 100m, 10m, DateTime.UtcNow.AddDays(30));

        // Act
        invoice.Cancel();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public void Void_ChangesStatusToVoided()
    {
        // Arrange
        var invoice = new Invoice(Guid.NewGuid(), "INV-001", 100m, 10m, DateTime.UtcNow.AddDays(30));

        // Act
        invoice.Void();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Voided);
    }

    [Fact]
    public void AddNotes_SetsNotes()
    {
        // Arrange
        var invoice = new Invoice(Guid.NewGuid(), "INV-001", 100m, 10m, DateTime.UtcNow.AddDays(30));
        var notes = "Payment terms: Net 30";

        // Act
        invoice.AddNotes(notes);

        // Assert
        invoice.Notes.Should().Be(notes);
    }

    [Fact]
    public void SetLineItems_SetsLineItemsJson()
    {
        // Arrange
        var invoice = new Invoice(Guid.NewGuid(), "INV-001", 100m, 10m, DateTime.UtcNow.AddDays(30));
        var lineItems = "[{\"description\":\"Service fee\",\"amount\":100}]";

        // Act
        invoice.SetLineItems(lineItems);

        // Assert
        invoice.LineItems.Should().Be(lineItems);
    }
}
