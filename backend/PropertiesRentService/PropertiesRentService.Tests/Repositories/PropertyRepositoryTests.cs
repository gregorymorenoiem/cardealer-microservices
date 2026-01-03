using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PropertiesRentService.Domain.Entities;
using PropertiesRentService.Infrastructure.Persistence;
using PropertiesRentService.Infrastructure.Repositories;
using Xunit;

namespace PropertiesRentService.Tests.Repositories;

public class PropertyRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PropertyRepository _repository;

    public PropertyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new PropertyRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingProperty_ReturnsProperty()
    {
        // Arrange
        var property = CreateTestProperty();
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(property.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(property.Id);
        result.Title.Should().Be(property.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingProperty_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_DeletedProperty_ReturnsNull()
    {
        // Arrange
        var property = CreateTestProperty();
        property.IsDeleted = true;
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(property.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_NoParameters_ReturnsActiveProperties()
    {
        // Arrange
        var activeProperty = CreateTestProperty(status: PropertyStatus.Active);
        var draftProperty = CreateTestProperty(status: PropertyStatus.Draft);
        _context.Properties.AddRange(activeProperty, draftProperty);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters());

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(PropertyStatus.Active);
    }

    [Fact]
    public async Task SearchAsync_ByPricePerMonthRange_FiltersCorrectly()
    {
        // Arrange
        var cheap = CreateTestProperty(pricePerMonth: 1000);
        var medium = CreateTestProperty(pricePerMonth: 2500);
        var expensive = CreateTestProperty(pricePerMonth: 5000);
        _context.Properties.AddRange(cheap, medium, expensive);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters 
        { 
            MinPricePerMonth = 2000, 
            MaxPricePerMonth = 3000 
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().PricePerMonth.Should().Be(2500);
    }

    [Fact]
    public async Task SearchAsync_ByBedrooms_FiltersCorrectly()
    {
        // Arrange
        var studio = CreateTestProperty(bedrooms: 0);
        var twoBed = CreateTestProperty(bedrooms: 2);
        var fourBed = CreateTestProperty(bedrooms: 4);
        _context.Properties.AddRange(studio, twoBed, fourBed);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters 
        { 
            MinBedrooms = 2 
        });

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Bedrooms.Should().BeGreaterOrEqualTo(2));
    }

    [Fact]
    public async Task SearchAsync_ByPropertyType_FiltersCorrectly()
    {
        // Arrange
        var apartment = CreateTestProperty(propertyType: PropertyType.Apartment);
        var house = CreateTestProperty(propertyType: PropertyType.House);
        _context.Properties.AddRange(apartment, house);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters 
        { 
            PropertyType = PropertyType.Apartment 
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().PropertyType.Should().Be(PropertyType.Apartment);
    }

    [Fact]
    public async Task SearchAsync_ByPetFriendly_FiltersCorrectly()
    {
        // Arrange
        var petFriendly = CreateTestProperty();
        petFriendly.AllowsPets = true;

        var noPets = CreateTestProperty();
        noPets.AllowsPets = false;

        _context.Properties.AddRange(petFriendly, noPets);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters 
        { 
            AllowsPets = true 
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().AllowsPets.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            _context.Properties.Add(CreateTestProperty(title: $"Rental Property {i}"));
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters 
        { 
            Page = 2, 
            PageSize = 10 
        });

        // Assert
        result.Should().HaveCount(10);
    }

    #endregion

    #region GetAvailableAsync Tests

    [Fact]
    public async Task GetAvailableAsync_ReturnsOnlyAvailableProperties()
    {
        // Arrange
        var available = CreateTestProperty(status: PropertyStatus.Active);
        available.AvailableFrom = DateTime.UtcNow.AddDays(-1);

        var rented = CreateTestProperty(status: PropertyStatus.Rented);

        _context.Properties.AddRange(available, rented);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAvailableAsync(DateTime.UtcNow);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(PropertyStatus.Active);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidProperty_SavesAndReturnsProperty()
    {
        // Arrange
        var property = new Property
        {
            Title = "New Rental Listing",
            StreetAddress = "789 Palm Street",
            City = "Fort Lauderdale",
            State = "FL",
            ZipCode = "33301",
            PricePerMonth = 2200,
            Bedrooms = 2,
            Bathrooms = 2,
            SquareFeet = 1100,
            PropertyType = PropertyType.Apartment
        };

        // Act
        var result = await _repository.CreateAsync(property);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        var saved = await _context.Properties.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingProperty_UpdatesFields()
    {
        // Arrange
        var property = CreateTestProperty();
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        // Act
        property.Title = "Updated Title";
        property.PricePerMonth = 3500;
        await _repository.UpdateAsync(property);

        // Assert
        var updated = await _context.Properties.FindAsync(property.Id);
        updated!.Title.Should().Be("Updated Title");
        updated.PricePerMonth.Should().Be(3500);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingProperty_SoftDeletes()
    {
        // Arrange
        var property = CreateTestProperty();
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(property.Id);

        // Assert
        var deleted = await _context.Properties.FindAsync(property.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region GetFeaturedAsync Tests

    [Fact]
    public async Task GetFeaturedAsync_ReturnsFeaturedProperties()
    {
        // Arrange
        var featured = CreateTestProperty();
        featured.IsFeatured = true;

        var regular = CreateTestProperty();
        regular.IsFeatured = false;

        _context.Properties.AddRange(featured, regular);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetFeaturedAsync(10);

        // Assert
        result.Should().HaveCount(1);
        result.First().IsFeatured.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static Property CreateTestProperty(
        string? title = null,
        string city = "Miami Beach",
        string state = "FL",
        decimal pricePerMonth = 2500,
        int bedrooms = 2,
        PropertyType propertyType = PropertyType.Apartment,
        PropertyStatus status = PropertyStatus.Active)
    {
        return new Property
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            LandlordId = Guid.NewGuid(),
            Title = title ?? $"Spacious {bedrooms}BR {propertyType} for Rent",
            Description = "Test rental property description",
            StreetAddress = "123 Ocean Drive",
            City = city,
            State = state,
            ZipCode = "33139",
            PricePerMonth = pricePerMonth,
            PricePerWeek = pricePerMonth / 4,
            SecurityDeposit = pricePerMonth,
            Bedrooms = bedrooms,
            Bathrooms = bedrooms,
            SquareFeet = bedrooms * 400 + 400,
            PropertyType = propertyType,
            Status = status,
            Currency = "USD",
            AvailableFrom = DateTime.UtcNow.AddDays(-1),
            MinLeaseTerm = 12,
            AllowsPets = true,
            AllowsSmoking = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
