using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using CarDealer.Shared.MultiTenancy;
using VehiclesRentService.Domain.Entities;
using VehiclesRentService.Domain.Interfaces;
using VehiclesRentService.Infrastructure.Persistence;
using VehiclesRentService.Infrastructure.Repositories;
using Xunit;
using Entities = VehiclesRentService.Domain.Entities;

namespace VehiclesRentService.Tests.Repositories;

public class VehicleRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly VehicleRepository _repository;
    private readonly Guid _testDealerId = Guid.NewGuid();

    public VehicleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Mock ITenantContext
        var tenantContextMock = new Mock<ITenantContext>();
        tenantContextMock.Setup(t => t.CurrentDealerId).Returns(_testDealerId);

        _context = new ApplicationDbContext(options, tenantContextMock.Object);
        _repository = new VehicleRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingVehicle_ReturnsVehicle()
    {
        // Arrange
        var vehicle = CreateTestVehicle();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(vehicle.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(vehicle.Id);
        result.Title.Should().Be(vehicle.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingVehicle_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_DeletedVehicle_ReturnsNull()
    {
        // Arrange
        var vehicle = CreateTestVehicle();
        vehicle.IsDeleted = true;
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(vehicle.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByVINAsync Tests

    [Fact]
    public async Task GetByVINAsync_ExistingVIN_ReturnsVehicle()
    {
        // Arrange
        var vehicle = CreateTestVehicle();
        vehicle.VIN = "1HGBH41JXMN109186";
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByVINAsync("1HGBH41JXMN109186");

        // Assert
        result.Should().NotBeNull();
        result!.VIN.Should().Be("1HGBH41JXMN109186");
    }

    [Fact]
    public async Task GetByVINAsync_NonExistingVIN_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByVINAsync("INVALIDVIN123456");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_NoParameters_ReturnsVehicles()
    {
        // Arrange
        var vehicle1 = CreateTestVehicle();
        var vehicle2 = CreateTestVehicle();
        _context.Vehicles.AddRange(vehicle1, vehicle2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters());

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ByPriceRange_FiltersCorrectly()
    {
        // Arrange
        var cheap = CreateTestVehicle(price: 15000);
        var medium = CreateTestVehicle(price: 30000);
        var expensive = CreateTestVehicle(price: 60000);
        _context.Vehicles.AddRange(cheap, medium, expensive);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters
        {
            MinPrice = 20000,
            MaxPrice = 40000
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().Price.Should().Be(30000);
    }

    [Fact]
    public async Task SearchAsync_ByMake_FiltersCorrectly()
    {
        // Arrange
        var toyota = CreateTestVehicle(make: "Toyota");
        var honda = CreateTestVehicle(make: "Honda");
        _context.Vehicles.AddRange(toyota, honda);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters { Make = "Toyota" });

        // Assert
        result.Should().HaveCount(1);
        result.First().Make.Should().Be("Toyota");
    }

    [Fact]
    public async Task SearchAsync_ByModel_FiltersCorrectly()
    {
        // Arrange
        var camry = CreateTestVehicle(make: "Toyota", model: "Camry");
        var corolla = CreateTestVehicle(make: "Toyota", model: "Corolla");
        _context.Vehicles.AddRange(camry, corolla);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters { Model = "Camry" });

        // Assert
        result.Should().HaveCount(1);
        result.First().Model.Should().Be("Camry");
    }

    [Fact]
    public async Task SearchAsync_ByYearRange_FiltersCorrectly()
    {
        // Arrange
        var old = CreateTestVehicle(year: 2018);
        var recent = CreateTestVehicle(year: 2022);
        var newest = CreateTestVehicle(year: 2024);
        _context.Vehicles.AddRange(old, recent, newest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters
        {
            MinYear = 2020,
            MaxYear = 2023
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().Year.Should().Be(2022);
    }

    [Fact]
    public async Task SearchAsync_ByMaxMileage_FiltersCorrectly()
    {
        // Arrange
        var lowMiles = CreateTestVehicle();
        lowMiles.Mileage = 15000;
        var highMiles = CreateTestVehicle();
        highMiles.Mileage = 100000;
        _context.Vehicles.AddRange(lowMiles, highMiles);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters { MaxMileage = 50000 });

        // Assert
        result.Should().HaveCount(1);
        result.First().Mileage.Should().Be(15000);
    }

    [Fact]
    public async Task SearchAsync_ByVehicleType_FiltersCorrectly()
    {
        // Arrange
        var car = CreateTestVehicle();
        car.VehicleType = VehicleType.Car;
        var truck = CreateTestVehicle();
        truck.VehicleType = VehicleType.Truck;
        _context.Vehicles.AddRange(car, truck);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters { VehicleType = VehicleType.Truck });

        // Assert
        result.Should().HaveCount(1);
        result.First().VehicleType.Should().Be(VehicleType.Truck);
    }

    [Fact]
    public async Task SearchAsync_ByDriveType_FiltersCorrectly()
    {
        // Arrange
        var fwd = CreateTestVehicle();
        fwd.DriveType = Entities.DriveType.FWD;
        var awd = CreateTestVehicle();
        awd.DriveType = Entities.DriveType.AWD;
        _context.Vehicles.AddRange(fwd, awd);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters { DriveType = Entities.DriveType.AWD });

        // Assert
        result.Should().HaveCount(1);
        result.First().DriveType.Should().Be(Entities.DriveType.AWD);
    }

    [Fact]
    public async Task SearchAsync_ByState_FiltersCorrectly()
    {
        // Arrange
        var florida = CreateTestVehicle();
        florida.State = "FL";
        var california = CreateTestVehicle();
        california.State = "CA";
        _context.Vehicles.AddRange(florida, california);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters { State = "FL" });

        // Assert
        result.Should().HaveCount(1);
        result.First().State.Should().Be("FL");
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            _context.Vehicles.Add(CreateTestVehicle());
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters { Skip = 5, Take = 5 });

        // Assert
        result.Should().HaveCount(5);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidVehicle_SavesAndReturnsVehicle()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            DealerId = _testDealerId,
            Title = "New Vehicle",
            Make = "Ford",
            Model = "Mustang",
            Year = 2024,
            Price = 55000,
            VIN = "1FA6P8TH2K1234567"
        };

        // Act
        var result = await _repository.CreateAsync(vehicle);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        var saved = await _context.Vehicles.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingVehicle_UpdatesFields()
    {
        // Arrange
        var vehicle = CreateTestVehicle();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // Act
        vehicle.Title = "Updated Title";
        vehicle.Price = 35000;
        await _repository.UpdateAsync(vehicle);

        // Assert
        var updated = await _context.Vehicles.FindAsync(vehicle.Id);
        updated!.Title.Should().Be("Updated Title");
        updated.Price.Should().Be(35000);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingVehicle_SoftDeletes()
    {
        // Arrange
        var vehicle = CreateTestVehicle();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(vehicle.Id);

        // Assert
        var deleted = await _context.Vehicles.FindAsync(vehicle.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region GetFeaturedAsync Tests

    [Fact]
    public async Task GetFeaturedAsync_ReturnsFeaturedVehicles()
    {
        // Arrange
        var featured = CreateTestVehicle();
        featured.IsFeatured = true;
        featured.Status = VehicleStatus.Active;

        var regular = CreateTestVehicle();
        regular.IsFeatured = false;
        regular.Status = VehicleStatus.Active;

        _context.Vehicles.AddRange(featured, regular);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetFeaturedAsync(10);

        // Assert
        result.Should().HaveCount(1);
        result.First().IsFeatured.Should().BeTrue();
    }

    #endregion

    #region GetCountAsync Tests

    [Fact]
    public async Task GetCountAsync_NoParameters_ReturnsTotalCount()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            _context.Vehicles.Add(CreateTestVehicle());
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCountAsync();

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task GetCountAsync_WithFilters_ReturnsFilteredCount()
    {
        // Arrange
        for (int i = 0; i < 3; i++)
        {
            var toyota = CreateTestVehicle(make: "Toyota");
            _context.Vehicles.Add(toyota);
        }
        for (int i = 0; i < 2; i++)
        {
            var honda = CreateTestVehicle(make: "Honda");
            _context.Vehicles.Add(honda);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCountAsync(new VehicleSearchParameters { Make = "Toyota" });

        // Assert
        result.Should().Be(3);
    }

    #endregion

    #region Helper Methods

    private Vehicle CreateTestVehicle(
        string? title = null,
        string make = "Toyota",
        string model = "Camry",
        int year = 2024,
        decimal price = 28000)
    {
        return new Vehicle
        {
            Id = Guid.NewGuid(),
            DealerId = _testDealerId,
            Title = title ?? $"{year} {make} {model}",
            Description = "Test vehicle description",
            Make = make,
            Model = model,
            Year = year,
            Price = price,
            VIN = GenerateRandomVIN(),
            Status = VehicleStatus.Active,
            Currency = "USD",
            VehicleType = VehicleType.Car,
            Condition = VehicleCondition.Used,
            Mileage = 30000,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static string GenerateRandomVIN()
    {
        const string chars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 17)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    #endregion
}
