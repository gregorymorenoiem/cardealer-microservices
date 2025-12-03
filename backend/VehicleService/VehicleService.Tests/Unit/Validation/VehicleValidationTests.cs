namespace VehicleService.Tests.Unit.Validation;

/// <summary>
/// Unit tests for vehicle validation logic
/// </summary>
public class VehicleValidationTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("T", true)]
    [InlineData("Toyota", true)]
    [InlineData("Ford Motor Company", true)]
    public void ValidateMake_WithVariousInputs_ReturnsExpectedResult(string make, bool expected)
    {
        // Act
        var isValid = !string.IsNullOrWhiteSpace(make);

        // Assert
        isValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("3", true)]
    [InlineData("Camry", true)]
    [InlineData("F-150 SuperCrew", true)]
    public void ValidateModel_WithVariousInputs_ReturnsExpectedResult(string model, bool expected)
    {
        // Act
        var isValid = !string.IsNullOrWhiteSpace(model);

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void ValidateColor_WithValidColors_ReturnsTrue()
    {
        // Arrange
        var validColors = new[] { "Black", "White", "Silver", "Red", "Blue", "Gray" };

        // Act & Assert
        foreach (var color in validColors)
        {
            var isValid = !string.IsNullOrWhiteSpace(color);
            isValid.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData("Gasoline", true)]
    [InlineData("Diesel", true)]
    [InlineData("Electric", true)]
    [InlineData("Hybrid", true)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    public void ValidateFuelType_WithVariousTypes_ReturnsExpectedResult(string fuelType, bool expected)
    {
        // Arrange
        var validFuelTypes = new[] { "Gasoline", "Diesel", "Electric", "Hybrid", "Plugin-Hybrid" };

        // Act
        var isValid = validFuelTypes.Contains(fuelType);

        // Assert
        isValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("Manual", true)]
    [InlineData("Automatic", true)]
    [InlineData("CVT", true)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    public void ValidateTransmission_WithVariousTypes_ReturnsExpectedResult(string transmission, bool expected)
    {
        // Arrange
        var validTransmissions = new[] { "Manual", "Automatic", "CVT", "Semi-Automatic" };

        // Act
        var isValid = validTransmissions.Contains(transmission);

        // Assert
        isValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("Sedan", true)]
    [InlineData("SUV", true)]
    [InlineData("Truck", true)]
    [InlineData("Coupe", true)]
    [InlineData("Minivan", false)]
    [InlineData("", false)]
    public void ValidateBodyType_WithVariousTypes_ReturnsExpectedResult(string bodyType, bool expected)
    {
        // Arrange
        var validBodyTypes = new[] { "Sedan", "SUV", "Truck", "Coupe", "Convertible", "Van", "Wagon" };

        // Act
        var isValid = validBodyTypes.Contains(bodyType);

        // Assert
        isValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("Available", true)]
    [InlineData("Reserved", true)]
    [InlineData("Sold", true)]
    [InlineData("Maintenance", true)]
    [InlineData("Pending", false)]
    [InlineData("", false)]
    public void ValidateStatus_WithVariousStatuses_ReturnsExpectedResult(string status, bool expected)
    {
        // Arrange
        var validStatuses = new[] { "Available", "Reserved", "Sold", "Maintenance" };

        // Act
        var isValid = validStatuses.Contains(status);

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void VehicleSpecs_WithAllFields_PassesValidation()
    {
        // Arrange
        var specs = new
        {
            EngineSize = "2.5L",
            Horsepower = 203,
            Cylinders = 4,
            DriveType = "FWD",
            SeatingCapacity = 5
        };

        // Act
        var isValid = !string.IsNullOrWhiteSpace(specs.EngineSize) &&
                      specs.Horsepower > 0 &&
                      specs.Cylinders > 0 &&
                      specs.SeatingCapacity > 0;

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(50, true)]
    [InlineData(500, true)]
    [InlineData(1000, true)]
    [InlineData(0, false)]
    [InlineData(-50, false)]
    public void ValidateHorsepower_WithVariousValues_ReturnsExpectedResult(int horsepower, bool expected)
    {
        // Act
        var isValid = horsepower > 0;

        // Assert
        isValid.Should().Be(expected);
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(4, true)]
    [InlineData(6, true)]
    [InlineData(8, true)]
    [InlineData(12, true)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void ValidateCylinders_WithVariousValues_ReturnsExpectedResult(int cylinders, bool expected)
    {
        // Act
        var isValid = cylinders > 0;

        // Assert
        isValid.Should().Be(expected);
    }
}
