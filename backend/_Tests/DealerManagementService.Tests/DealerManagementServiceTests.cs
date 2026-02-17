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

    // ============================================
    // DealerLocation Tests
    // ============================================

    [Fact]
    public void DealerLocation_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var dealerId = Guid.NewGuid();
        var location = new DealerLocation
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Name = "Sede Principal",
            Type = LocationType.Headquarters,
            Address = "Av. Winston Churchill #45",
            City = "Santo Domingo",
            Province = "Distrito Nacional",
            Country = "DO",
            Phone = "809-555-0100",
            IsPrimary = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        location.Should().NotBeNull();
        location.Id.Should().NotBeEmpty();
        location.DealerId.Should().Be(dealerId);
        location.Name.Should().Be("Sede Principal");
        location.Type.Should().Be(LocationType.Headquarters);
        location.City.Should().Be("Santo Domingo");
        location.IsPrimary.Should().BeTrue();
        location.IsActive.Should().BeTrue();
    }

    [Fact]
    public void DealerLocation_ShouldSetAsBranch()
    {
        // Arrange
        var location = new DealerLocation
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Name = "Sucursal Santiago",
            Type = LocationType.Branch,
            Address = "Av. 27 de Febrero #123",
            City = "Santiago",
            Province = "Santiago",
            Country = "DO",
            IsPrimary = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        location.Type.Should().Be(LocationType.Branch);
        location.IsPrimary.Should().BeFalse();
        location.City.Should().Be("Santiago");
    }

    [Fact]
    public void DealerLocation_ShouldSetShowroomFeatures()
    {
        // Arrange
        var location = new DealerLocation
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Name = "Showroom Mall",
            Type = LocationType.Showroom,
            Address = "Blue Mall, Local 105",
            City = "Santo Domingo",
            Province = "Distrito Nacional",
            Country = "DO",
            HasShowroom = true,
            HasServiceCenter = false,
            HasParking = true,
            ParkingSpaces = 20,
            IsPrimary = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        location.Type.Should().Be(LocationType.Showroom);
        location.HasShowroom.Should().BeTrue();
        location.HasServiceCenter.Should().BeFalse();
        location.HasParking.Should().BeTrue();
        location.ParkingSpaces.Should().Be(20);
    }

    [Fact]
    public void DealerLocation_ShouldSetCoordinates()
    {
        // Arrange
        var location = new DealerLocation
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Name = "Sucursal GPS",
            Type = LocationType.Branch,
            Address = "Calle Principal #1",
            City = "Santo Domingo",
            Province = "Distrito Nacional",
            Country = "DO",
            Latitude = 18.4664,
            Longitude = -69.9325,
            IsPrimary = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        location.Latitude.Should().BeApproximately(18.4664, 0.0001);
        location.Longitude.Should().BeApproximately(-69.9325, 0.0001);
    }

    [Fact]
    public void DealerLocation_ShouldUpdatePrimaryStatus()
    {
        // Arrange
        var location = new DealerLocation
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Name = "Location Test",
            Type = LocationType.Branch,
            Address = "Test Address",
            City = "Test City",
            Province = "Test Province",
            Country = "DO",
            IsPrimary = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        location.IsPrimary = true;
        location.UpdatedAt = DateTime.UtcNow;

        // Assert
        location.IsPrimary.Should().BeTrue();
        location.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void DealerLocation_ShouldDeactivate()
    {
        // Arrange
        var location = new DealerLocation
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Name = "Location to Deactivate",
            Type = LocationType.Warehouse,
            Address = "Warehouse Address",
            City = "Warehouse City",
            Province = "Warehouse Province",
            Country = "DO",
            IsPrimary = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        location.IsActive = false;
        location.UpdatedAt = DateTime.UtcNow;

        // Assert
        location.IsActive.Should().BeFalse();
        location.Type.Should().Be(LocationType.Warehouse);
    }

    [Fact]
    public void LocationType_ShouldHaveExpectedValues()
    {
        // Assert
        ((int)LocationType.Headquarters).Should().Be(0);
        ((int)LocationType.Branch).Should().Be(1);
        ((int)LocationType.Showroom).Should().Be(2);
        ((int)LocationType.ServiceCenter).Should().Be(3);
        ((int)LocationType.Warehouse).Should().Be(4);
    }

    [Fact]
    public void DealerLocation_ShouldSetWorkingHours()
    {
        // Arrange
        var location = new DealerLocation
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Name = "Location with Hours",
            Type = LocationType.Showroom,
            Address = "Test Address",
            City = "Santo Domingo",
            Province = "DN",
            Country = "DO",
            WorkingHours = "Lun-Vie: 8:00 AM - 6:00 PM, Sáb: 9:00 AM - 3:00 PM",
            IsPrimary = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        location.WorkingHours.Should().Contain("Lun-Vie");
        location.WorkingHours.Should().Contain("Sáb");
    }

    [Fact]
    public void DealerLocation_ShouldSetContactInfo()
    {
        // Arrange
        var location = new DealerLocation
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Name = "Location with Contact",
            Type = LocationType.Branch,
            Address = "Contact Address",
            City = "Santo Domingo",
            Province = "DN",
            Country = "DO",
            Phone = "809-555-1234",
            Email = "sucursal@dealer.com",
            IsPrimary = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        location.Phone.Should().Be("809-555-1234");
        location.Email.Should().Be("sucursal@dealer.com");
    }
}
