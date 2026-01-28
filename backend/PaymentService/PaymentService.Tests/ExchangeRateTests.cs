using FluentAssertions;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using Xunit;

namespace PaymentService.Tests;

/// <summary>
/// Tests para la entidad ExchangeRate y conversiones de moneda
/// Validación de cumplimiento DGII
/// </summary>
public class ExchangeRateTests
{
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.Now);

    [Fact]
    public void ExchangeRate_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var rate = new ExchangeRate
        {
            Id = Guid.NewGuid(),
            RateDate = _today,
            SourceCurrency = "USD",
            TargetCurrency = "DOP",
            BuyRate = 58.50m,
            SellRate = 59.00m,
            Source = ExchangeRateSource.BancoCentralApi,
            IsActive = true
        };

        // Assert
        rate.SourceCurrency.Should().Be("USD");
        rate.TargetCurrency.Should().Be("DOP");
        rate.BuyRate.Should().Be(58.50m);
        rate.SellRate.Should().Be(59.00m);
        rate.Source.Should().Be(ExchangeRateSource.BancoCentralApi);
        rate.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(100, 58.50, 5850.00)]  // $100 USD = 5,850 DOP
    [InlineData(250, 58.50, 14625.00)] // $250 USD = 14,625 DOP
    [InlineData(1000, 58.50, 58500.00)] // $1,000 USD = 58,500 DOP
    [InlineData(0.01, 58.50, 0.59)]    // $0.01 USD = 0.59 DOP (redondeo)
    public void ConvertToDop_ShouldCalculateCorrectly(decimal usd, decimal rate, decimal expectedDop)
    {
        // Arrange
        var exchangeRate = new ExchangeRate
        {
            RateDate = _today,
            SourceCurrency = "USD",
            BuyRate = rate,
            SellRate = rate + 0.50m
        };

        // Act
        var result = exchangeRate.ConvertToDop(usd);

        // Assert
        result.Should().Be(expectedDop);
    }

    [Theory]
    [InlineData(5850, 59.00, 99.15)]   // 5,850 DOP ≈ $99.15 USD
    [InlineData(59000, 59.00, 1000.00)] // 59,000 DOP = $1,000 USD
    public void ConvertFromDop_ShouldCalculateCorrectly(decimal dop, decimal rate, decimal expectedUsd)
    {
        // Arrange
        var exchangeRate = new ExchangeRate
        {
            RateDate = _today,
            SourceCurrency = "USD",
            BuyRate = rate - 0.50m,
            SellRate = rate
        };

        // Act
        var result = exchangeRate.ConvertFromDop(dop);

        // Assert
        result.Should().Be(expectedUsd);
    }

    [Fact]
    public void ConvertToDop_WithZeroAmount_ShouldReturnZero()
    {
        // Arrange
        var rate = new ExchangeRate { BuyRate = 58.50m };

        // Act
        var result = rate.ConvertToDop(0);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ConvertToDop_WithNegativeAmount_ShouldReturnZero()
    {
        // Arrange
        var rate = new ExchangeRate { BuyRate = 58.50m };

        // Act
        var result = rate.ConvertToDop(-100);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ConvertFromDop_WithZeroRate_ShouldReturnZero()
    {
        // Arrange
        var rate = new ExchangeRate { SellRate = 0 };

        // Act
        var result = rate.ConvertFromDop(5850);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ExchangeRateSource_ShouldHaveCorrectValues()
    {
        // Assert - Verificar que las fuentes oficiales existen
        ExchangeRateSource.BancoCentralApi.Should().Be((ExchangeRateSource)1);
        ExchangeRateSource.BancoCentralWebScrape.Should().Be((ExchangeRateSource)2);
        ExchangeRateSource.CachedPreviousDay.Should().Be((ExchangeRateSource)3);
        ExchangeRateSource.ManualEntry.Should().Be((ExchangeRateSource)4);
        ExchangeRateSource.ExternalProvider.Should().Be((ExchangeRateSource)5);
    }
}

/// <summary>
/// Tests para CurrencyConversion (registro de auditoría DGII)
/// </summary>
public class CurrencyConversionTests
{
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.Now);

    [Fact]
    public void CurrencyConversion_Create_ShouldCalculateCorrectly()
    {
        // Arrange
        var rate = new ExchangeRate
        {
            Id = Guid.NewGuid(),
            RateDate = _today,
            SourceCurrency = "USD",
            BuyRate = 58.50m,
            SellRate = 59.00m,
            Source = ExchangeRateSource.BancoCentralApi
        };
        var transactionId = Guid.NewGuid();
        decimal originalAmount = 100m; // $100 USD

        // Act
        var conversion = CurrencyConversion.Create(
            transactionId,
            rate,
            originalAmount,
            "USD",
            ConversionType.Purchase);

        // Assert
        conversion.PaymentTransactionId.Should().Be(transactionId);
        conversion.OriginalAmount.Should().Be(100m);
        conversion.OriginalCurrency.Should().Be("USD");
        conversion.ConvertedAmountDop.Should().Be(5850m); // 100 * 58.50
        conversion.AppliedRate.Should().Be(58.50m);
        conversion.RateSource.Should().Be(ExchangeRateSource.BancoCentralApi);
    }

    [Fact]
    public void CurrencyConversion_ShouldCalculateItbis_18Percent()
    {
        // Arrange
        var rate = new ExchangeRate
        {
            Id = Guid.NewGuid(),
            RateDate = _today,
            SourceCurrency = "USD",
            BuyRate = 58.50m,
            SellRate = 59.00m,
            Source = ExchangeRateSource.BancoCentralApi
        };

        // Act
        var conversion = CurrencyConversion.Create(
            Guid.NewGuid(),
            rate,
            100m, // $100 USD
            "USD",
            ConversionType.Purchase);

        // Assert - ITBIS debe ser 18% del monto en DOP
        var expectedItbis = Math.Round(5850m * 0.18m, 2);
        conversion.ItbisDop.Should().Be(expectedItbis); // 1053.00
        conversion.TotalWithItbisDop.Should().Be(5850m + expectedItbis); // 6903.00
    }

    [Theory]
    [InlineData(100, 58.50, 5850, 1053)]    // $100 → 5,850 DOP → ITBIS 1,053
    [InlineData(500, 58.50, 29250, 5265)]   // $500 → 29,250 DOP → ITBIS 5,265
    [InlineData(1000, 58.50, 58500, 10530)] // $1,000 → 58,500 DOP → ITBIS 10,530
    public void CurrencyConversion_ItbisCalculation_ShouldBeAccurate(
        decimal usdAmount, decimal rate, decimal expectedDop, decimal expectedItbis)
    {
        // Arrange
        var exchangeRate = new ExchangeRate
        {
            Id = Guid.NewGuid(),
            RateDate = DateOnly.FromDateTime(DateTime.Now),
            SourceCurrency = "USD",
            BuyRate = rate,
            SellRate = rate + 0.50m,
            Source = ExchangeRateSource.BancoCentralApi
        };

        // Act
        var conversion = CurrencyConversion.Create(
            Guid.NewGuid(),
            exchangeRate,
            usdAmount,
            "USD",
            ConversionType.Purchase);

        // Assert
        conversion.ConvertedAmountDop.Should().Be(expectedDop);
        conversion.ItbisDop.Should().Be(expectedItbis);
        conversion.TotalWithItbisDop.Should().Be(expectedDop + expectedItbis);
    }

    [Fact]
    public void ConversionType_ShouldHaveCorrectValues()
    {
        // Assert
        ConversionType.Purchase.Should().Be((ConversionType)1);
        ConversionType.Refund.Should().Be((ConversionType)2);
        ConversionType.Quote.Should().Be((ConversionType)3);
    }

    [Fact]
    public void CurrencyConversion_ForEUR_ShouldWorkCorrectly()
    {
        // Arrange - Tasa EUR más alta que USD típicamente
        var rate = new ExchangeRate
        {
            Id = Guid.NewGuid(),
            RateDate = DateOnly.FromDateTime(DateTime.Now),
            SourceCurrency = "EUR",
            BuyRate = 63.50m, // EUR típicamente más fuerte que USD
            SellRate = 64.00m,
            Source = ExchangeRateSource.BancoCentralApi
        };

        // Act
        var conversion = CurrencyConversion.Create(
            Guid.NewGuid(),
            rate,
            100m, // €100 EUR
            "EUR",
            ConversionType.Purchase);

        // Assert
        conversion.OriginalCurrency.Should().Be("EUR");
        conversion.ConvertedAmountDop.Should().Be(6350m); // 100 * 63.50
        conversion.ItbisDop.Should().Be(1143m); // 6350 * 0.18 = 1143
    }
}

/// <summary>
/// Tests para ConversionResult
/// </summary>
public class ConversionResultTests
{
    [Fact]
    public void ConversionResult_Successful_ShouldSetAllProperties()
    {
        // Arrange
        var rate = new ExchangeRate
        {
            Id = Guid.NewGuid(),
            RateDate = DateOnly.FromDateTime(DateTime.Now),
            SourceCurrency = "USD",
            BuyRate = 58.50m,
            SellRate = 59.00m,
            Source = ExchangeRateSource.BancoCentralApi
        };

        // Act
        var result = ConversionResult.Successful(100m, "USD", rate);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.OriginalAmount.Should().Be(100m);
        result.OriginalCurrency.Should().Be("USD");
        result.ConvertedAmountDop.Should().Be(5850m);
        result.AppliedRate.Should().Be(58.50m);
        result.RateSource.Should().Be("BancoCentralApi");
        result.ExchangeRateId.Should().Be(rate.Id);
    }

    [Fact]
    public void ConversionResult_Failed_ShouldSetErrorMessage()
    {
        // Act
        var result = ConversionResult.Failed("No se pudo obtener tasa de cambio");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("No se pudo obtener tasa de cambio");
        result.ConvertedAmountDop.Should().Be(0);
    }
}
