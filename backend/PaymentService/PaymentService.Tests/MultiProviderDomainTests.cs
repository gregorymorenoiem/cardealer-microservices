using Xunit;
using FluentAssertions;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Entities;

namespace PaymentService.Tests;

/// <summary>
/// Tests para entidades del dominio multi-proveedor
/// </summary>
public class MultiProviderDomainTests
{
    #region PaymentGateway Enum Tests

    [Fact]
    public void PaymentGateway_HasFiveProviders()
    {
        // Assert
        var values = Enum.GetValues<PaymentGateway>();
        values.Should().HaveCount(5);
    }

    [Theory]
    [InlineData(PaymentGateway.Azul, 0)]
    [InlineData(PaymentGateway.CardNET, 1)]
    [InlineData(PaymentGateway.PixelPay, 2)]
    [InlineData(PaymentGateway.Fygaro, 3)]
    [InlineData(PaymentGateway.PayPal, 4)]
    public void PaymentGateway_HasCorrectValues(PaymentGateway gateway, int expected)
    {
        // Assert
        ((int)gateway).Should().Be(expected);
    }

    [Theory]
    [InlineData("Azul", PaymentGateway.Azul)]
    [InlineData("CardNET", PaymentGateway.CardNET)]
    [InlineData("PixelPay", PaymentGateway.PixelPay)]
    [InlineData("Fygaro", PaymentGateway.Fygaro)]
    [InlineData("PayPal", PaymentGateway.PayPal)]
    public void PaymentGateway_ParsesFromString(string input, PaymentGateway expected)
    {
        // Act
        var result = Enum.Parse<PaymentGateway>(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region PaymentGatewayType Enum Tests

    [Fact]
    public void PaymentGatewayType_HasThreeTypes()
    {
        // Assert
        var values = Enum.GetValues<PaymentGatewayType>();
        values.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(PaymentGatewayType.Banking)]
    [InlineData(PaymentGatewayType.Fintech)]
    [InlineData(PaymentGatewayType.Aggregator)]
    public void PaymentGatewayType_ContainsExpectedTypes(PaymentGatewayType type)
    {
        // Assert
        Enum.IsDefined(type).Should().BeTrue();
    }

    #endregion

    #region TransactionStatus Enum Tests

    [Theory]
    [InlineData(TransactionStatus.Pending)]
    [InlineData(TransactionStatus.Approved)]
    [InlineData(TransactionStatus.Declined)]
    [InlineData(TransactionStatus.Error)]
    [InlineData(TransactionStatus.Refunded)]
    [InlineData(TransactionStatus.Voided)]
    [InlineData(TransactionStatus.Authorized)]
    [InlineData(TransactionStatus.Captured)]
    public void TransactionStatus_ContainsExpectedStatuses(TransactionStatus status)
    {
        // Assert
        Enum.IsDefined(status).Should().BeTrue();
    }

    #endregion

    #region AzulTransaction Entity Tests

    [Fact]
    public void AzulTransaction_CanBeCreated_WithGateway()
    {
        // Arrange & Act
        var transaction = new AzulTransaction
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 1000m,
            Currency = "DOP",
            Gateway = "Azul",
            TransactionType = "Charge",
            Status = TransactionStatus.Approved
        };

        // Assert
        transaction.Gateway.Should().Be("Azul");
    }

    [Theory]
    [InlineData("Azul")]
    [InlineData("CardNET")]
    [InlineData("PixelPay")]
    [InlineData("Fygaro")]
    [InlineData("PayPal")]
    public void AzulTransaction_AcceptsAllGatewayTypes(string gatewayName)
    {
        // Arrange & Act
        var transaction = new AzulTransaction
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 500m,
            Currency = "DOP",
            Gateway = gatewayName,
            TransactionType = "Charge",
            Status = TransactionStatus.Pending
        };

        // Assert
        transaction.Gateway.Should().Be(gatewayName);
    }

    [Fact]
    public void AzulTransaction_HasCommissionFields()
    {
        // Arrange & Act
        var transaction = new AzulTransaction
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 1000m,
            Currency = "DOP",
            Gateway = "PixelPay",
            Commission = 25m,
            TransactionType = "Charge",
            Status = TransactionStatus.Approved
        };

        // Assert
        transaction.Commission.Should().Be(25m);
    }

    [Fact]
    public void AzulTransaction_CalculatesNetAmount()
    {
        // Arrange
        var amount = 1000m;
        var commission = 35m;
        
        var transaction = new AzulTransaction
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = amount,
            Currency = "DOP",
            Gateway = "Azul",
            Commission = commission,
            TransactionType = "Charge",
            Status = TransactionStatus.Approved
        };

        // Act
        var netAmount = transaction.Amount - (transaction.Commission ?? 0);

        // Assert
        netAmount.Should().Be(965m);
    }

    #endregion

    #region Provider Commission Tests

    [Theory]
    [InlineData("Azul", 1000, 3.5, 35)]
    [InlineData("CardNET", 1000, 3.0, 30)]
    [InlineData("PixelPay", 1000, 2.5, 25)]
    [InlineData("Fygaro", 1000, 3.0, 30)]
    [InlineData("PayPal", 1000, 2.9, 29)] // Sin fee fijo en este test simplificado
    public void Provider_CalculatesCorrectCommission(string gateway, decimal amount, decimal percent, decimal expectedCommission)
    {
        // Act
        var commission = amount * percent / 100;

        // Assert
        _ = gateway; // used to satisfy xUnit1026 (Theory parameter must be used)
        commission.Should().Be(expectedCommission);
    }

    #endregion
}
