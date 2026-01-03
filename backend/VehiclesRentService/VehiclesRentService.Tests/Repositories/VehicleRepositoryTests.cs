using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehiclesRentService.Domain.Entities;
using VehiclesRentService.Infrastructure.Persistence;
using VehiclesRentService.Infrastructure.Repositories;
using Xunit;

namespace VehiclesRentService.Tests.Repositories;

public class VehicleRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly VehicleRepository _repository;

    public VehicleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
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

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_NoParameters_ReturnsActiveVehicles()
    {
        // Arrange
        var activeVehicle = CreateTestVehicle(status: VehicleStatus.Active);
        var draftVehicle = CreateTestVehicle(status: VehicleStatus.Draft);
        _context.Vehicles.AddRange(activeVehicle, draftVehicle);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters());

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(VehicleStatus.Active);
    }

    [Fact]
    public async Task SearchAsync_ByPricePerDayRange_FiltersCorrectly()
    {
        // Arrange
        var cheap = CreateTestVehicle(pricePerDay: 50);
        var medium = CreateTestVehicle(pricePerDay: 100);
        var expensive = CreateTestVehicle(pricePerDay: 200);
        _context.Vehicles.AddRange(cheap, medium, expensive);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters 
        { 
            MinPricePerDay = 75, 
            MaxPricePerDay = 150 
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().PricePerDay.Should().Be(100);
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

    #endregion

    #region GetAvailableAsync Tests

    [Fact]
    public async Task GetAvailableAsync_ReturnsOnlyAvailableVehicles()
    {
        // Arrange
        var available = CreateTestVehicle(status: VehicleStatus.Active);
        available.AvailableFrom = DateTime.UtcNow.AddDays(-1);
        available.AvailableTo = DateTime.UtcNow.AddMonths(1);

        var rented = CreateTestVehicle(status: VehicleStatus.Rented);

        _context.Vehicles.AddRange(available, rented);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAvailableAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(VehicleStatus.Active);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidVehicle_SavesAndReturnsVehicle()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Title = "New Rental Vehicle",
            Make = "Ford",
            Model = "Mustang",
            Year = 2024,
            PricePerDay = 150,
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
        vehicle.PricePerDay = 199;
        await _repository.UpdateAsync(vehicle);

        // Assert
        var updated = await _context.Vehicles.FindAsync(vehicle.Id);
        updated!.Title.Should().Be("Updated Title");
        updated.PricePerDay.Should().Be(199);
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

        var regular = CreateTestVehicle();
        regular.IsFeatured = false;

        _context.Vehicles.AddRange(featured, regular);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetFeaturedAsync(10);

        // Assert
        result.Should().HaveCount(1);
        result.First().IsFeatured.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static Vehicle CreateTestVehicle(
        string? title = null,
        string make = "Toyota",
        string model = "Camry",
        int year = 2024,
        decimal pricePerDay = 85,
        VehicleStatus status = VehicleStatus.Active)
    {
        return new Vehicle
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Title = title ?? $"{year} {make} {model} - Rental",
            Description = "Test rental vehicle description",
            Make = make,
            Model = model,
            Year = year,
            PricePerDay = pricePerDay,
            PricePerWeek = pricePerDay * 6,
            PricePerMonth = pricePerDay * 25,
            VIN = GenerateRandomVIN(),
            Status = status,
            Currency = "USD",
            VehicleType = VehicleType.Car,
            Condition = VehicleCondition.Excellent,
            Mileage = 15000,
            AvailableFrom = DateTime.UtcNow.AddDays(-1),
            AvailableTo = DateTime.UtcNow.AddMonths(6),
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
