using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentService.Api.Controllers;
using PaymentService.Application.DTOs;
using PaymentService.Application.Features.Charge.Commands;
using PaymentService.Application.Features.Refund.Commands;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using Xunit;

namespace PaymentService.Tests;

/// <summary>
/// Tests unitarios para PaymentsController
/// Verifica el comportamiento del controller multi-proveedor
/// </summary>
public class PaymentsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IPaymentGatewayRegistry> _registryMock;
    private readonly Mock<ILogger<PaymentsController>> _loggerMock;

    public PaymentsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _registryMock = new Mock<IPaymentGatewayRegistry>();
        _loggerMock = new Mock<ILogger<PaymentsController>>();
    }

    private PaymentsController CreateController()
    {
        var controller = new PaymentsController(
            _mediatorMock.Object,
            _registryMock.Object,
            _loggerMock.Object);
        
        // Setup HttpContext
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        
        return controller;
    }

    #region GetProviders Tests

    [Fact]
    public void GetProviders_ReturnsAllRegisteredProviders()
    {
        // Arrange
        var providers = new List<IPaymentGatewayProvider>
        {
            CreateMockProvider(PaymentGateway.Azul, "AZUL", PaymentGatewayType.Banking).Object,
            CreateMockProvider(PaymentGateway.CardNET, "CardNET", PaymentGatewayType.Banking).Object,
            CreateMockProvider(PaymentGateway.PixelPay, "PixelPay", PaymentGatewayType.Fintech).Object,
            CreateMockProvider(PaymentGateway.Fygaro, "Fygaro", PaymentGatewayType.Aggregator).Object,
            CreateMockProvider(PaymentGateway.PayPal, "PayPal", PaymentGatewayType.Fintech).Object
        };
        
        _registryMock.Setup(r => r.GetAll()).Returns(providers);
        
        var controller = CreateController();

        // Act
        var result = controller.GetProviders();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var providersDto = okResult.Value.Should().BeOfType<ProvidersListDto>().Subject;
        
        providersDto.TotalProviders.Should().Be(5);
        providersDto.Providers.Should().HaveCount(5);
    }

    [Fact]
    public void GetProviders_WhenEmpty_ReturnsEmptyList()
    {
        // Arrange
        _registryMock.Setup(r => r.GetAll()).Returns(new List<IPaymentGatewayProvider>());
        
        var controller = CreateController();

        // Act
        var result = controller.GetProviders();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var providersDto = okResult.Value.Should().BeOfType<ProvidersListDto>().Subject;
        
        providersDto.TotalProviders.Should().Be(0);
        providersDto.Providers.Should().BeEmpty();
    }

    [Fact]
    public void GetProviders_IncludesConfigurationStatus()
    {
        // Arrange
        var azulProvider = CreateMockProvider(PaymentGateway.Azul, "AZUL", PaymentGatewayType.Banking);
        azulProvider.Setup(p => p.ValidateConfiguration()).Returns(new List<string>());
        
        var unconfiguredProvider = CreateMockProvider(PaymentGateway.CardNET, "CardNET", PaymentGatewayType.Banking);
        unconfiguredProvider.Setup(p => p.ValidateConfiguration()).Returns(new List<string> { "Missing API Key" });
        
        _registryMock.Setup(r => r.GetAll()).Returns(new List<IPaymentGatewayProvider> 
        { 
            azulProvider.Object, 
            unconfiguredProvider.Object 
        });
        
        var controller = CreateController();

        // Act
        var result = controller.GetProviders();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var providersDto = okResult.Value.Should().BeOfType<ProvidersListDto>().Subject;
        
        providersDto.Providers.First(p => p.Gateway == "Azul").IsConfigured.Should().BeTrue();
        providersDto.Providers.First(p => p.Gateway == "CardNET").IsConfigured.Should().BeFalse();
    }

    #endregion

    #region GetProvider Tests

    [Fact]
    public void GetProvider_WithValidGateway_ReturnsProviderInfo()
    {
        // Arrange
        var provider = CreateMockProvider(PaymentGateway.Azul, "AZUL", PaymentGatewayType.Banking);
        provider.Setup(p => p.ValidateConfiguration()).Returns(new List<string>());
        
        _registryMock.Setup(r => r.Get(PaymentGateway.Azul)).Returns(provider.Object);
        
        var controller = CreateController();

        // Act
        var result = controller.GetProvider("Azul");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var providerInfo = okResult.Value.Should().BeOfType<ProviderInfoDto>().Subject;
        
        providerInfo.Gateway.Should().Be("Azul");
        providerInfo.Name.Should().Be("AZUL");
        providerInfo.Type.Should().Be("Banking");
        providerInfo.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public void GetProvider_WithInvalidGateway_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetProvider("InvalidGateway");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void GetProvider_WithUnregisteredGateway_ReturnsNotFound()
    {
        // Arrange
        _registryMock.Setup(r => r.Get(PaymentGateway.PayPal)).Returns((IPaymentGatewayProvider?)null);
        
        var controller = CreateController();

        // Act
        var result = controller.GetProvider("PayPal");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Theory]
    [InlineData("azul")]
    [InlineData("AZUL")]
    [InlineData("Azul")]
    public void GetProvider_IsCaseInsensitive(string gateway)
    {
        // Arrange
        var provider = CreateMockProvider(PaymentGateway.Azul, "AZUL", PaymentGatewayType.Banking);
        provider.Setup(p => p.ValidateConfiguration()).Returns(new List<string>());
        
        _registryMock.Setup(r => r.Get(PaymentGateway.Azul)).Returns(provider.Object);
        
        var controller = CreateController();

        // Act
        var result = controller.GetProvider(gateway);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region CheckProviderHealth Tests

    [Fact]
    public async Task CheckProviderHealth_WhenProviderAvailable_ReturnsHealthy()
    {
        // Arrange
        var provider = CreateMockProvider(PaymentGateway.Azul, "AZUL", PaymentGatewayType.Banking);
        provider.Setup(p => p.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        _registryMock.Setup(r => r.Get(PaymentGateway.Azul)).Returns(provider.Object);
        
        var controller = CreateController();

        // Act
        var result = await controller.CheckProviderHealth("Azul", CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var healthDto = okResult.Value.Should().BeOfType<ProviderHealthDto>().Subject;
        
        healthDto.Gateway.Should().Be("Azul");
        healthDto.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task CheckProviderHealth_WhenProviderUnavailable_ReturnsUnhealthy()
    {
        // Arrange
        var provider = CreateMockProvider(PaymentGateway.Azul, "AZUL", PaymentGatewayType.Banking);
        provider.Setup(p => p.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        
        _registryMock.Setup(r => r.Get(PaymentGateway.Azul)).Returns(provider.Object);
        
        var controller = CreateController();

        // Act
        var result = await controller.CheckProviderHealth("Azul", CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var healthDto = okResult.Value.Should().BeOfType<ProviderHealthDto>().Subject;
        
        healthDto.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task CheckProviderHealth_WithInvalidGateway_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.CheckProviderHealth("InvalidGateway", CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Charge Tests

    [Fact]
    public async Task Charge_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var chargeResponse = new ChargeResponseDto
        {
            TransactionId = Guid.NewGuid(),
            IsSuccessful = true,
            Status = "Approved",
            Gateway = "Azul",
            ProviderName = "AZUL",
            Amount = 1000m,
            Currency = "DOP"
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ChargeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chargeResponse);
        
        var controller = CreateController();
        var request = new ChargeRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 1000m,
            Currency = "DOP",
            CardToken = "tok_test",
            Gateway = PaymentGateway.Azul
        };

        // Act
        var result = await controller.ProcessCharge(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ChargeResponseDto>().Subject;
        
        response.IsSuccessful.Should().BeTrue();
        response.Gateway.Should().Be("Azul");
    }

    [Fact]
    public async Task Charge_CallsMediatorWithCorrectGateway()
    {
        // Arrange
        ChargeCommand? capturedCommand = null;
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ChargeCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<ChargeResponseDto>, CancellationToken>((cmd, ct) => capturedCommand = cmd as ChargeCommand)
            .ReturnsAsync(new ChargeResponseDto());
        
        var controller = CreateController();
        var request = new ChargeRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 1000m,
            Currency = "DOP",
            CardToken = "tok_test",
            Gateway = PaymentGateway.PixelPay
        };

        // Act
        await controller.ProcessCharge(request, CancellationToken.None);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.ChargeRequest.Gateway.Should().Be(PaymentGateway.PixelPay);
    }

    #endregion

    #region Refund Tests

    [Fact]
    public async Task Refund_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var refundResponse = new ChargeResponseDto
        {
            TransactionId = Guid.NewGuid(),
            IsSuccessful = true,
            Status = "Refunded",
            Amount = 1000m
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RefundCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refundResponse);
        
        var controller = CreateController();
        var request = new RefundRequestDto
        {
            TransactionId = Guid.NewGuid(),
            Reason = "Customer request"
        };

        // Act
        var result = await controller.ProcessRefund(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ChargeResponseDto>().Subject;
        
        response.IsSuccessful.Should().BeTrue();
        response.Status.Should().Be("Refunded");
    }

    #endregion

    #region Helper Methods

    private Mock<IPaymentGatewayProvider> CreateMockProvider(
        PaymentGateway gateway,
        string name,
        PaymentGatewayType type)
    {
        var mock = new Mock<IPaymentGatewayProvider>();
        mock.Setup(p => p.Gateway).Returns(gateway);
        mock.Setup(p => p.Name).Returns(name);
        mock.Setup(p => p.Type).Returns(type);
        mock.Setup(p => p.ValidateConfiguration()).Returns(new List<string>());
        mock.Setup(p => p.IsAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        return mock;
    }

    #endregion
}
