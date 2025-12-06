using FluentAssertions;
using InvoicingService.Domain.Entities;
using Xunit;

namespace InvoicingService.Tests.Domain;

public class QuoteTests
{
    private static readonly Guid TestDealerId = Guid.NewGuid();
    private static readonly Guid TestCustomerId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    [Fact]
    public void Quote_Creation_SetsCorrectDefaults()
    {
        // Arrange & Act
        var quote = new Quote(
            TestDealerId,
            "QT-2024-000001",
            TestCustomerId,
            "John Doe",
            "john@example.com",
            "Vehicle Quote",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            "MXN",
            16m,
            TestUserId);

        // Assert
        quote.Id.Should().NotBeEmpty();
        quote.DealerId.Should().Be(TestDealerId);
        quote.QuoteNumber.Should().Be("QT-2024-000001");
        quote.Status.Should().Be(QuoteStatus.Draft);
        quote.Title.Should().Be("Vehicle Quote");
    }

    [Fact]
    public void Quote_Send_ChangesStatusToSent()
    {
        // Arrange
        var quote = CreateTestQuote();

        // Act
        quote.Send();

        // Assert
        quote.Status.Should().Be(QuoteStatus.Sent);
    }

    [Fact]
    public void Quote_MarkAsViewed_SetsViewedAt()
    {
        // Arrange
        var quote = CreateTestQuote();
        quote.Send();

        // Act
        quote.MarkAsViewed();

        // Assert
        quote.Status.Should().Be(QuoteStatus.Viewed);
        quote.ViewedAt.Should().NotBeNull();
    }

    [Fact]
    public void Quote_Accept_SetsAcceptedAt()
    {
        // Arrange
        var quote = CreateTestQuote();
        quote.Send();

        // Act
        quote.Accept();

        // Assert
        quote.Status.Should().Be(QuoteStatus.Accepted);
        quote.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public void Quote_Reject_SetsRejectionReason()
    {
        // Arrange
        var quote = CreateTestQuote();
        quote.Send();

        // Act
        quote.Reject("Too expensive");

        // Assert
        quote.Status.Should().Be(QuoteStatus.Rejected);
        quote.RejectionReason.Should().Be("Too expensive");
        quote.RejectedAt.Should().NotBeNull();
    }

    [Fact]
    public void Quote_Accept_ThrowsIfRejected()
    {
        // Arrange
        var quote = CreateTestQuote();
        quote.Send();
        quote.Reject("Not interested");

        // Act & Assert
        var action = () => quote.Accept();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Quote_ConvertToInvoice_CreatesInvoice()
    {
        // Arrange
        var quote = CreateTestQuote();
        AddItemToQuote(quote);
        quote.Send();
        quote.Accept();

        // Act
        var invoice = quote.ConvertToInvoice(
            "INV-2024-000001",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            TestUserId);

        // Assert
        quote.Status.Should().Be(QuoteStatus.Converted);
        invoice.Should().NotBeNull();
        invoice.QuoteId.Should().Be(quote.Id);
        invoice.CustomerName.Should().Be(quote.CustomerName);
        invoice.CustomerEmail.Should().Be(quote.CustomerEmail);
    }

    [Fact]
    public void Quote_ConvertToInvoice_ThrowsIfNotAccepted()
    {
        // Arrange
        var quote = CreateTestQuote();
        quote.Send();

        // Act & Assert
        var action = () => quote.ConvertToInvoice(
            "INV-2024-000001",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            TestUserId);

        action.Should().Throw<InvalidOperationException>();
    }

    private Quote CreateTestQuote()
    {
        return new Quote(
            TestDealerId,
            "QT-2024-000001",
            TestCustomerId,
            "John Doe",
            "john@example.com",
            "Vehicle Quote",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            "MXN",
            16m,
            TestUserId);
    }

    private void AddItemToQuote(Quote quote)
    {
        var item = new QuoteItem(
            quote.Id,
            "Test Vehicle",
            1m,
            "unit",
            500000m,
            1);
        quote.AddItem(item);
    }
}
