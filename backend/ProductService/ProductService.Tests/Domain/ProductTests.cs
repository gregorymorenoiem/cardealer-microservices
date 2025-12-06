using FluentAssertions;
using ProductService.Domain.Entities;
using Xunit;

namespace ProductService.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Product_ShouldHaveCorrectDefaultValues()
    {
        // Act
        var product = new Product();

        // Assert
        product.Currency.Should().Be("USD");
        product.Status.Should().Be(ProductStatus.Draft);
        product.CustomFieldsJson.Should().Be("{}");
        product.IsDeleted.Should().BeFalse();
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Product_ShouldAllowSettingBasicProperties()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();

        // Act
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Price = 999.99m,
            Currency = "EUR",
            Status = ProductStatus.Active,
            CategoryId = categoryId,
            CategoryName = "Test Category",
            SellerId = sellerId,
            SellerName = "Test Seller"
        };

        // Assert
        product.Id.Should().Be(productId);
        product.Name.Should().Be("Test Product");
        product.Description.Should().Be("Test Description");
        product.Price.Should().Be(999.99m);
        product.Currency.Should().Be("EUR");
        product.Status.Should().Be(ProductStatus.Active);
        product.CategoryId.Should().Be(categoryId);
        product.CategoryName.Should().Be("Test Category");
        product.SellerId.Should().Be(sellerId);
        product.SellerName.Should().Be("Test Seller");
    }

    [Fact]
    public void ProductStatus_ShouldHaveAllExpectedValues()
    {
        // Assert
        Enum.GetValues<ProductStatus>().Should().Contain(new[]
        {
            ProductStatus.Draft,
            ProductStatus.Active,
            ProductStatus.Sold,
            ProductStatus.Reserved,
            ProductStatus.Archived
        });
    }

    [Fact]
    public void ProductImage_ShouldHaveCorrectProperties()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        // Act
        var productImage = new ProductImage
        {
            Id = imageId,
            ProductId = productId,
            Url = "https://example.com/image.jpg",
            ThumbnailUrl = "https://example.com/thumb.jpg",
            SortOrder = 1,
            IsPrimary = true
        };

        // Assert
        productImage.Id.Should().Be(imageId);
        productImage.ProductId.Should().Be(productId);
        productImage.Url.Should().Be("https://example.com/image.jpg");
        productImage.ThumbnailUrl.Should().Be("https://example.com/thumb.jpg");
        productImage.SortOrder.Should().Be(1);
        productImage.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void ProductCustomField_ShouldHaveCorrectProperties()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();

        // Act
        var customField = new ProductCustomField
        {
            Id = fieldId,
            ProductId = productId,
            Key = "make",
            Value = "Toyota",
            DataType = "string",
            Unit = null,
            SortOrder = 1,
            IsSearchable = true
        };

        // Assert
        customField.Id.Should().Be(fieldId);
        customField.ProductId.Should().Be(productId);
        customField.Key.Should().Be("make");
        customField.Value.Should().Be("Toyota");
        customField.DataType.Should().Be("string");
        customField.SortOrder.Should().Be(1);
        customField.IsSearchable.Should().BeTrue();
    }

    [Fact]
    public void Category_ShouldHaveCorrectDefaultValues()
    {
        // Act
        var category = new Category();

        // Assert
        category.CustomFieldsSchemaJson.Should().Be("[]");
        category.IsActive.Should().BeTrue();
        category.Level.Should().Be(0);
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Category_ShouldSupportHierarchy()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var parent = new Category
        {
            Id = parentId,
            Name = "Vehicles",
            Slug = "vehicles",
            Level = 0,
            ParentId = null
        };

        var child = new Category
        {
            Id = childId,
            Name = "Cars",
            Slug = "cars",
            Level = 1,
            ParentId = parentId
        };

        // Assert
        parent.ParentId.Should().BeNull();
        parent.Level.Should().Be(0);
        child.ParentId.Should().Be(parentId);
        child.Level.Should().Be(1);
    }
}
