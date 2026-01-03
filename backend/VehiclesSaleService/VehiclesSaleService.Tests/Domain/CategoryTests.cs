using FluentAssertions;
using VehiclesSaleService.Domain.Entities;
using Xunit;

namespace VehiclesSaleService.Tests.Domain;

public class CategoryTests
{
    [Fact]
    public void Category_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var category = new Category();

        // Assert
        category.Id.Should().Be(Guid.Empty);
        category.IsActive.Should().BeTrue();
        category.IsSystem.Should().BeFalse();
        category.SortOrder.Should().Be(0);
        category.Vehicles.Should().NotBeNull();
        category.Vehicles.Should().BeEmpty();
        category.Children.Should().NotBeNull();
        category.Children.Should().BeEmpty();
    }

    [Fact]
    public void Category_WithHierarchy_ShouldSupportParentChild()
    {
        // Arrange
        var parentCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Vehicles",
            Slug = "vehicles"
        };

        var childCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Sedans",
            Slug = "sedans",
            ParentId = parentCategory.Id,
            Parent = parentCategory
        };

        // Act
        parentCategory.Children.Add(childCategory);

        // Assert
        parentCategory.Children.Should().HaveCount(1);
        childCategory.ParentId.Should().Be(parentCategory.Id);
        childCategory.Parent.Should().Be(parentCategory);
    }

    [Fact]
    public void Category_SystemCategory_ShouldBeMarkedAsSystem()
    {
        // Arrange & Act
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Cars",
            Slug = "cars",
            IsSystem = true,
            DealerId = Guid.Empty // System categories don't belong to a dealer
        };

        // Assert
        category.IsSystem.Should().BeTrue();
    }

    [Fact]
    public void Category_DealerCustomCategory_ShouldHaveDealerId()
    {
        // Arrange
        var dealerId = Guid.NewGuid();

        // Act
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Custom Category",
            Slug = "custom-category",
            DealerId = dealerId,
            IsSystem = false
        };

        // Assert
        category.DealerId.Should().Be(dealerId);
        category.IsSystem.Should().BeFalse();
    }

    [Fact]
    public void Category_Timestamps_ShouldBeSet()
    {
        // Arrange & Act
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Slug = "test"
        };

        // Assert
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        category.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
