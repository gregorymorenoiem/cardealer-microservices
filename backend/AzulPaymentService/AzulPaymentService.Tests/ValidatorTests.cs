using Xunit;
using FluentAssertions;
using AzulPaymentService.Application.DTOs;
using AzulPaymentService.Application.Validators;
using FluentValidation.TestHelper;

namespace AzulPaymentService.Tests;

/// <summary>
/// Tests para validación de requests
/// </summary>
public class ValidatorTests
{
    [Fact]
    public void ChargeRequestValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var validator = new ChargeRequestValidator();
        var request = new ChargeRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 1000m,
            Currency = "DOP",
            Description = "Test",
            CardNumber = "4111111111111111",
            CardExpiryMonth = "12",
            CardExpiryYear = "2025",
            CardCVV = "123"
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ChargeRequestValidator_WithNegativeAmount_ShouldFail()
    {
        // Arrange
        var validator = new ChargeRequestValidator();
        var request = new ChargeRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = -100m,
            Currency = "DOP"
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ChargeRequestValidator_WithoutCardData_ShouldFail()
    {
        // Arrange
        var validator = new ChargeRequestValidator();
        var request = new ChargeRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "DOP"
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CardNumber);
    }

    [Fact]
    public void RefundRequestValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var validator = new RefundRequestValidator();
        var request = new RefundRequestDto
        {
            TransactionId = Guid.NewGuid(),
            Reason = "Customer requested refund"
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RefundRequestValidator_WithoutReason_ShouldFail()
    {
        // Arrange
        var validator = new RefundRequestValidator();
        var request = new RefundRequestDto
        {
            TransactionId = Guid.NewGuid(),
            Reason = string.Empty
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void SubscriptionRequestValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var validator = new SubscriptionRequestValidator();
        var request = new SubscriptionRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 500m,
            Currency = "DOP",
            Frequency = "Monthly",
            StartDate = DateTime.UtcNow.AddDays(1),
            CardNumber = "4111111111111111",
            CardExpiryMonth = "12",
            CardExpiryYear = "2025",
            CardCVV = "123"
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SubscriptionRequestValidator_WithInvalidFrequency_ShouldFail()
    {
        // Arrange
        var validator = new SubscriptionRequestValidator();
        var request = new SubscriptionRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 500m,
            Frequency = "InvalidFrequency",
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Frequency);
    }

    [Fact]
    public void SubscriptionRequestValidator_WithPastStartDate_ShouldFail()
    {
        // Arrange
        var validator = new SubscriptionRequestValidator();
        var request = new SubscriptionRequestDto
        {
            UserId = Guid.NewGuid(),
            Amount = 500m,
            StartDate = DateTime.UtcNow.AddDays(-1),
            Frequency = "Monthly"
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }
}
