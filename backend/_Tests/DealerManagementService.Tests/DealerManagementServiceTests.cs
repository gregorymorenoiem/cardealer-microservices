using DealerManagementService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace DealerManagementService.Tests;

public class DealerManagementServiceTests
{
    [Fact]
    public void Dealer_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Premium Motors RD",
            RNC = "123456789",
            Email = "contact@premiummotors.com",
            Phone = "809-555-1234",
            Status = DealerStatus.Pending,
            VerificationStatus = VerificationStatus.NotVerified,
            CurrentPlan = DealerPlan.Free,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        dealer.Should().NotBeNull();
        dealer.Id.Should().NotBeEmpty();
        dealer.BusinessName.Should().Be("Premium Motors RD");
        dealer.RNC.Should().Be("123456789");
        dealer.Status.Should().Be(DealerStatus.Pending);
        dealer.VerificationStatus.Should().Be(VerificationStatus.NotVerified);
        dealer.CurrentPlan.Should().Be(DealerPlan.Free);
    }

    [Fact]
    public void Dealer_ShouldUpdatePlan_ToBasic()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Test Motors",
            RNC = "123456789",
            Email = "test@motors.com",
            Phone = "809-555-0000",
            Status = DealerStatus.Pending,
            CurrentPlan = DealerPlan.Free,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        dealer.CurrentPlan = DealerPlan.Basic;
        dealer.MaxActiveListings = 50;
        dealer.IsSubscriptionActive = true;

        // Assert
        dealer.CurrentPlan.Should().Be(DealerPlan.Basic);
        dealer.MaxActiveListings.Should().Be(50);
        dealer.IsSubscriptionActive.Should().BeTrue();
    }

    [Fact]
    public void Dealer_ShouldUpdatePlan_ToPro()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Test Motors",
            RNC = "123456789",
            Email = "test@motors.com",
            Phone = "809-555-0000",
            CurrentPlan = DealerPlan.Free,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        dealer.CurrentPlan = DealerPlan.Pro;
        dealer.MaxActiveListings = 200;
        dealer.IsSubscriptionActive = true;

        // Assert
        dealer.CurrentPlan.Should().Be(DealerPlan.Pro);
        dealer.MaxActiveListings.Should().Be(200);
        dealer.IsSubscriptionActive.Should().BeTrue();
    }

    [Fact]
    public void Dealer_ShouldUpdatePlan_ToEnterprise()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Test Motors",
            RNC = "123456789",
            Email = "test@motors.com",
            Phone = "809-555-0000",
            CurrentPlan = DealerPlan.Free,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        dealer.CurrentPlan = DealerPlan.Enterprise;
        dealer.MaxActiveListings = 9999;
        dealer.IsSubscriptionActive = true;

        // Assert
        dealer.CurrentPlan.Should().Be(DealerPlan.Enterprise);
        dealer.MaxActiveListings.Should().Be(9999);
        dealer.IsSubscriptionActive.Should().BeTrue();
    }

    [Fact]
    public void Dealer_ShouldUpdateStatus_ToActive()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Test Motors",
            RNC = "123456789",
            Email = "test@motors.com",
            Phone = "809-555-0000",
            Status = DealerStatus.Pending,
            VerificationStatus = VerificationStatus.NotVerified,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        dealer.Status = DealerStatus.Active;
        dealer.VerificationStatus = VerificationStatus.Verified;

        // Assert
        dealer.Status.Should().Be(DealerStatus.Active);
        dealer.VerificationStatus.Should().Be(VerificationStatus.Verified);
    }

    [Fact]
    public void Dealer_ShouldUpdateStatus_ToSuspended()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Test Motors",
            RNC = "123456789",
            Email = "test@motors.com",
            Phone = "809-555-0000",
            Status = DealerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        dealer.Status = DealerStatus.Suspended;

        // Assert
        dealer.Status.Should().Be(DealerStatus.Suspended);
    }

    [Fact]
    public void DealerType_ShouldHaveExpectedValues()
    {
        // Assert
        ((int)DealerType.Independent).Should().Be(0);
        ((int)DealerType.Chain).Should().Be(1);
        ((int)DealerType.MultipleStore).Should().Be(2);
        ((int)DealerType.Franchise).Should().Be(3);
    }

    [Fact]
    public void DealerStatus_ShouldHaveExpectedValues()
    {
        // Assert
        ((int)DealerStatus.Pending).Should().Be(0);
        ((int)DealerStatus.UnderReview).Should().Be(1);
        ((int)DealerStatus.Active).Should().Be(2);
        ((int)DealerStatus.Suspended).Should().Be(3);
        ((int)DealerStatus.Rejected).Should().Be(4);
        ((int)DealerStatus.Inactive).Should().Be(5);
    }

    [Fact]
    public void DealerPlan_ShouldHaveExpectedValues()
    {
        // Assert - Updated to match actual enum values
        ((int)DealerPlan.Free).Should().Be(0);
        ((int)DealerPlan.Basic).Should().Be(1);
        ((int)DealerPlan.Pro).Should().Be(2);
        ((int)DealerPlan.Enterprise).Should().Be(3);
    }

    [Fact]
    public void VerificationStatus_ShouldHaveExpectedValues()
    {
        // Assert
        ((int)VerificationStatus.NotVerified).Should().Be(0);
        ((int)VerificationStatus.DocumentsUploaded).Should().Be(1);
        ((int)VerificationStatus.UnderReview).Should().Be(2);
        ((int)VerificationStatus.Verified).Should().Be(3);
        ((int)VerificationStatus.Rejected).Should().Be(4);
        ((int)VerificationStatus.RequiresMoreInfo).Should().Be(5);
    }
}
