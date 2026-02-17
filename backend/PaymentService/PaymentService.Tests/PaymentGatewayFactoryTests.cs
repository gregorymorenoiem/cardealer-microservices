using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Services;

namespace PaymentService.Tests;

/// <summary>
/// Tests para PaymentGatewayFactory multi-proveedor
/// </summary>
public class PaymentGatewayFactoryTests
{
    private readonly Mock<IPaymentGatewayRegistry> _mockRegistry;
    private readonly Mock<ILogger<PaymentGatewayFactory>> _mockLogger;
    private readonly IConfiguration _configuration;

    public PaymentGatewayFactoryTests()
    {
        _mockRegistry = new Mock<IPaymentGatewayRegistry>();
        _mockLogger = new Mock<ILogger<PaymentGatewayFactory>>();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "PaymentGateway:Default", "Azul" }
            })
            .Build();
    }

    [Fact]
    public void GetProvider_WithValidGateway_ReturnsProvider()
    {
        // Arrange
        var mockProvider = CreateMockProvider(PaymentGateway.Azul);
        _mockRegistry.Setup(r => r.Get(PaymentGateway.Azul)).Returns(mockProvider.Object);

        var factory = new PaymentGatewayFactory(_mockRegistry.Object, _mockLogger.Object, _configuration);

        // Act
        var provider = factory.GetProvider(PaymentGateway.Azul);

        // Assert
        provider.Should().NotBeNull();
        provider.Gateway.Should().Be(PaymentGateway.Azul);
    }

    [Fact]
    public void GetProvider_WithUnregisteredGateway_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockRegistry.Setup(r => r.Get(PaymentGateway.PayPal)).Returns((IPaymentGatewayProvider?)null);

        var factory = new PaymentGatewayFactory(_mockRegistry.Object, _mockLogger.Object, _configuration);

        // Act
        Action act = () => factory.GetProvider(PaymentGateway.PayPal);

        // Assert
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*PayPal*");
    }

    [Fact]
    public void GetDefaultProvider_ReturnsConfiguredDefault()
    {
        // Arrange
        var mockProvider = CreateMockProvider(PaymentGateway.Azul);
        _mockRegistry.Setup(r => r.Get(PaymentGateway.Azul)).Returns(mockProvider.Object);

        var factory = new PaymentGatewayFactory(_mockRegistry.Object, _mockLogger.Object, _configuration);

        // Act
        var provider = factory.GetDefaultProvider();

        // Assert
        provider.Should().NotBeNull();
        provider.Gateway.Should().Be(PaymentGateway.Azul);
    }

    [Fact]
    public void GetAllProviders_ReturnsAllRegisteredProviders()
    {
        // Arrange
        var providers = new List<IPaymentGatewayProvider>
        {
            CreateMockProvider(PaymentGateway.Azul).Object,
            CreateMockProvider(PaymentGateway.CardNET).Object,
            CreateMockProvider(PaymentGateway.PixelPay).Object,
            CreateMockProvider(PaymentGateway.Fygaro).Object,
            CreateMockProvider(PaymentGateway.PayPal).Object
        };
        _mockRegistry.Setup(r => r.GetAll()).Returns(providers.AsReadOnly());

        var factory = new PaymentGatewayFactory(_mockRegistry.Object, _mockLogger.Object, _configuration);

        // Act
        var result = factory.GetAllProviders();

        // Assert
        result.Should().HaveCount(5);
    }

    [Theory]
    [InlineData(PaymentGateway.Azul)]
    [InlineData(PaymentGateway.CardNET)]
    [InlineData(PaymentGateway.PixelPay)]
    public void IsProviderAvailable_WithValidProvider_ReturnsTrue(PaymentGateway gateway)
    {
        // Arrange
        var mockProvider = CreateMockProvider(gateway);
        mockProvider.Setup(p => p.ValidateConfiguration()).Returns(new List<string>());
        _mockRegistry.Setup(r => r.Get(gateway)).Returns(mockProvider.Object);

        var factory = new PaymentGatewayFactory(_mockRegistry.Object, _mockLogger.Object, _configuration);

        // Act
        var isAvailable = factory.IsProviderAvailable(gateway);

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public void IsProviderAvailable_WithConfigErrors_ReturnsFalse()
    {
        // Arrange
        var mockProvider = CreateMockProvider(PaymentGateway.PayPal);
        mockProvider.Setup(p => p.ValidateConfiguration()).Returns(new List<string> { "Missing API Key" });
        _mockRegistry.Setup(r => r.Get(PaymentGateway.PayPal)).Returns(mockProvider.Object);

        var factory = new PaymentGatewayFactory(_mockRegistry.Object, _mockLogger.Object, _configuration);

        // Act
        var isAvailable = factory.IsProviderAvailable(PaymentGateway.PayPal);

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithCustomDefaultGateway_UsesConfigured()
    {
        // Arrange
        var customConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "PaymentGateway:Default", "PixelPay" }
            })
            .Build();

        var mockProvider = CreateMockProvider(PaymentGateway.PixelPay);
        _mockRegistry.Setup(r => r.Get(PaymentGateway.PixelPay)).Returns(mockProvider.Object);

        var factory = new PaymentGatewayFactory(_mockRegistry.Object, _mockLogger.Object, customConfig);

        // Act
        var provider = factory.GetDefaultProvider();

        // Assert
        provider.Gateway.Should().Be(PaymentGateway.PixelPay);
    }

    private Mock<IPaymentGatewayProvider> CreateMockProvider(PaymentGateway gateway)
    {
        var mock = new Mock<IPaymentGatewayProvider>();
        mock.Setup(p => p.Gateway).Returns(gateway);
        mock.Setup(p => p.Name).Returns(gateway.ToString());
        mock.Setup(p => p.Type).Returns(PaymentGatewayType.Banking);
        mock.Setup(p => p.ValidateConfiguration()).Returns(new List<string>());
        return mock;
    }
}
