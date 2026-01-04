using CarDealer.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VehiclesSaleService.Domain.Entities;
using VehiclesSaleService.Domain.Interfaces;
using VehiclesSaleService.Infrastructure.Persistence;
using VehiclesSaleService.Infrastructure.Repositories;
using Xunit;
using Entities = VehiclesSaleService.Domain.Entities;

namespace VehiclesSaleService.Tests.Repositories;

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
        var result = await _repository.GetByVINAsync("NONEXISTENT123456");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidVehicle_ReturnsCreatedVehicle()
    {
        // Arrange
        var vehicle = CreateTestVehicle();

        // Act
        var result = await _repository.CreateAsync(vehicle);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);

        var savedVehicle = await _context.Vehicles.FindAsync(result.Id);
        savedVehicle.Should().NotBeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingVehicle_UpdatesVehicle()
    {
        // Arrange
        var vehicle = CreateTestVehicle();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // Act
        vehicle.Price = 35000m;
        vehicle.Title = "Updated Title";
        await _repository.UpdateAsync(vehicle);

        // Assert
        var updatedVehicle = await _context.Vehicles.FindAsync(vehicle.Id);
        updatedVehicle!.Price.Should().Be(35000m);
        updatedVehicle.Title.Should().Be("Updated Title");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingVehicle_SoftDeletesVehicle()
    {
        // Arrange
        var vehicle = CreateTestVehicle();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(vehicle.Id);

        // Assert
        var deletedVehicle = await _context.Vehicles.FindAsync(vehicle.Id);
        deletedVehicle!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_ByMake_ReturnsMatchingVehicles()
    {
        // Arrange
        var toyota1 = CreateTestVehicle("Toyota", "Camry");
        var toyota2 = CreateTestVehicle("Toyota", "Corolla");
        var honda = CreateTestVehicle("Honda", "Civic");

        _context.Vehicles.AddRange(toyota1, toyota2, honda);
        await _context.SaveChangesAsync();

        var parameters = new VehicleSearchParameters { Make = "Toyota" };

        // Act
        var results = await _repository.SearchAsync(parameters);

        // Assert
        results.Should().HaveCount(2);
        results.All(v => v.Make == "Toyota").Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_ByPriceRange_ReturnsMatchingVehicles()
    {
        // Arrange
        var cheap = CreateTestVehicle();
        cheap.Price = 15000m;

        var mid = CreateTestVehicle();
        mid.Price = 25000m;

        var expensive = CreateTestVehicle();
        expensive.Price = 50000m;

        _context.Vehicles.AddRange(cheap, mid, expensive);
        await _context.SaveChangesAsync();

        var parameters = new VehicleSearchParameters
        {
            MinPrice = 20000m,
            MaxPrice = 30000m
        };

        // Act
        var results = await _repository.SearchAsync(parameters);

        // Assert
        results.Should().HaveCount(1);
        results.First().Price.Should().Be(25000m);
    }

    [Fact]
    public async Task SearchAsync_ByYear_ReturnsMatchingVehicles()
    {
        // Arrange
        var old = CreateTestVehicle();
        old.Year = 2018;

        var recent = CreateTestVehicle();
        recent.Year = 2023;

        var newest = CreateTestVehicle();
        newest.Year = 2024;

        _context.Vehicles.AddRange(old, recent, newest);
        await _context.SaveChangesAsync();

        var parameters = new VehicleSearchParameters
        {
            MinYear = 2022
        };

        // Act
        var results = await _repository.SearchAsync(parameters);

        // Assert
        results.Should().HaveCount(2);
        results.All(v => v.Year >= 2022).Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_ByCondition_ReturnsMatchingVehicles()
    {
        // Arrange
        var newCar = CreateTestVehicle();
        newCar.Condition = VehicleCondition.New;

        var usedCar = CreateTestVehicle();
        usedCar.Condition = VehicleCondition.Used;

        _context.Vehicles.AddRange(newCar, usedCar);
        await _context.SaveChangesAsync();

        var parameters = new VehicleSearchParameters
        {
            Condition = VehicleCondition.New
        };

        // Act
        var results = await _repository.SearchAsync(parameters);

        // Assert
        results.Should().HaveCount(1);
        results.First().Condition.Should().Be(VehicleCondition.New);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            var vehicle = CreateTestVehicle();
            vehicle.Title = $"Vehicle {i:D2}";
            _context.Vehicles.Add(vehicle);
        }
        await _context.SaveChangesAsync();

        var parameters = new VehicleSearchParameters
        {
            Skip = 10,
            Take = 10
        };

        // Act
        var results = await _repository.SearchAsync(parameters);

        // Assert
        results.Should().HaveCount(10);
    }

    #endregion

    #region GetCountAsync Tests

    [Fact]
    public async Task GetCountAsync_WithParameters_ReturnsCorrectCount()
    {
        // Arrange
        var toyota1 = CreateTestVehicle("Toyota", "Camry");
        var toyota2 = CreateTestVehicle("Toyota", "Corolla");
        var honda = CreateTestVehicle("Honda", "Civic");

        _context.Vehicles.AddRange(toyota1, toyota2, honda);
        await _context.SaveChangesAsync();

        var parameters = new VehicleSearchParameters { Make = "Toyota" };

        // Act
        var count = await _repository.GetCountAsync(parameters);

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetCountAsync_NoParameters_ReturnsTotalCount()
    {
        // Arrange
        _context.Vehicles.AddRange(
            CreateTestVehicle(),
            CreateTestVehicle(),
            CreateTestVehicle()
        );
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        count.Should().Be(3);
    }

    #endregion

    #region Helper Methods

    private Vehicle CreateTestVehicle(string make = "Toyota", string model = "Camry")
    {
        return new Vehicle
        {
            Id = Guid.NewGuid(),
            DealerId = _testDealerId,
            Title = $"2024 {make} {model} SE",
            Make = make,
            Model = model,
            Year = 2024,
            Price = 28500.00m,
            VIN = GenerateRandomVIN(),
            Status = VehicleStatus.Active,
            Condition = VehicleCondition.Used,
            Mileage = 25000,
            MileageUnit = MileageUnit.Miles,
            VehicleType = VehicleType.Car,
            BodyStyle = BodyStyle.Sedan,
            FuelType = FuelType.Gasoline,
            Transmission = TransmissionType.Automatic,
            DriveType = Entities.DriveType.FWD,
            SellerId = Guid.NewGuid(),
            SellerName = "Test Dealer",
            CreatedAt = DateTime.UtcNow
        };
    }

    private string GenerateRandomVIN()
    {
        const string chars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 17).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }

    #endregion
}
