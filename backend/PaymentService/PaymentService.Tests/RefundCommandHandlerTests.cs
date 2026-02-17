using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentService.Application.DTOs;
using PaymentService.Application.Features.Refund.Commands;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Models;
using Xunit;

namespace PaymentService.Tests;

/// <summary>
/// Tests unitarios para RefundCommandHandler
/// Verifica el comportamiento del handler de reembolsos multi-proveedor
/// </summary>
public class RefundCommandHandlerTests
{
    private readonly Mock<IAzulTransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IPaymentGatewayFactory> _gatewayFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<RefundCommandHandler>> _loggerMock;

    public RefundCommandHandlerTests()
    {
        _transactionRepositoryMock = new Mock<IAzulTransactionRepository>();
        _gatewayFactoryMock = new Mock<IPaymentGatewayFactory>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<RefundCommandHandler>>();
    }

    private RefundCommandHandler CreateHandler()
    {
        return new RefundCommandHandler(
            _transactionRepositoryMock.Object,
            _gatewayFactoryMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    #region Transaction Validation Tests

    [Fact]
    public async Task Handle_WhenTransactionNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        _transactionRepositoryMock
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AzulTransaction?)null);

        var request = CreateRefundRequest(transactionId);
        var handler = CreateHandler();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new RefundCommand(request), CancellationToken.None));
        
        exception.Message.Should().Contain("Transacción no encontrada");
    }

    [Theory]
    [InlineData(TransactionStatus.Pending)]
    [InlineData(TransactionStatus.Declined)]
    [InlineData(TransactionStatus.Voided)]
    [InlineData(TransactionStatus.Refunded)]
    public async Task Handle_WhenTransactionNotRefundable_ThrowsInvalidOperationException(TransactionStatus status)
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var originalTransaction = CreateOriginalTransaction(transactionId, status);
        
        _transactionRepositoryMock
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalTransaction);

        var request = CreateRefundRequest(transactionId);
        var handler = CreateHandler();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new RefundCommand(request), CancellationToken.None));
        
        exception.Message.Should().Contain("no puede ser reembolsada");
    }

    [Theory]
    [InlineData(TransactionStatus.Approved)]
    [InlineData(TransactionStatus.Captured)]
    public async Task Handle_WithRefundableStatus_ProcessesSuccessfully(TransactionStatus status)
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var originalTransaction = CreateOriginalTransaction(transactionId, status, gateway: "Azul");
        
        _transactionRepositoryMock
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalTransaction);

        var mockProvider = CreateMockProvider(PaymentGateway.Azul, "AZUL");
        _gatewayFactoryMock.Setup(f => f.GetProvider(PaymentGateway.Azul)).Returns(mockProvider.Object);

        var request = CreateRefundRequest(transactionId);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new RefundCommand(request), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
    }

    #endregion

    #region Gateway Detection Tests

    [Theory]
    [InlineData("Azul", PaymentGateway.Azul)]
    [InlineData("CardNET", PaymentGateway.CardNET)]
    [InlineData("PixelPay", PaymentGateway.PixelPay)]
    [InlineData("Fygaro", PaymentGateway.Fygaro)]
    [InlineData("PayPal", PaymentGateway.PayPal)]
    public async Task Handle_UsesGatewayFromOriginalTransaction(string gatewayName, PaymentGateway expectedGateway)
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var originalTransaction = CreateOriginalTransaction(transactionId, TransactionStatus.Approved, gateway: gatewayName);
        
        _transactionRepositoryMock
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalTransaction);

        var mockProvider = CreateMockProvider(expectedGateway, gatewayName);
        _gatewayFactoryMock.Setup(f => f.GetProvider(expectedGateway)).Returns(mockProvider.Object);

        var request = CreateRefundRequest(transactionId);
        var handler = CreateHandler();

        // Act
        await handler.Handle(new RefundCommand(request), CancellationToken.None);

        // Assert
        _gatewayFactoryMock.Verify(f => f.GetProvider(expectedGateway), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenGatewayFieldNull_FallsBackToAzul()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var originalTransaction = CreateOriginalTransaction(transactionId, TransactionStatus.Approved, gateway: null);
        
        _transactionRepositoryMock
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalTransaction);

        var mockProvider = CreateMockProvider(PaymentGateway.Azul, "AZUL");
        _gatewayFactoryMock.Setup(f => f.GetProvider(PaymentGateway.Azul)).Returns(mockProvider.Object);

        var request = CreateRefundRequest(transactionId);
        var handler = CreateHandler();

        // Act
        await handler.Handle(new RefundCommand(request), CancellationToken.None);

        // Assert
        _gatewayFactoryMock.Verify(f => f.GetProvider(PaymentGateway.Azul), Times.Once);
    }

    #endregion

    #region Partial Refund Tests

    [Fact]
    public async Task Handle_WithPartialAmount_UsesSpecifiedAmount()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var originalAmount = 1000m;
        var partialAmount = 500m;
        
        var originalTransaction = CreateOriginalTransaction(transactionId, TransactionStatus.Approved, gateway: "Azul", amount: originalAmount);
        
        _transactionRepositoryMock
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalTransaction);

        var mockProvider = CreateMockProvider(PaymentGateway.Azul, "AZUL");
        _gatewayFactoryMock.Setup(f => f.GetProvider(PaymentGateway.Azul)).Returns(mockProvider.Object);

        var request = CreateRefundRequest(transactionId, partialAmount: partialAmount);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new RefundCommand(request), CancellationToken.None);

        // Assert
        result.Amount.Should().Be(partialAmount);
    }

    [Fact]
    public async Task Handle_WithoutPartialAmount_UsesOriginalAmount()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var originalAmount = 1000m;
        
        var originalTransaction = CreateOriginalTransaction(transactionId, TransactionStatus.Approved, gateway: "Azul", amount: originalAmount);
        
        _transactionRepositoryMock
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalTransaction);

        var mockProvider = CreateMockProvider(PaymentGateway.Azul, "AZUL");
        _gatewayFactoryMock.Setup(f => f.GetProvider(PaymentGateway.Azul)).Returns(mockProvider.Object);

        var request = CreateRefundRequest(transactionId, partialAmount: null);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new RefundCommand(request), CancellationToken.None);

        // Assert
        result.Amount.Should().Be(originalAmount);
    }

    #endregion

    #region Repository Tests

    [Fact]
    public async Task Handle_SavesRefundTransactionToRepository()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var originalTransaction = CreateOriginalTransaction(transactionId, TransactionStatus.Approved, gateway: "Azul");
        
        _transactionRepositoryMock
            .Setup(r => r.GetByIdAsync(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalTransaction);

        var mockProvider = CreateMockProvider(PaymentGateway.Azul, "AZUL");
        _gatewayFactoryMock.Setup(f => f.GetProvider(PaymentGateway.Azul)).Returns(mockProvider.Object);

        var request = CreateRefundRequest(transactionId);
        var handler = CreateHandler();

        // Act
        await handler.Handle(new RefundCommand(request), CancellationToken.None);

        // Assert
        _transactionRepositoryMock.Verify(
            r => r.CreateAsync(It.Is<AzulTransaction>(t => 
                t.TransactionType == "Refund"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private RefundRequestDto CreateRefundRequest(Guid transactionId, decimal? partialAmount = null, string? reason = null)
    {
        return new RefundRequestDto
        {
            TransactionId = transactionId,
            PartialAmount = partialAmount,
            Reason = reason ?? "Test refund"
        };
    }

    private AzulTransaction CreateOriginalTransaction(
        Guid transactionId, 
        TransactionStatus status, 
        string? gateway = "Azul",
        decimal amount = 1000m)
    {
        return new AzulTransaction
        {
            Id = transactionId,
            UserId = Guid.NewGuid(),
            Amount = amount,
            Currency = "DOP",
            Status = status,
            Gateway = gateway,
            AzulTransactionId = $"AZL-{Guid.NewGuid():N}",
            TransactionType = "Sale",
            PaymentMethod = PaymentMethod.CreditCard,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }

    private Mock<IPaymentGatewayProvider> CreateMockProvider(
        PaymentGateway gateway,
        string name,
        bool success = true)
    {
        var mock = new Mock<IPaymentGatewayProvider>();
        mock.Setup(p => p.Name).Returns(name);
        mock.Setup(p => p.Gateway).Returns(gateway);
        mock.Setup(p => p.Type).Returns(PaymentGatewayType.Banking);

        var refundResult = new RefundResult
        {
            Success = success,
            TransactionId = $"REF-{Guid.NewGuid():N}",
            ResponseCode = success ? "00" : "05",
            Message = success ? "Refund approved" : "Refund declined",
            Amount = 1000m,
            ProcessedAt = DateTime.UtcNow
        };

        mock.Setup(p => p.RefundAsync(It.IsAny<RefundRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refundResult);

        return mock;
    }

    #endregion
}
