using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehiclesSaleService.Domain.Entities;
using VehiclesSaleService.Infrastructure.Persistence;
using VehiclesSaleService.Infrastructure.Repositories;
using Xunit;

namespace VehiclesSaleService.Tests.Repositories;

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
        var result = await _repository.GetByVINAsync("NONEXISTENT12345");

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
    public async Task SearchAsync_ByPriceRange_FiltersCorrectly()
    {
        // Arrange
        var cheap = CreateTestVehicle(price: 10000);
        var medium = CreateTestVehicle(price: 25000);
        var expensive = CreateTestVehicle(price: 50000);
        _context.Vehicles.AddRange(cheap, medium, expensive);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters
        {
            MinPrice = 20000,
            MaxPrice = 30000
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().Price.Should().Be(25000);
    }

    [Fact]
    public async Task SearchAsync_ByYearRange_FiltersCorrectly()
    {
        // Arrange
        var old = CreateTestVehicle(year: 2015);
        var recent = CreateTestVehicle(year: 2023);
        var newest = CreateTestVehicle(year: 2024);
        _context.Vehicles.AddRange(old, recent, newest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters
        {
            MinYear = 2022,
            MaxYear = 2024
        });

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(v => v.Year.Should().BeGreaterOrEqualTo(2022));
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            _context.Vehicles.Add(CreateTestVehicle(title: $"Vehicle {i}"));
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters
        {
            Page = 2,
            PageSize = 10
        });

        // Assert
        result.Should().HaveCount(10);
    }

    [Fact]
    public async Task SearchAsync_BySearchTerm_SearchesTitleMakeModel()
    {
        // Arrange
        var camry = CreateTestVehicle(title: "2024 Toyota Camry", make: "Toyota", model: "Camry");
        var accord = CreateTestVehicle(title: "2024 Honda Accord", make: "Honda", model: "Accord");
        _context.Vehicles.AddRange(camry, accord);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new VehicleSearchParameters
        {
            SearchTerm = "camry"
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().Model.Should().Be("Camry");
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidVehicle_SavesAndReturnsVehicle()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Title = "New Vehicle",
            Make = "Ford",
            Model = "Mustang",
            Year = 2024,
            Price = 45000,
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

    [Fact]
    public async Task CreateAsync_SetsUpdatedAt()
    {
        // Arrange
        var vehicle = CreateTestVehicle();

        // Act
        var result = await _repository.CreateAsync(vehicle);

        // Assert
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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
        vehicle.Price = 99999;
        await _repository.UpdateAsync(vehicle);

        // Assert
        var updated = await _context.Vehicles.FindAsync(vehicle.Id);
        updated!.Title.Should().Be("Updated Title");
        updated.Price.Should().Be(99999);
        updated.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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
        deleted.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task DeleteAsync_NonExistingVehicle_DoesNotThrow()
    {
        // Act
        var act = async () => await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_ExistingVehicle_ReturnsTrue()
    {
        // Arrange
        var vehicle = CreateTestVehicle();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(vehicle.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_DeletedVehicle_ReturnsFalse()
    {
        // Arrange
        var vehicle = CreateTestVehicle();
        vehicle.IsDeleted = true;
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(vehicle.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetBySellerAsync Tests

    [Fact]
    public async Task GetBySellerAsync_ReturnsSellersVehicles()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var sellerVehicle = CreateTestVehicle();
        sellerVehicle.SellerId = sellerId;

        var otherVehicle = CreateTestVehicle();
        otherVehicle.SellerId = Guid.NewGuid();

        _context.Vehicles.AddRange(sellerVehicle, otherVehicle);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySellerAsync(sellerId);

        // Assert
        result.Should().HaveCount(1);
        result.First().SellerId.Should().Be(sellerId);
    }

    #endregion

    #region GetByDealerAsync Tests

    [Fact]
    public async Task GetByDealerAsync_ReturnsDealersVehicles()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var dealerVehicle = CreateTestVehicle();
        dealerVehicle.DealerId = dealerId;

        var otherVehicle = CreateTestVehicle();
        otherVehicle.DealerId = Guid.NewGuid();

        _context.Vehicles.AddRange(dealerVehicle, otherVehicle);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDealerAsync(dealerId);

        // Assert
        result.Should().HaveCount(1);
        result.First().DealerId.Should().Be(dealerId);
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

    [Fact]
    public async Task GetFeaturedAsync_RespectsLimit()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            var vehicle = CreateTestVehicle();
            vehicle.IsFeatured = true;
            _context.Vehicles.Add(vehicle);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetFeaturedAsync(5);

        // Assert
        result.Should().HaveCount(5);
    }

    #endregion

    #region GetCountAsync Tests

    [Fact]
    public async Task GetCountAsync_NoParameters_ReturnsActiveCount()
    {
        // Arrange
        _context.Vehicles.Add(CreateTestVehicle());
        _context.Vehicles.Add(CreateTestVehicle());
        var deleted = CreateTestVehicle();
        deleted.IsDeleted = true;
        _context.Vehicles.Add(deleted);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCountAsync();

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region Helper Methods

    private static Vehicle CreateTestVehicle(
        string? title = null,
        string make = "Toyota",
        string model = "Camry",
        int year = 2024,
        decimal price = 25000,
        VehicleStatus status = VehicleStatus.Active)
    {
        return new Vehicle
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            Title = title ?? $"{year} {make} {model}",
            Description = "Test vehicle description",
            Make = make,
            Model = model,
            Year = year,
            Price = price,
            VIN = GenerateRandomVIN(),
            Status = status,
            Currency = "USD",
            VehicleType = VehicleType.Car,
            Condition = VehicleCondition.Used,
            Mileage = 25000,
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
