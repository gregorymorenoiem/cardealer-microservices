using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using CarDealer.Shared.MultiTenancy;
using PropertiesRentService.Domain.Entities;
using PropertiesRentService.Domain.Interfaces;
using PropertiesRentService.Infrastructure.Persistence;
using PropertiesRentService.Infrastructure.Repositories;
using Xunit;

namespace PropertiesRentService.Tests.Repositories;

public class PropertyRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PropertyRepository _repository;
    private readonly Guid _testDealerId = Guid.NewGuid();
    private readonly Mock<ITenantContext> _tenantContextMock;

    public PropertyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _tenantContextMock = new Mock<ITenantContext>();
        _tenantContextMock.Setup(x => x.CurrentDealerId).Returns(_testDealerId);

        _context = new ApplicationDbContext(options, _tenantContextMock.Object);
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
        draftProperty.DealerId = _testDealerId;
        _context.Properties.AddRange(activeProperty, draftProperty);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters());

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task SearchAsync_ByPriceRange_FiltersCorrectly()
    {
        // Arrange
        var cheap = CreateTestProperty(price: 1000);
        var medium = CreateTestProperty(price: 2500);
        var expensive = CreateTestProperty(price: 5000);
        _context.Properties.AddRange(cheap, medium, expensive);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters
        {
            MinPrice = 2000,
            MaxPrice = 3000
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().Price.Should().Be(2500);
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
        result.Should().AllSatisfy(p => p.Bedrooms.Should().BeGreaterThanOrEqualTo(2));
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
    public async Task SearchAsync_ByHasPool_FiltersCorrectly()
    {
        // Arrange
        var withPool = CreateTestProperty();
        withPool.HasPool = true;

        var noPool = CreateTestProperty();
        noPool.HasPool = false;

        _context.Properties.AddRange(withPool, noPool);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters
        {
            HasPool = true
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().HasPool.Should().BeTrue();
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
            Skip = 10,
            Take = 10
        });

        // Assert
        result.Should().HaveCount(10);
    }

    #endregion

    #region GetByAgentAsync Tests

    [Fact]
    public async Task GetByAgentAsync_ReturnsPropertiesForAgent()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var property1 = CreateTestProperty();
        property1.AgentId = agentId;

        var property2 = CreateTestProperty();
        property2.AgentId = agentId;

        var otherAgentProperty = CreateTestProperty();
        otherAgentProperty.AgentId = Guid.NewGuid();

        _context.Properties.AddRange(property1, property2, otherAgentProperty);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByAgentAsync(agentId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.AgentId.Should().Be(agentId));
    }

    #endregion

    #region GetByDealerAsync Tests

    [Fact]
    public async Task GetByDealerAsync_ReturnsPropertiesForDealer()
    {
        // Arrange - use _testDealerId which is the tenant context dealer
        var property1 = CreateTestProperty();
        property1.DealerId = _testDealerId;

        var property2 = CreateTestProperty();
        property2.DealerId = _testDealerId;

        _context.Properties.AddRange(property1, property2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDealerAsync(_testDealerId);

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Should().AllSatisfy(p => p.DealerId.Should().Be(_testDealerId));
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidProperty_SavesAndReturnsProperty()
    {
        // Arrange
        var property = new Property
        {
            DealerId = _testDealerId,
            AgentId = Guid.NewGuid(),
            AgentName = "John Agent",
            Title = "New Rental Listing",
            StreetAddress = "789 Palm Street",
            City = "Fort Lauderdale",
            State = "FL",
            ZipCode = "33301",
            Price = 2200,
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
        property.Price = 3500;
        await _repository.UpdateAsync(property);

        // Assert
        var updated = await _context.Properties.FindAsync(property.Id);
        updated!.Title.Should().Be("Updated Title");
        updated.Price.Should().Be(3500);
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

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_ExistingProperty_ReturnsTrue()
    {
        // Arrange
        var property = CreateTestProperty();
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(property.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingProperty_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region GetCountAsync Tests

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            _context.Properties.Add(CreateTestProperty());
        }
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.GetCountAsync(null);

        // Assert
        count.Should().BeGreaterThanOrEqualTo(5);
    }

    #endregion

    #region Helper Methods

    private Property CreateTestProperty(
        string? title = null,
        string city = "Miami Beach",
        string state = "FL",
        decimal price = 2500,
        int bedrooms = 2,
        PropertyType propertyType = PropertyType.Apartment,
        PropertyStatus status = PropertyStatus.Active)
    {
        return new Property
        {
            Id = Guid.NewGuid(),
            DealerId = _testDealerId,
            AgentId = Guid.NewGuid(),
            AgentName = "Test Agent",
            Title = title ?? $"Spacious {bedrooms}BR {propertyType} for Rent",
            Description = "Test rental property description",
            StreetAddress = "123 Ocean Drive",
            City = city,
            State = state,
            ZipCode = "33139",
            Price = price,
            Bedrooms = bedrooms,
            Bathrooms = bedrooms,
            SquareFeet = bedrooms * 400 + 400,
            PropertyType = propertyType,
            Status = status,
            Currency = "USD",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
