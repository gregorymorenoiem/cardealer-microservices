using Xunit;

using FluentAssertions;
using ReviewService.Domain.Entities;

namespace ReviewService.Tests.Domain.Entities;

/// <summary>
/// Tests para ReviewRequest entity
/// </summary>
public class ReviewRequestTests
{
    [Fact]
    public void ReviewRequest_ShouldBeCreatedSuccessfully_WithValidData()
    {
        // Arrange & Act
        var request = new ReviewRequest
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            BuyerEmail = "buyer@example.com",
            VehicleMake = "Toyota",
            VehicleModel = "Camry",
            RequestToken = "abc123def456",
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsCompleted = false,
            RemindersSent = 0
        };

        // Assert
        request.Id.Should().NotBe(Guid.Empty);
        request.SellerId.Should().NotBe(Guid.Empty);
        request.BuyerId.Should().NotBe(Guid.Empty);
        request.VehicleId.Should().NotBe(Guid.Empty);
        request.OrderId.Should().NotBe(Guid.Empty);
        request.BuyerEmail.Should().Be("buyer@example.com");
        request.VehicleMake.Should().Be("Toyota");
        request.VehicleModel.Should().Be("Camry");
        request.RequestToken.Should().Be("abc123def456");
        request.IsCompleted.Should().BeFalse();
        request.CompletedAt.Should().BeNull();
        request.RemindersSent.Should().Be(0);
        request.LastReminderSent.Should().BeNull();
    }

    [Fact]
    public void ReviewRequest_ShouldAllowCompletion()
    {
        // Arrange
        var request = new ReviewRequest
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            BuyerEmail = "buyer@example.com",
            RequestToken = "token123",
            RequestedAt = DateTime.UtcNow.AddDays(-5),
            ExpiresAt = DateTime.UtcNow.AddDays(25),
            IsCompleted = false
        };

        // Act
        request.IsCompleted = true;
        request.CompletedAt = DateTime.UtcNow;

        // Assert
        request.IsCompleted.Should().BeTrue();
        request.CompletedAt.Should().NotBeNull();
        request.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ReviewRequest_ShouldTrackReminders()
    {
        // Arrange
        var request = new ReviewRequest
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            BuyerEmail = "buyer@example.com",
            RequestToken = "token123",
            RequestedAt = DateTime.UtcNow.AddDays(-8),
            ExpiresAt = DateTime.UtcNow.AddDays(22),
            IsCompleted = false,
            RemindersSent = 0
        };

        // Act - First reminder
        request.RemindersSent = 1;
        request.LastReminderSent = DateTime.UtcNow;

        // Assert
        request.RemindersSent.Should().Be(1);
        request.LastReminderSent.Should().NotBeNull();
        request.LastReminderSent.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ReviewRequest_ShouldHandleOptionalFields()
    {
        // Arrange & Act
        var request = new ReviewRequest
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            BuyerEmail = "buyer@example.com",
            RequestToken = "token123",
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsCompleted = false,
            RemindersSent = 0
            // VehicleId, OrderId, VehicleMake, VehicleModel are null
        };

        // Assert
        request.VehicleId.Should().BeNull();
        request.OrderId.Should().BeNull();
        request.VehicleMake.Should().BeNull();
        request.VehicleModel.Should().BeNull();
    }

    [Theory]
    [InlineData("buyer@example.com")]
    [InlineData("test.user+tag@domain.co.uk")]
    [InlineData("user123@test-domain.org")]
    public void ReviewRequest_ShouldAcceptValidEmails(string email)
    {
        // Arrange & Act
        var request = new ReviewRequest
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            BuyerEmail = email,
            RequestToken = "token123",
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        // Assert
        request.BuyerEmail.Should().Be(email);
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("TOKEN_456")]
    [InlineData("long-token-with-dashes-123456789")]
    public void ReviewRequest_ShouldAcceptValidTokens(string token)
    {
        // Arrange & Act
        var request = new ReviewRequest
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            BuyerEmail = "buyer@example.com",
            RequestToken = token,
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        // Assert
        request.RequestToken.Should().Be(token);
    }
}