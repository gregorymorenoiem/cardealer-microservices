using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Services;

namespace PaymentService.Tests;

/// <summary>
/// Tests para PaymentGatewayRegistry multi-proveedor
/// </summary>
public class PaymentGatewayRegistryTests
{
    private readonly Mock<ILogger<PaymentGatewayRegistry>> _mockLogger;

    public PaymentGatewayRegistryTests()
    {
        _mockLogger = new Mock<ILogger<PaymentGatewayRegistry>>();
    }

    [Fact]
    public void Register_WithValidProvider_AddsToRegistry()
    {
        // Arrange
        var registry = new PaymentGatewayRegistry(_mockLogger.Object);
        var mockProvider = CreateMockProvider(PaymentGateway.Azul);

        // Act
        registry.Register(mockProvider.Object);

        // Assert
        registry.Contains(PaymentGateway.Azul).Should().BeTrue();
        registry.Count().Should().Be(1);
    }

    [Fact]
    public void Register_WithDuplicateGateway_OverwritesPrevious()
    {
        // Arrange
        var registry = new PaymentGatewayRegistry(_mockLogger.Object);
        var provider1 = CreateMockProvider(PaymentGateway.Azul, "AZUL v1");
        var provider2 = CreateMockProvider(PaymentGateway.Azul, "AZUL v2");

        // Act
        registry.Register(provider1.Object);
        registry.Register(provider2.Object);

        // Assert
        registry.Count().Should().Be(1);
        var retrieved = registry.Get(PaymentGateway.Azul);
        retrieved!.Name.Should().Be("AZUL v2");
    }

    [Fact]
    public void Get_WithUnregisteredGateway_ReturnsNull()
    {
        // Arrange
        var registry = new PaymentGatewayRegistry(_mockLogger.Object);

        // Act
        var result = registry.Get(PaymentGateway.PayPal);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_ReturnsAllRegisteredProviders()
    {
        // Arrange
        var registry = new PaymentGatewayRegistry(_mockLogger.Object);
        registry.Register(CreateMockProvider(PaymentGateway.Azul).Object);
        registry.Register(CreateMockProvider(PaymentGateway.CardNET).Object);
        registry.Register(CreateMockProvider(PaymentGateway.PixelPay).Object);
        registry.Register(CreateMockProvider(PaymentGateway.Fygaro).Object);
        registry.Register(CreateMockProvider(PaymentGateway.PayPal).Object);

        // Act
        var all = registry.GetAll();

        // Assert
        all.Should().HaveCount(5);
    }

    [Fact]
    public void GetAll_WhenEmpty_ReturnsEmptyList()
    {
        // Arrange
        var registry = new PaymentGatewayRegistry(_mockLogger.Object);

        // Act
        var all = registry.GetAll();

        // Assert
        all.Should().BeEmpty();
    }

    [Fact]
    public void Contains_WithRegisteredGateway_ReturnsTrue()
    {
        // Arrange
        var registry = new PaymentGatewayRegistry(_mockLogger.Object);
        registry.Register(CreateMockProvider(PaymentGateway.Azul).Object);

        // Act
        var result = registry.Contains(PaymentGateway.Azul);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithUnregisteredGateway_ReturnsFalse()
    {
        // Arrange
        var registry = new PaymentGatewayRegistry(_mockLogger.Object);

        // Act
        var result = registry.Contains(PaymentGateway.PayPal);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Unregister_RemovesProvider()
    {
        // Arrange
        var registry = new PaymentGatewayRegistry(_mockLogger.Object);
        registry.Register(CreateMockProvider(PaymentGateway.Azul).Object);

        // Act
        var removed = registry.Unregister(PaymentGateway.Azul);

        // Assert
        removed.Should().BeTrue();
        registry.Contains(PaymentGateway.Azul).Should().BeFalse();
        registry.Count().Should().Be(0);
    }

    [Fact]
    public void Count_ReturnsCorrectNumber()
    {
        // Arrange
        var registry = new PaymentGatewayRegistry(_mockLogger.Object);
        registry.Register(CreateMockProvider(PaymentGateway.Azul).Object);
        registry.Register(CreateMockProvider(PaymentGateway.CardNET).Object);

        // Act
        var count = registry.Count();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void Clear_RemovesAllProviders()
    {
        // Arrange
        var registry = new PaymentGatewayRegistry(_mockLogger.Object);
        registry.Register(CreateMockProvider(PaymentGateway.Azul).Object);
        registry.Register(CreateMockProvider(PaymentGateway.CardNET).Object);

        // Act
        registry.Clear();

        // Assert
        registry.Count().Should().Be(0);
    }

    [Fact]
    public void Register_WithNullProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = new PaymentGatewayRegistry(_mockLogger.Object);

        // Act
        Action act = () => registry.Register(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private Mock<IPaymentGatewayProvider> CreateMockProvider(PaymentGateway gateway, string? name = null)
    {
        var mock = new Mock<IPaymentGatewayProvider>();
        mock.Setup(p => p.Gateway).Returns(gateway);
        mock.Setup(p => p.Name).Returns(name ?? gateway.ToString());
        mock.Setup(p => p.Type).Returns(PaymentGatewayType.Banking);
        return mock;
    }
}
