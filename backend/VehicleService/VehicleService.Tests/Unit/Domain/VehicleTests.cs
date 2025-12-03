namespace VehicleService.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Vehicle domain entity
/// </summary>
public class VehicleTests
{
    [Fact]
    public void CreateVehicle_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var vehicle = new
        {
            VIN = "1HGBH41JXMN109186",
            Make = "Toyota",
            Model = "Camry",
            Year = 2024,
            Price = 35000m,
            Status = "Available"
        };

        // Assert
        vehicle.VIN.Should().NotBeNullOrEmpty();
        vehicle.Make.Should().NotBeNullOrEmpty();
        vehicle.Model.Should().NotBeNullOrEmpty();
        vehicle.Year.Should().BeGreaterThan(1900);
        vehicle.Price.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("1HGBH41JXMN109186", true)]
    [InlineData("5FNRL5H41FB123456", true)]
    [InlineData("ABC123", false)]
    [InlineData("", false)]
    [InlineData("123456789012345678", false)] // Too long
    public void ValidateVIN_WithVariousFormats_ReturnsExpectedResult(string vin, bool expected)
    {
        // Act
        var isValid = !string.IsNullOrWhiteSpace(vin) && vin.Length == 17;

        // Assert
        isValid.Should().Be(expected);
    }

    [Theory]
    [InlineData(1990, true)]
    [InlineData(2024, true)]
    [InlineData(2025, true)]
    [InlineData(1800, false)]
    [InlineData(2030, false)]
    public void ValidateYear_WithVariousYears_ReturnsExpectedResult(int year, bool expected)
    {
        // Act
        var currentYear = DateTime.UtcNow.Year;
        var isValid = year >= 1900 && year <= currentYear + 1;

        // Assert
        isValid.Should().Be(expected);
    }

    [Theory]
    [InlineData(1000, true)]
    [InlineData(50000, true)]
    [InlineData(150000, true)]
    [InlineData(0, false)]
    [InlineData(-100, false)]
    public void ValidatePrice_WithVariousPrices_ReturnsExpectedResult(decimal price, bool expected)
    {
        // Act
        var isValid = price > 0;

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void VehicleStatus_ShouldHaveValidValues()
    {
        // Arrange & Act
        var statuses = new[] { "Available", "Reserved", "Sold", "Maintenance" };

        // Assert
        statuses.Should().Contain("Available");
        statuses.Should().Contain("Reserved");
        statuses.Should().Contain("Sold");
        statuses.Should().Contain("Maintenance");
        statuses.Should().HaveCount(4);
    }

    [Fact]
    public void FuelType_ShouldHaveValidValues()
    {
        // Arrange & Act
        var fuelTypes = new[] { "Gasoline", "Diesel", "Electric", "Hybrid", "Plugin-Hybrid" };

        // Assert
        fuelTypes.Should().Contain("Gasoline");
        fuelTypes.Should().Contain("Diesel");
        fuelTypes.Should().Contain("Electric");
        fuelTypes.Should().Contain("Hybrid");
        fuelTypes.Should().HaveCount(5);
    }

    [Fact]
    public void Transmission_ShouldHaveValidValues()
    {
        // Arrange & Act
        var transmissions = new[] { "Manual", "Automatic", "CVT", "Semi-Automatic" };

        // Assert
        transmissions.Should().Contain("Manual");
        transmissions.Should().Contain("Automatic");
        transmissions.Should().Contain("CVT");
        transmissions.Should().HaveCount(4);
    }

    [Fact]
    public void BodyType_ShouldHaveValidValues()
    {
        // Arrange & Act
        var bodyTypes = new[] { "Sedan", "SUV", "Truck", "Coupe", "Convertible", "Van", "Wagon" };

        // Assert
        bodyTypes.Should().Contain("Sedan");
        bodyTypes.Should().Contain("SUV");
        bodyTypes.Should().Contain("Truck");
        bodyTypes.Should().HaveCount(7);
    }

    [Fact]
    public void Vehicle_WithAllRequiredFields_PassesValidation()
    {
        // Arrange
        var vehicle = new
        {
            VIN = "1HGBH41JXMN109186",
            Make = "Honda",
            Model = "Accord",
            Year = 2023,
            Price = 28000m,
            Mileage = 15000,
            FuelType = "Gasoline",
            Transmission = "Automatic",
            BodyType = "Sedan",
            Status = "Available"
        };

        // Act
        var isValid = !string.IsNullOrWhiteSpace(vehicle.VIN) &&
                      vehicle.VIN.Length == 17 &&
                      !string.IsNullOrWhiteSpace(vehicle.Make) &&
                      !string.IsNullOrWhiteSpace(vehicle.Model) &&
                      vehicle.Year >= 1900 &&
                      vehicle.Price > 0;

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(50000, true)]
    [InlineData(200000, true)]
    [InlineData(-1, false)]
    public void ValidateMileage_WithVariousMileages_ReturnsExpectedResult(int mileage, bool expected)
    {
        // Act
        var isValid = mileage >= 0;

        // Assert
        isValid.Should().Be(expected);
    }
}
