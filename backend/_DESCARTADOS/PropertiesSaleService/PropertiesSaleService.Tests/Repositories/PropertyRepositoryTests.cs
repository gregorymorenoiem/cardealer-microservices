using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PropertiesSaleService.Domain.Entities;
using PropertiesSaleService.Domain.Interfaces;
using PropertiesSaleService.Infrastructure.Persistence;
using PropertiesSaleService.Infrastructure.Repositories;
using CarDealer.Shared.MultiTenancy;
using Xunit;

namespace PropertiesSaleService.Tests.Repositories;

public class PropertyRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PropertyRepository _repository;
    private readonly Guid _testDealerId = Guid.NewGuid();

    public PropertyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContextMock = new Mock<ITenantContext>();
        tenantContextMock.Setup(t => t.CurrentDealerId).Returns(_testDealerId);

        _context = new ApplicationDbContext(options, tenantContextMock.Object);
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

    #region GetByMLSNumberAsync Tests

    [Fact]
    public async Task GetByMLSNumberAsync_ExistingMLS_ReturnsProperty()
    {
        // Arrange
        var property = CreateTestProperty();
        property.MLSNumber = "MLS-12345678";
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByMLSNumberAsync("MLS-12345678");

        // Assert
        result.Should().NotBeNull();
        result!.MLSNumber.Should().Be("MLS-12345678");
    }

    [Fact]
    public async Task GetByMLSNumberAsync_NonExistingMLS_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByMLSNumberAsync("NONEXISTENT");

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
    public async Task SearchAsync_ByPriceRange_FiltersCorrectly()
    {
        // Arrange
        var cheap = CreateTestProperty(price: 200000);
        var medium = CreateTestProperty(price: 450000);
        var expensive = CreateTestProperty(price: 800000);
        _context.Properties.AddRange(cheap, medium, expensive);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters
        {
            MinPrice = 300000,
            MaxPrice = 500000
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().Price.Should().Be(450000);
    }

    [Fact]
    public async Task SearchAsync_ByBedrooms_FiltersCorrectly()
    {
        // Arrange
        var oneBed = CreateTestProperty(bedrooms: 1);
        var threeBed = CreateTestProperty(bedrooms: 3);
        var fiveBed = CreateTestProperty(bedrooms: 5);
        _context.Properties.AddRange(oneBed, threeBed, fiveBed);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters
        {
            MinBedrooms = 3
        });

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Bedrooms.Should().BeGreaterThanOrEqualTo(3));
    }

    [Fact]
    public async Task SearchAsync_ByPropertyType_FiltersCorrectly()
    {
        // Arrange
        var house = CreateTestProperty(propertyType: PropertyType.House);
        var condo = CreateTestProperty(propertyType: PropertyType.Condo);
        _context.Properties.AddRange(house, condo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters
        {
            PropertyType = PropertyType.House
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().PropertyType.Should().Be(PropertyType.House);
    }

    [Fact]
    public async Task SearchAsync_ByCity_FiltersCorrectly()
    {
        // Arrange
        var miami = CreateTestProperty(city: "Miami");
        var orlando = CreateTestProperty(city: "Orlando");
        _context.Properties.AddRange(miami, orlando);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters
        {
            City = "Miami"
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().City.Should().Be("Miami");
    }

    [Fact]
    public async Task SearchAsync_BySearchTerm_SearchesMultipleFields()
    {
        // Arrange
        var beachfront = CreateTestProperty(title: "Beachfront Villa");
        var downtown = CreateTestProperty(title: "Downtown Condo");
        _context.Properties.AddRange(beachfront, downtown);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(new PropertySearchParameters
        {
            SearchTerm = "beachfront"
        });

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Contain("Beachfront");
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
            AgentName = "Test Agent",
            Title = "New Property Listing",
            StreetAddress = "456 Oak Avenue",
            City = "Miami",
            State = "FL",
            ZipCode = "33101",
            Price = 525000,
            Bedrooms = 4,
            Bathrooms = 3,
            SquareFeet = 2500,
            PropertyType = PropertyType.House,
            PropertySubType = PropertySubType.SingleFamily
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
        property.Price = 999999;
        await _repository.UpdateAsync(property);

        // Assert
        var updated = await _context.Properties.FindAsync(property.Id);
        updated!.Title.Should().Be("Updated Title");
        updated.Price.Should().Be(999999);
        updated.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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
        deleted.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task DeleteAsync_NonExistingProperty_DoesNotThrow()
    {
        // Act
        var act = async () => await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
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
        var result = await _repository.ExistsAsync(property.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_DeletedProperty_ReturnsFalse()
    {
        // Arrange
        var property = CreateTestProperty();
        property.IsDeleted = true;
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(property.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetByAgentAsync Tests

    [Fact]
    public async Task GetByAgentAsync_ReturnsAgentProperties()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var agentProperty = CreateTestProperty();
        agentProperty.AgentId = agentId;

        var otherProperty = CreateTestProperty();
        otherProperty.AgentId = Guid.NewGuid();

        _context.Properties.AddRange(agentProperty, otherProperty);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByAgentAsync(agentId);

        // Assert
        result.Should().HaveCount(1);
        result.First().AgentId.Should().Be(agentId);
    }

    #endregion

    #region GetByDealerAsync Tests

    [Fact]
    public async Task GetByDealerAsync_ReturnsDealerProperties()
    {
        // Arrange - use the test dealer ID from context
        var dealerProperty = CreateTestProperty();
        dealerProperty.DealerId = _testDealerId;

        _context.Properties.Add(dealerProperty);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDealerAsync(_testDealerId);

        // Assert
        result.Should().HaveCount(1);
        result.First().DealerId.Should().Be(_testDealerId);
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

    #region GetCountAsync Tests

    [Fact]
    public async Task GetCountAsync_NoParameters_ReturnsNonDeletedCount()
    {
        // Arrange
        _context.Properties.Add(CreateTestProperty());
        _context.Properties.Add(CreateTestProperty());
        var deleted = CreateTestProperty();
        deleted.IsDeleted = true;
        _context.Properties.Add(deleted);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCountAsync();

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region Helper Methods

    private Property CreateTestProperty(
        string? title = null,
        string city = "Miami",
        string state = "FL",
        decimal price = 450000,
        int bedrooms = 3,
        PropertyType propertyType = PropertyType.House,
        PropertyStatus status = PropertyStatus.Active)
    {
        return new Property
        {
            Id = Guid.NewGuid(),
            DealerId = _testDealerId,
            AgentId = Guid.NewGuid(),
            AgentName = "Test Agent",
            Title = title ?? $"Beautiful {bedrooms}BR Home in {city}",
            Description = "Test property description",
            StreetAddress = "123 Test Street",
            City = city,
            State = state,
            ZipCode = "33101",
            Price = price,
            Bedrooms = bedrooms,
            Bathrooms = bedrooms - 1,
            SquareFeet = bedrooms * 500 + 1000,
            PropertyType = propertyType,
            PropertySubType = PropertySubType.SingleFamily,
            Status = status,
            Currency = "USD",
            YearBuilt = 2020,
            LotSizeAcres = 0.25m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
