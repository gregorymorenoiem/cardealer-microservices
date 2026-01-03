using FluentAssertions;
using PropertiesSaleService.Domain.Entities;
using Xunit;

namespace PropertiesSaleService.Tests.Domain;

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
        category.Properties.Should().NotBeNull();
        category.Properties.Should().BeEmpty();
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
            Name = "Residential",
            Slug = "residential"
        };

        var childCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Single Family Homes",
            Slug = "single-family-homes",
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
    public void Category_ForSale_ShouldSupportPropertyTypes()
    {
        // Arrange & Act
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Single Family", Slug = "single-family" },
            new() { Id = Guid.NewGuid(), Name = "Condos", Slug = "condos" },
            new() { Id = Guid.NewGuid(), Name = "Townhouses", Slug = "townhouses" },
            new() { Id = Guid.NewGuid(), Name = "Multi-Family", Slug = "multi-family" },
            new() { Id = Guid.NewGuid(), Name = "Land", Slug = "land" },
            new() { Id = Guid.NewGuid(), Name = "Commercial", Slug = "commercial" }
        };

        // Assert
        categories.Should().HaveCount(6);
        categories.Should().AllSatisfy(c => c.Id.Should().NotBe(Guid.Empty));
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
