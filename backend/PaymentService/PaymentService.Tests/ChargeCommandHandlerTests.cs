using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentService.Application.DTOs;
using PaymentService.Application.Features.Charge.Commands;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using Xunit;

namespace PaymentService.Tests;

/// <summary>
/// Tests unitarios para ChargeCommandHandler
/// Verifica el comportamiento del handler multi-proveedor
/// </summary>
public class ChargeCommandHandlerTests
{
    private readonly Mock<IAzulTransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IPaymentGatewayFactory> _gatewayFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<ChargeCommandHandler>> _loggerMock;

    public ChargeCommandHandlerTests()
    {
        _transactionRepositoryMock = new Mock<IAzulTransactionRepository>();
        _gatewayFactoryMock = new Mock<IPaymentGatewayFactory>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<ChargeCommandHandler>>();
    }

    private ChargeCommandHandler CreateHandler()
    {
        return new ChargeCommandHandler(
            _transactionRepositoryMock.Object,
            _gatewayFactoryMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    #region Gateway Selection Tests

    [Fact]
    public async Task Handle_WithSpecificGateway_UsesRequestedProvider()
    {
        // Arrange
        var mockProvider = CreateMockProvider(PaymentGateway.CardNET, "CardNET");
        _gatewayFactoryMock.Setup(f => f.GetProvider(PaymentGateway.CardNET)).Returns(mockProvider.Object);
        
        var request = CreateChargeRequest(gateway: PaymentGateway.CardNET);
        var handler = CreateHandler();

        // Act
        await handler.Handle(new ChargeCommand(request), CancellationToken.None);

        // Assert
        _gatewayFactoryMock.Verify(f => f.GetProvider(PaymentGateway.CardNET), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutGateway_UsesDefaultFromConfiguration()
    {
        // Arrange
        _configurationMock.Setup(c => c["PaymentGateway:Default"]).Returns("PixelPay");
        
        var mockProvider = CreateMockProvider(PaymentGateway.PixelPay, "PixelPay");
        _gatewayFactoryMock.Setup(f => f.GetProvider(PaymentGateway.PixelPay)).Returns(mockProvider.Object);
        
        var request = CreateChargeRequest(gateway: null);
        var handler = CreateHandler();

        // Act
        await handler.Handle(new ChargeCommand(request), CancellationToken.None);

        // Assert
        _gatewayFactoryMock.Verify(f => f.GetProvider(PaymentGateway.PixelPay), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidDefaultConfig_FallsBackToAzul()
    {
        // Arrange
        _configurationMock.Setup(c => c["PaymentGateway:Default"]).Returns("InvalidProvider");
        
        var mockProvider = CreateMockProvider(PaymentGateway.Azul, "AZUL");
        _gatewayFactoryMock.Setup(f => f.GetProvider(PaymentGateway.Azul)).Returns(mockProvider.Object);
        
        var request = CreateChargeRequest(gateway: null);
        var handler = CreateHandler();

        // Act
        await handler.Handle(new ChargeCommand(request), CancellationToken.None);

        // Assert
        _gatewayFactoryMock.Verify(f => f.GetProvider(PaymentGateway.Azul), Times.Once);
    }

    #endregion

    #region Transaction Processing Tests

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessfulResponse()
    {
        // Arrange
        var mockProvider = CreateMockProvider(PaymentGateway.Azul, "AZUL", success: true);
        _gatewayFactoryMock.Setup(f => f.GetProvider(PaymentGateway.Azul)).Returns(mockProvider.Object);
        _configurationMock.Setup(c => c["PaymentGateway:Default"]).Returns("Azul");

        var request = CreateChargeRequest();
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new ChargeCommand(request), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.Gateway.Should().Be("Azul");
        result.ProviderName.Should().Be("AZUL");
    }

    [Fact]
    public async Task Handle_SavesTransactionToRepository()
    {
        // Arrange
        var mockProvider = CreateMockProvider(PaymentGateway.Azul, "AZUL");
        _gatewayFactoryMock.Setup(f => f.GetProvider(PaymentGateway.Azul)).Returns(mockProvider.Object);
        _configurationMock.Setup(c => c["PaymentGateway:Default"]).Returns("Azul");

        var request = CreateChargeRequest();
        var handler = CreateHandler();

        // Act
        await handler.Handle(new ChargeCommand(request), CancellationToken.None);

        // Assert
        _transactionRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<AzulTransaction>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_IncludesCommissionDataInResponse()
    {
        // Arrange
        var commission = 35m; // 3.5%
        var commissionPercentage = 3.5m;
        var amount = 1000m;
        var netAmount = 965m;

        var mockProvider = CreateMockProvider(
            PaymentGateway.Azul, 
            "AZUL", 
            success: true,
            commission: commission,
            commissionPercentage: commissionPercentage,
            netAmount: netAmount);
        
        _gatewayFactoryMock.Setup(f => f.GetProvider(PaymentGateway.Azul)).Returns(mockProvider.Object);
        _configurationMock.Setup(c => c["PaymentGateway:Default"]).Returns("Azul");

        var request = CreateChargeRequest(amount: amount);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new ChargeCommand(request), CancellationToken.None);

        // Assert
        result.Commission.Should().Be(commission);
        result.CommissionPercentage.Should().Be(commissionPercentage);
        result.NetAmount.Should().Be(netAmount);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenProviderNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        _gatewayFactoryMock
            .Setup(f => f.GetProvider(PaymentGateway.Fygaro))
            .Throws(new KeyNotFoundException("Provider Fygaro not registered"));

        var request = CreateChargeRequest(gateway: PaymentGateway.Fygaro);
        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new ChargeCommand(request), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenProviderFails_PropagatesException()
    {
        // Arrange
        var mockProvider = new Mock<IPaymentGatewayProvider>();
        mockProvider.Setup(p => p.Name).Returns("AZUL");
        mockProvider.Setup(p => p.ChargeAsync(It.IsAny<ChargeRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Payment gateway timeout"));

        _gatewayFactoryMock.Setup(f => f.GetProvider(PaymentGateway.Azul)).Returns(mockProvider.Object);
        _configurationMock.Setup(c => c["PaymentGateway:Default"]).Returns("Azul");

        var request = CreateChargeRequest();
        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(new ChargeCommand(request), CancellationToken.None));
    }

    #endregion

    #region All Providers Tests

    [Theory]
    [InlineData(PaymentGateway.Azul, "AZUL")]
    [InlineData(PaymentGateway.CardNET, "CardNET")]
    [InlineData(PaymentGateway.PixelPay, "PixelPay")]
    [InlineData(PaymentGateway.Fygaro, "Fygaro")]
    [InlineData(PaymentGateway.PayPal, "PayPal")]
    public async Task Handle_AllProviders_ProcessSuccessfully(PaymentGateway gateway, string providerName)
    {
        // Arrange
        var mockProvider = CreateMockProvider(gateway, providerName);
        _gatewayFactoryMock.Setup(f => f.GetProvider(gateway)).Returns(mockProvider.Object);

        var request = CreateChargeRequest(gateway: gateway);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new ChargeCommand(request), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Gateway.Should().Be(gateway.ToString());
        result.ProviderName.Should().Be(providerName);
    }

    #endregion

    #region Helper Methods

    private ChargeRequestDto CreateChargeRequest(
        PaymentGateway? gateway = null,
        decimal amount = 1000m)
    {
        return new ChargeRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = amount,
            Currency = "DOP",
            Description = "Test charge",
            PaymentMethod = "CreditCard",
            CardToken = "tok_test_1234567890",
            CustomerEmail = "test@example.com",
            TransactionType = "Sale",
            Gateway = gateway
        };
    }

    private Mock<IPaymentGatewayProvider> CreateMockProvider(
        PaymentGateway gateway,
        string name,
        bool success = true,
        decimal commission = 35m,
        decimal commissionPercentage = 3.5m,
        decimal netAmount = 965m)
    {
        var mock = new Mock<IPaymentGatewayProvider>();
        mock.Setup(p => p.Name).Returns(name);
        mock.Setup(p => p.Gateway).Returns(gateway);
        mock.Setup(p => p.Type).Returns(PaymentGatewayType.Banking);

        var paymentResult = new PaymentResult
        {
            Success = success,
            TransactionId = Guid.NewGuid(),
            ExternalTransactionId = $"EXT-{Guid.NewGuid():N}",
            Status = success ? TransactionStatus.Approved : TransactionStatus.Declined,
            AuthorizationCode = "AUTH123",
            ResponseCode = success ? "00" : "05",
            ResponseMessage = success ? "Approved" : "Declined",
            Commission = commission,
            CommissionPercentage = commissionPercentage,
            NetAmount = netAmount,
            CardLastFour = "1234",
            CardToken = "tok_saved_123"
        };

        mock.Setup(p => p.ChargeAsync(It.IsAny<ChargeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentResult);

        return mock;
    }

    #endregion
}
