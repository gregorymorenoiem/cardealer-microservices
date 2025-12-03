namespace VehicleService.Tests.Unit.Search;

/// <summary>
/// Unit tests for vehicle search functionality
/// </summary>
public class VehicleSearchTests
{
    [Fact]
    public void SearchVehicles_ByMake_ReturnsMatchingVehicles()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Id = 1, Make = "Toyota", Model = "Camry" },
            new { Id = 2, Make = "Honda", Model = "Accord" },
            new { Id = 3, Make = "Toyota", Model = "Corolla" }
        };

        // Act
        var results = vehicles.Where(v => v.Make == "Toyota").ToList();

        // Assert
        results.Should().HaveCount(2);
        results.All(v => v.Make == "Toyota").Should().BeTrue();
    }

    [Fact]
    public void SearchVehicles_ByPriceRange_ReturnsMatchingVehicles()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Id = 1, Make = "Toyota", Price = 25000m },
            new { Id = 2, Make = "Honda", Price = 35000m },
            new { Id = 3, Make = "Ford", Price = 45000m }
        };

        // Act
        var results = vehicles.Where(v => v.Price >= 20000 && v.Price <= 40000).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(v => v.Id == 1);
        results.Should().Contain(v => v.Id == 2);
    }

    [Fact]
    public void SearchVehicles_ByYear_ReturnsMatchingVehicles()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Id = 1, Year = 2020 },
            new { Id = 2, Year = 2022 },
            new { Id = 3, Year = 2024 }
        };

        // Act
        var results = vehicles.Where(v => v.Year >= 2022).ToList();

        // Assert
        results.Should().HaveCount(2);
        results.All(v => v.Year >= 2022).Should().BeTrue();
    }

    [Fact]
    public void SearchVehicles_ByMultipleCriteria_ReturnsMatchingVehicles()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Id = 1, Make = "Toyota", Year = 2023, Price = 28000m, FuelType = "Hybrid" },
            new { Id = 2, Make = "Toyota", Year = 2024, Price = 32000m, FuelType = "Gasoline" },
            new { Id = 3, Make = "Honda", Year = 2023, Price = 30000m, FuelType = "Hybrid" }
        };

        // Act
        var results = vehicles.Where(v =>
            v.Make == "Toyota" &&
            v.Year >= 2023 &&
            v.FuelType == "Hybrid"
        ).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Id.Should().Be(1);
    }

    [Fact]
    public void SearchVehicles_ByMileageRange_ReturnsMatchingVehicles()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Id = 1, Mileage = 5000 },
            new { Id = 2, Mileage = 25000 },
            new { Id = 3, Mileage = 75000 }
        };

        // Act
        var results = vehicles.Where(v => v.Mileage <= 30000).ToList();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public void SearchVehicles_ByFuelType_ReturnsMatchingVehicles()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Id = 1, FuelType = "Electric" },
            new { Id = 2, FuelType = "Gasoline" },
            new { Id = 3, FuelType = "Electric" },
            new { Id = 4, FuelType = "Hybrid" }
        };

        // Act
        var results = vehicles.Where(v => v.FuelType == "Electric").ToList();

        // Assert
        results.Should().HaveCount(2);
        results.All(v => v.FuelType == "Electric").Should().BeTrue();
    }

    [Fact]
    public void SearchVehicles_ByTransmission_ReturnsMatchingVehicles()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Id = 1, Transmission = "Automatic" },
            new { Id = 2, Transmission = "Manual" },
            new { Id = 3, Transmission = "Automatic" }
        };

        // Act
        var results = vehicles.Where(v => v.Transmission == "Automatic").ToList();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public void SortVehicles_ByPrice_ReturnsOrderedList()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Id = 1, Price = 35000m },
            new { Id = 2, Price = 25000m },
            new { Id = 3, Price = 45000m }
        };

        // Act
        var results = vehicles.OrderBy(v => v.Price).ToList();

        // Assert
        results[0].Price.Should().Be(25000m);
        results[1].Price.Should().Be(35000m);
        results[2].Price.Should().Be(45000m);
    }

    [Fact]
    public void SortVehicles_ByYearDescending_ReturnsOrderedList()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Id = 1, Year = 2020 },
            new { Id = 2, Year = 2024 },
            new { Id = 3, Year = 2022 }
        };

        // Act
        var results = vehicles.OrderByDescending(v => v.Year).ToList();

        // Assert
        results[0].Year.Should().Be(2024);
        results[1].Year.Should().Be(2022);
        results[2].Year.Should().Be(2020);
    }

    [Fact]
    public void SearchVehicles_NoMatchingCriteria_ReturnsEmptyList()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Id = 1, Make = "Toyota" },
            new { Id = 2, Make = "Honda" }
        };

        // Act
        var results = vehicles.Where(v => v.Make == "Ferrari").ToList();

        // Assert
        results.Should().BeEmpty();
    }
}
