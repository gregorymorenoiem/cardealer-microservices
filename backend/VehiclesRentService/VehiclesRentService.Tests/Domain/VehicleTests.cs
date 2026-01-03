using FluentAssertions;
using VehiclesRentService.Domain.Entities;
using Xunit;

namespace VehiclesRentService.Tests.Domain;

public class VehicleTests
{
    [Fact]
    public void Vehicle_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var vehicle = new Vehicle();

        // Assert
        vehicle.Id.Should().Be(Guid.Empty);
        vehicle.IsDeleted.Should().BeFalse();
        vehicle.Status.Should().Be(VehicleStatus.Draft);
        vehicle.Currency.Should().Be("USD");
        vehicle.Images.Should().NotBeNull();
        vehicle.Images.Should().BeEmpty();
    }

    [Fact]
    public void Vehicle_WithRequiredFields_ShouldBeValid()
    {
        // Arrange & Act
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Title = "2024 Toyota Camry SE - Daily Rental",
            Make = "Toyota",
            Model = "Camry",
            Year = 2024,
            PricePerDay = 85.00m,
            VIN = "1HGBH41JXMN109186",
            Status = VehicleStatus.Active
        };

        // Assert
        vehicle.Id.Should().NotBe(Guid.Empty);
        vehicle.DealerId.Should().NotBe(Guid.Empty);
        vehicle.Title.Should().NotBeNullOrEmpty();
        vehicle.Make.Should().Be("Toyota");
        vehicle.Model.Should().Be("Camry");
        vehicle.Year.Should().Be(2024);
        vehicle.PricePerDay.Should().Be(85.00m);
        vehicle.VIN.Should().HaveLength(17);
    }

    [Theory]
    [InlineData(VehicleStatus.Draft)]
    [InlineData(VehicleStatus.Active)]
    [InlineData(VehicleStatus.Pending)]
    [InlineData(VehicleStatus.Rented)]
    [InlineData(VehicleStatus.Inactive)]
    [InlineData(VehicleStatus.Maintenance)]
    public void Vehicle_AllStatuses_ShouldBeValid(VehicleStatus status)
    {
        // Arrange
        var vehicle = new Vehicle { Status = status };

        // Assert
        vehicle.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(VehicleType.Car)]
    [InlineData(VehicleType.Truck)]
    [InlineData(VehicleType.SUV)]
    [InlineData(VehicleType.Motorcycle)]
    [InlineData(VehicleType.RV)]
    [InlineData(VehicleType.Boat)]
    public void Vehicle_AllTypes_ShouldBeValid(VehicleType type)
    {
        // Arrange
        var vehicle = new Vehicle { VehicleType = type };

        // Assert
        vehicle.VehicleType.Should().Be(type);
    }

    [Fact]
    public void Vehicle_RentalPricing_ShouldSupportMultiplePeriods()
    {
        // Arrange & Act
        var vehicle = new Vehicle
        {
            PricePerDay = 85.00m,
            PricePerWeek = 500.00m,
            PricePerMonth = 1800.00m
        };

        // Assert
        vehicle.PricePerDay.Should().Be(85.00m);
        vehicle.PricePerWeek.Should().Be(500.00m);
        vehicle.PricePerMonth.Should().Be(1800.00m);
    }

    [Fact]
    public void Vehicle_AvailabilityPeriod_ShouldBeSetable()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddMonths(6);

        // Act
        var vehicle = new Vehicle
        {
            AvailableFrom = startDate,
            AvailableTo = endDate
        };

        // Assert
        vehicle.AvailableFrom.Should().Be(startDate);
        vehicle.AvailableTo.Should().Be(endDate);
    }

    [Fact]
    public void Vehicle_Images_CanBeAdded()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Title = "Test Vehicle"
        };

        var image = new VehicleImage
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Url = "https://example.com/image.jpg",
            IsPrimary = true
        };

        // Act
        vehicle.Images.Add(image);

        // Assert
        vehicle.Images.Should().HaveCount(1);
        vehicle.Images.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void Vehicle_SoftDelete_ShouldSetIsDeletedFlag()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            IsDeleted = false
        };

        // Act
        vehicle.IsDeleted = true;
        vehicle.DeletedAt = DateTime.UtcNow;

        // Assert
        vehicle.IsDeleted.Should().BeTrue();
        vehicle.DeletedAt.Should().NotBeNull();
        vehicle.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Vehicle_Location_ShouldStoreCoordinates()
    {
        // Arrange & Act
        var vehicle = new Vehicle
        {
            City = "Miami",
            State = "FL",
            ZipCode = "33101",
            Latitude = 25.7617m,
            Longitude = -80.1918m
        };

        // Assert
        vehicle.City.Should().Be("Miami");
        vehicle.State.Should().Be("FL");
        vehicle.Latitude.Should().BeApproximately(25.7617m, 0.0001m);
        vehicle.Longitude.Should().BeApproximately(-80.1918m, 0.0001m);
    }
}
