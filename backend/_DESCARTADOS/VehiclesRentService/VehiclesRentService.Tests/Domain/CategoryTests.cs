using FluentAssertions;
using VehiclesRentService.Domain.Entities;
using Xunit;

namespace VehiclesRentService.Tests.Domain;

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
            Name = "Economy Cars",
            Slug = "economy-cars",
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
    public void Category_ForRentals_ShouldSupportRentalCategories()
    {
        // Arrange & Act
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Economy",
            Slug = "economy",
            Description = "Budget-friendly rental vehicles"
        };

        // Assert
        category.Name.Should().Be("Economy");
        category.Description.Should().Contain("Budget");
    }
}
