using Xunit;
using FluentAssertions;
using PostgresDbService.Infrastructure.Repositories;
using PostgresDbService.Tests.Helpers;
using System.Text.Json;

namespace PostgresDbService.Tests.Infrastructure;

/// <summary>
/// Tests for VehicleRepository
/// </summary>
public class VehicleRepositoryTests : IDisposable
{
    private readonly Infrastructure.Persistence.CentralizedDbContext _context;
    private readonly GenericRepository _genericRepository;
    private readonly VehicleRepository _vehicleRepository;

    public VehicleRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext(nameof(VehicleRepositoryTests));
        _genericRepository = new GenericRepository(_context);
        _vehicleRepository = new VehicleRepository(_genericRepository);
    }

    [Fact]
    public async Task CreateVehicleAsync_ShouldCreateVehicle_WithCorrectData()
    {
        // Arrange
        var vehicleData = new
        {
            Make = "Toyota",
            Model = "Corolla",
            Year = 2023,
            Price = 28000m,
            Mileage = 1500,
            FuelType = "Gasoline",
            SellerId = Guid.NewGuid().ToString(),
            Status = "Active",
            City = "Santo Domingo"
        };

        // Act
        var result = await _vehicleRepository.CreateVehicleAsync(vehicleData, "seller1");

        // Assert
        result.Should().NotBeNull();
        result.ServiceName.Should().Be("VehiclesSaleService");
        result.EntityType.Should().Be("Vehicle");
        result.CreatedBy.Should().Be("seller1");
        
        var data = JsonSerializer.Deserialize<JsonElement>(result.DataJson);
        data.GetProperty("Make").GetString().Should().Be("Toyota");
        data.GetProperty("Model").GetString().Should().Be("Corolla");
        data.GetProperty("Year").GetInt32().Should().Be(2023);
        data.GetProperty("Price").GetDecimal().Should().Be(28000m);
    }

    [Fact]
    public async Task GetVehicleByIdAsync_ShouldReturnVehicle_WhenExists()
    {
        // Arrange
        var vehicleData = new
        {
            Make = "Honda",
            Model = "Civic",
            Year = 2022,
            Price = 25000m,
            SellerId = "seller-123"
        };
        var created = await _vehicleRepository.CreateVehicleAsync(vehicleData, "seller");
        var vehicleId = Guid.Parse(created.EntityId);

        // Act
        var result = await _vehicleRepository.GetVehicleByIdAsync(vehicleId);

        // Assert
        result.Should().NotBeNull();
        result!.EntityId.Should().Be(vehicleId.ToString());
        var data = JsonSerializer.Deserialize<JsonElement>(result.DataJson);
        data.GetProperty("Make").GetString().Should().Be("Honda");
        data.GetProperty("Model").GetString().Should().Be("Civic");
    }

    [Fact]
    public async Task GetVehiclesBySellerAsync_ShouldReturnSellerVehicles()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var vehicles = new[]
        {
            new { Make = "Toyota", Model = "Camry", Year = 2022, Price = 30000m, SellerId = sellerId.ToString() },
            new { Make = "Honda", Model = "Accord", Year = 2021, Price = 28000m, SellerId = sellerId.ToString() },
            new { Make = "Ford", Model = "Focus", Year = 2023, Price = 22000m, SellerId = Guid.NewGuid().ToString() } // Different seller
        };

        foreach (var vehicle in vehicles)
        {
            await _vehicleRepository.CreateVehicleAsync(vehicle, "system");
        }

        // Act
        var results = await _vehicleRepository.GetVehiclesBySellerAsync(sellerId);

        // Assert
        results.Should().HaveCount(2);
        results.All(v => 
        {
            var data = JsonSerializer.Deserialize<JsonElement>(v.DataJson);
            return data.GetProperty("SellerId").GetString() == sellerId.ToString();
        }).Should().BeTrue();
    }

    [Fact]
    public async Task SearchVehiclesAsync_ShouldFilterByMake()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Make = "Toyota", Model = "Camry", Year = 2022, Price = 30000m, SellerId = "seller1" },
            new { Make = "Toyota", Model = "Corolla", Year = 2023, Price = 25000m, SellerId = "seller1" },
            new { Make = "Honda", Model = "Civic", Year = 2022, Price = 24000m, SellerId = "seller2" }
        };

        foreach (var vehicle in vehicles)
        {
            await _vehicleRepository.CreateVehicleAsync(vehicle, "system");
        }

        // Act
        var results = await _vehicleRepository.SearchVehiclesAsync("Toyota", null, null, null, null, null);

        // Assert
        results.Should().HaveCount(2);
        results.All(v => 
        {
            var data = JsonSerializer.Deserialize<JsonElement>(v.DataJson);
            return data.GetProperty("Make").GetString() == "Toyota";
        }).Should().BeTrue();
    }

    [Fact]
    public async Task SearchVehiclesAsync_ShouldFilterByPriceRange()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Make = "Toyota", Model = "Camry", Year = 2022, Price = 30000m, SellerId = "seller1" },
            new { Make = "Honda", Model = "Civic", Year = 2022, Price = 24000m, SellerId = "seller2" },
            new { Make = "Ford", Model = "Focus", Year = 2023, Price = 18000m, SellerId = "seller3" }
        };

        foreach (var vehicle in vehicles)
        {
            await _vehicleRepository.CreateVehicleAsync(vehicle, "system");
        }

        // Act
        var results = await _vehicleRepository.SearchVehiclesAsync(null, null, null, null, 20000m, 28000m);

        // Assert
        results.Should().HaveCount(1);
        var data = JsonSerializer.Deserialize<JsonElement>(results.First().DataJson);
        data.GetProperty("Price").GetDecimal().Should().Be(24000m);
    }

    [Fact]
    public async Task SearchVehiclesAsync_ShouldFilterByYearRange()
    {
        // Arrange
        var vehicles = new[]
        {
            new { Make = "Toyota", Model = "Camry", Year = 2020, Price = 25000m, SellerId = "seller1" },
            new { Make = "Honda", Model = "Civic", Year = 2022, Price = 26000m, SellerId = "seller2" },
            new { Make = "Ford", Model = "Focus", Year = 2024, Price = 27000m, SellerId = "seller3" }
        };

        foreach (var vehicle in vehicles)
        {
            await _vehicleRepository.CreateVehicleAsync(vehicle, "system");
        }

        // Act
        var results = await _vehicleRepository.SearchVehiclesAsync(null, null, 2021, 2023, null, null);

        // Assert
        results.Should().HaveCount(1);
        var data = JsonSerializer.Deserialize<JsonElement>(results.First().DataJson);
        data.GetProperty("Year").GetInt32().Should().Be(2022);
    }

    [Fact]
    public async Task UpdateVehicleAsync_ShouldUpdateVehicleData()
    {
        // Arrange
        var originalData = new
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2022,
            Price = 30000m,
            Status = "Active",
            SellerId = "seller1"
        };
        var created = await _vehicleRepository.CreateVehicleAsync(originalData, "seller");
        var vehicleId = Guid.Parse(created.EntityId);

        var updateData = new
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2022,
            Price = 28000m, // Price reduced
            Status = "Sold", // Status changed
            SellerId = "seller1",
            Mileage = 5000
        };

        // Act
        var result = await _vehicleRepository.UpdateVehicleAsync(vehicleId, updateData, "updater");

        // Assert
        result.Should().NotBeNull();
        result.UpdatedBy.Should().Be("updater");
        result.UpdatedAt.Should().NotBeNull();
        
        var data = JsonSerializer.Deserialize<JsonElement>(result.DataJson);
        data.GetProperty("Price").GetDecimal().Should().Be(28000m);
        data.GetProperty("Status").GetString().Should().Be("Sold");
        data.GetProperty("Mileage").GetInt32().Should().Be(5000);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}