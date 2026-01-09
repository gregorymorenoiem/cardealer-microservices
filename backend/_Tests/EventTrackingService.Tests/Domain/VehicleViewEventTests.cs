using EventTrackingService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EventTrackingService.Tests.Domain;

public class VehicleViewEventTests
{
    [Fact]
    public void VehicleViewEvent_ShouldBeCreated_WithVehicleDetails()
    {
        // Arrange & Act
        var vehicleId = Guid.NewGuid();
        var evt = new VehicleViewEvent
        {
            Id = Guid.NewGuid(),
            EventType = "VehicleView",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = $"https://okla.com.do/vehicles/{vehicleId}",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            VehicleId = vehicleId,
            VehicleTitle = "Toyota Corolla 2020 XLE",
            VehiclePrice = 1500000m,
            VehicleMake = "Toyota",
            VehicleModel = "Corolla",
            VehicleYear = 2020
        };

        // Assert
        evt.VehicleId.Should().Be(vehicleId);
        evt.VehicleTitle.Should().Be("Toyota Corolla 2020 XLE");
        evt.VehiclePrice.Should().Be(1500000m);
        evt.VehicleMake.Should().Be("Toyota");
    }

    [Fact]
    public void VehicleViewEvent_RecordEngagement_ShouldSetAllFlags()
    {
        // Arrange
        var evt = new VehicleViewEvent
        {
            Id = Guid.NewGuid(),
            EventType = "VehicleView",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/vehicles/123",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            VehicleId = Guid.NewGuid(),
            VehicleTitle = "Honda Civic 2021",
            VehiclePrice = 1800000m,
            VehicleMake = "Honda",
            VehicleModel = "Civic",
            VehicleYear = 2021
        };

        // Act
        evt.RecordEngagement(
            viewedImages: true,
            viewedSpecs: true,
            clickedContact: true,
            addedToFavorites: true
        );

        // Assert
        evt.ViewedImages.Should().BeTrue();
        evt.ViewedSpecs.Should().BeTrue();
        evt.ClickedContact.Should().BeTrue();
        evt.AddedToFavorites.Should().BeTrue();
    }

    [Fact]
    public void VehicleViewEvent_SetTimeSpent_ShouldUpdateTimeSpentSeconds()
    {
        // Arrange
        var evt = new VehicleViewEvent
        {
            Id = Guid.NewGuid(),
            EventType = "VehicleView",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/vehicles/123",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            VehicleId = Guid.NewGuid(),
            VehicleTitle = "Mazda 3 2022",
            VehiclePrice = 2000000m,
            VehicleMake = "Mazda",
            VehicleModel = "3",
            VehicleYear = 2022
        };

        // Act
        evt.SetTimeSpent(180);

        // Assert
        evt.TimeSpentSeconds.Should().Be(180);
    }

    [Fact]
    public void VehicleViewEvent_IsHighIntent_ShouldReturnTrue_WhenEngagedAndSpent60Seconds()
    {
        // Arrange
        var evt = new VehicleViewEvent
        {
            Id = Guid.NewGuid(),
            EventType = "VehicleView",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/vehicles/123",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            VehicleId = Guid.NewGuid(),
            VehicleTitle = "Nissan Altima 2021",
            VehiclePrice = 1600000m,
            VehicleMake = "Nissan",
            VehicleModel = "Altima",
            VehicleYear = 2021,
            ViewedImages = true,
            ViewedSpecs = true,
            TimeSpentSeconds = 90
        };

        // Act
        var result = evt.IsHighIntent();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VehicleViewEvent_IsHighIntent_ShouldReturnFalse_WhenNotEngaged()
    {
        // Arrange
        var evt = new VehicleViewEvent
        {
            Id = Guid.NewGuid(),
            EventType = "VehicleView",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/vehicles/123",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            VehicleId = Guid.NewGuid(),
            VehicleTitle = "Ford F-150 2022",
            VehiclePrice = 3000000m,
            VehicleMake = "Ford",
            VehicleModel = "F-150",
            VehicleYear = 2022,
            ViewedImages = false,
            ViewedSpecs = false,
            TimeSpentSeconds = 20
        };

        // Act
        var result = evt.IsHighIntent();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VehicleViewEvent_IsConverted_ShouldReturnTrue_WhenContactedOrFavorited()
    {
        // Arrange
        var evt = new VehicleViewEvent
        {
            Id = Guid.NewGuid(),
            EventType = "VehicleView",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/vehicles/123",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            VehicleId = Guid.NewGuid(),
            VehicleTitle = "Hyundai Elantra 2023",
            VehiclePrice = 1700000m,
            VehicleMake = "Hyundai",
            VehicleModel = "Elantra",
            VehicleYear = 2023,
            ClickedContact = true
        };

        // Act
        var result = evt.IsConverted();

        // Assert
        result.Should().BeTrue();
    }
}
