using FluentAssertions;
using VehiclesSaleService.Domain.Entities;
using Xunit;
using Entities = VehiclesSaleService.Domain.Entities;

namespace VehiclesSaleService.Tests.Domain;

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
            Title = "2024 Toyota Camry SE",
            Make = "Toyota",
            Model = "Camry",
            Year = 2024,
            Price = 28500.00m,
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
        vehicle.Price.Should().Be(28500.00m);
        vehicle.VIN.Should().HaveLength(17);
    }

    [Theory]
    [InlineData(VehicleStatus.Draft)]
    [InlineData(VehicleStatus.Active)]
    [InlineData(VehicleStatus.PendingReview)]
    [InlineData(VehicleStatus.Sold)]
    [InlineData(VehicleStatus.Archived)]
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
    public void Vehicle_Category_CanBeAssigned()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Sedans",
            Slug = "sedans"
        };

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Category = category
        };

        // Assert
        vehicle.CategoryId.Should().Be(category.Id);
        vehicle.Category.Should().NotBeNull();
        vehicle.Category!.Name.Should().Be("Sedans");
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

        // Act - Simulating soft delete
        vehicle.IsDeleted = true;
        vehicle.UpdatedAt = DateTime.UtcNow;

        // Assert
        vehicle.IsDeleted.Should().BeTrue();
        vehicle.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Vehicle_Pricing_ShouldHandlePrice()
    {
        // Arrange & Act
        var vehicle = new Vehicle
        {
            Price = 25000m,
            Currency = "USD"
        };

        // Assert
        vehicle.Price.Should().Be(25000m);
        vehicle.Currency.Should().Be("USD");
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
            Latitude = 25.7617,
            Longitude = -80.1918
        };

        // Assert
        vehicle.City.Should().Be("Miami");
        vehicle.State.Should().Be("FL");
        vehicle.Latitude.Should().BeApproximately(25.7617, 0.0001);
        vehicle.Longitude.Should().BeApproximately(-80.1918, 0.0001);
    }

    [Fact]
    public void Vehicle_EngineSpecs_ShouldBeConfigurable()
    {
        // Arrange & Act
        var vehicle = new Vehicle
        {
            EngineSize = "2.5L",
            Horsepower = 203,
            Torque = 184,
            Cylinders = 4,
            Transmission = TransmissionType.Automatic,
            DriveType = Entities.DriveType.FWD,
            FuelType = FuelType.Gasoline
        };

        // Assert
        vehicle.EngineSize.Should().Be("2.5L");
        vehicle.Horsepower.Should().Be(203);
        vehicle.Cylinders.Should().Be(4);
        vehicle.Transmission.Should().Be(TransmissionType.Automatic);
        vehicle.DriveType.Should().Be(Entities.DriveType.FWD);
    }

    [Fact]
    public void Vehicle_Mileage_ShouldSupportDifferentUnits()
    {
        // Arrange & Act
        var vehicleMiles = new Vehicle
        {
            Mileage = 50000,
            MileageUnit = MileageUnit.Miles
        };

        var vehicleKm = new Vehicle
        {
            Mileage = 80000,
            MileageUnit = MileageUnit.Kilometers
        };

        // Assert
        vehicleMiles.MileageUnit.Should().Be(MileageUnit.Miles);
        vehicleKm.MileageUnit.Should().Be(MileageUnit.Kilometers);
    }

    [Fact]
    public void Vehicle_Condition_ShouldHandleAllStates()
    {
        // Arrange & Act
        var newVehicle = new Vehicle { Condition = VehicleCondition.New };
        var usedVehicle = new Vehicle { Condition = VehicleCondition.Used };
        var certifiedVehicle = new Vehicle
        {
            Condition = VehicleCondition.CertifiedPreOwned,
            IsCertified = true,
            CertificationProgram = "Toyota CPO"
        };

        // Assert
        newVehicle.Condition.Should().Be(VehicleCondition.New);
        usedVehicle.Condition.Should().Be(VehicleCondition.Used);
        certifiedVehicle.IsCertified.Should().BeTrue();
        certifiedVehicle.CertificationProgram.Should().Be("Toyota CPO");
    }

    [Fact]
    public void Vehicle_Engagement_ShouldTrackMetrics()
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.ViewCount = 100;
        vehicle.FavoriteCount = 25;
        vehicle.InquiryCount = 10;

        // Assert
        vehicle.ViewCount.Should().Be(100);
        vehicle.FavoriteCount.Should().Be(25);
        vehicle.InquiryCount.Should().Be(10);
    }
}
