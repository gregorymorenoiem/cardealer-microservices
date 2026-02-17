using FluentAssertions;
using VehiclesRentService.Domain.Entities;
using Xunit;
using Entities = VehiclesRentService.Domain.Entities;

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
        vehicle.ViewCount.Should().Be(0);
        vehicle.FavoriteCount.Should().Be(0);
        vehicle.InquiryCount.Should().Be(0);
    }

    [Fact]
    public void Vehicle_WithRequiredFields_ShouldBeValid()
    {
        // Arrange & Act
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Title = "2024 Toyota Camry SE",
            Make = "Toyota",
            Model = "Camry",
            Year = 2024,
            Price = 28000.00m,
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
        vehicle.Price.Should().Be(28000.00m);
        vehicle.VIN.Should().HaveLength(17);
    }

    [Theory]
    [InlineData(VehicleStatus.Draft)]
    [InlineData(VehicleStatus.Active)]
    [InlineData(VehicleStatus.PendingReview)]
    [InlineData(VehicleStatus.Reserved)]
    [InlineData(VehicleStatus.Sold)]
    [InlineData(VehicleStatus.Archived)]
    [InlineData(VehicleStatus.Rejected)]
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
    [InlineData(VehicleType.Van)]
    [InlineData(VehicleType.Motorcycle)]
    [InlineData(VehicleType.RV)]
    [InlineData(VehicleType.Boat)]
    [InlineData(VehicleType.ATV)]
    [InlineData(VehicleType.Commercial)]
    public void Vehicle_AllTypes_ShouldBeValid(VehicleType type)
    {
        // Arrange
        var vehicle = new Vehicle { VehicleType = type };

        // Assert
        vehicle.VehicleType.Should().Be(type);
    }

    [Fact]
    public void Vehicle_Images_CanBeAdded()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Title = "Test Vehicle"
        };

        var image = new VehicleImage
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            DealerId = vehicle.DealerId,
            Url = "https://example.com/image.jpg",
            IsPrimary = true
        };

        // Act
        vehicle.Images.Add(image);

        // Assert
        vehicle.Images.Should().HaveCount(1);
        vehicle.Images.First().IsPrimary.Should().BeTrue();
        vehicle.Images.First().Url.Should().Be("https://example.com/image.jpg");
    }

    [Fact]
    public void Vehicle_Category_CanBeAssigned()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            DealerId = Guid.Empty,
            Name = "SUVs",
            Slug = "suvs",
            IsActive = true
        };

        // Act
        var vehicle = new Vehicle
        {
            CategoryId = categoryId,
            Category = category,
            VehicleType = VehicleType.SUV
        };

        // Assert
        vehicle.CategoryId.Should().Be(categoryId);
        vehicle.Category.Should().NotBeNull();
        vehicle.Category!.Name.Should().Be("SUVs");
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

        // Assert
        vehicle.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Vehicle_Pricing_ShouldSupportCurrency()
    {
        // Arrange & Act
        var vehicle = new Vehicle
        {
            Price = 45000.00m,
            Currency = "EUR"
        };

        // Assert
        vehicle.Price.Should().Be(45000.00m);
        vehicle.Currency.Should().Be("EUR");
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
            Country = "USA",
            Latitude = 25.7617,
            Longitude = -80.1918
        };

        // Assert
        vehicle.City.Should().Be("Miami");
        vehicle.State.Should().Be("FL");
        vehicle.ZipCode.Should().Be("33101");
        vehicle.Country.Should().Be("USA");
        vehicle.Latitude.Should().BeApproximately(25.7617, 0.0001);
        vehicle.Longitude.Should().BeApproximately(-80.1918, 0.0001);
    }

    [Fact]
    public void Vehicle_EngineSpecs_ShouldBeSetable()
    {
        // Arrange & Act
        var vehicle = new Vehicle
        {
            EngineSize = "2.5L",
            Horsepower = 203,
            Torque = 184,
            Cylinders = 4,
            FuelType = FuelType.Gasoline,
            Transmission = TransmissionType.Automatic,
            DriveType = Entities.DriveType.FWD
        };

        // Assert
        vehicle.EngineSize.Should().Be("2.5L");
        vehicle.Horsepower.Should().Be(203);
        vehicle.Torque.Should().Be(184);
        vehicle.Cylinders.Should().Be(4);
        vehicle.FuelType.Should().Be(FuelType.Gasoline);
        vehicle.Transmission.Should().Be(TransmissionType.Automatic);
        vehicle.DriveType.Should().Be(Entities.DriveType.FWD);
    }

    [Fact]
    public void Vehicle_Mileage_ShouldSupportDifferentUnits()
    {
        // Arrange & Act - Miles
        var vehicleMiles = new Vehicle
        {
            Mileage = 45000,
            MileageUnit = MileageUnit.Miles
        };

        // Arrange & Act - Kilometers
        var vehicleKm = new Vehicle
        {
            Mileage = 72000,
            MileageUnit = MileageUnit.Kilometers
        };

        // Assert
        vehicleMiles.Mileage.Should().Be(45000);
        vehicleMiles.MileageUnit.Should().Be(MileageUnit.Miles);

        vehicleKm.Mileage.Should().Be(72000);
        vehicleKm.MileageUnit.Should().Be(MileageUnit.Kilometers);
    }

    [Fact]
    public void Vehicle_Condition_ShouldSupportAllTypes()
    {
        // Arrange & Act
        var newVehicle = new Vehicle { Condition = VehicleCondition.New };
        var usedVehicle = new Vehicle { Condition = VehicleCondition.Used };
        var cpoVehicle = new Vehicle { Condition = VehicleCondition.CertifiedPreOwned };

        // Assert
        newVehicle.Condition.Should().Be(VehicleCondition.New);
        usedVehicle.Condition.Should().Be(VehicleCondition.Used);
        cpoVehicle.Condition.Should().Be(VehicleCondition.CertifiedPreOwned);
    }

    [Fact]
    public void Vehicle_EngagementMetrics_ShouldBeUpdatable()
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.ViewCount = 150;
        vehicle.FavoriteCount = 25;
        vehicle.InquiryCount = 10;

        // Assert
        vehicle.ViewCount.Should().Be(150);
        vehicle.FavoriteCount.Should().Be(25);
        vehicle.InquiryCount.Should().Be(10);
    }
}
