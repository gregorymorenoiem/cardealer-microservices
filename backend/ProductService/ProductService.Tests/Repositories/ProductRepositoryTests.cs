using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Repositories;
using CarDealer.Shared.MultiTenancy;
using Xunit;

namespace ProductService.Tests.Repositories;

public class ProductRepositoryTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new TestTenantContext();
        var context = new ApplicationDbContext(options, tenantContext);

        // Seed categories
        var vehiclesCategory = new Category
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Vehículos",
            Slug = "vehiculos",
            Level = 0,
            IsActive = true
        };
        context.Categories.Add(vehiclesCategory);
        context.SaveChanges();

        return context;
    }

    /// <summary>
    /// Test tenant context for unit tests
    /// </summary>
    private class TestTenantContext : ITenantContext
    {
        public Guid? CurrentDealerId => null;
        public bool HasDealerContext => false;
    }

    [Fact]
    public async Task CreateAsync_ShouldAddProductToDatabase()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00m,
            CategoryId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            SellerName = "Test Seller",
            CategoryName = "Test Category"
        };

        // Act
        var result = await repository.CreateAsync(product);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Test Product");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);

        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            Slug = "test-category",
            Level = 0,
            IsActive = true
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00m,
            CategoryId = categoryId,
            SellerId = Guid.NewGuid(),
            SellerName = "Test Seller",
            CategoryName = "Test Category"
        };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        // Detach to force fresh query
        context.ChangeTracker.Clear();

        // Act
        var result = await repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnFilteredProducts_WhenSearchTermMatches()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var categoryId = Guid.NewGuid();

        var category = new Category
        {
            Id = categoryId,
            Name = "Vehicles",
            Slug = "vehicles",
            Level = 0,
            IsActive = true
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var products = new[]
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Toyota Camry",
                Description = "Excellent sedan",
                Price = 25000m,
                Status = ProductStatus.Active,
                CategoryId = categoryId,
                SellerId = Guid.NewGuid(),
                SellerName = "Seller 1",
                CategoryName = "Vehicles"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Honda Accord",
                Description = "Great car",
                Price = 23000m,
                Status = ProductStatus.Active,
                CategoryId = categoryId,
                SellerId = Guid.NewGuid(),
                SellerName = "Seller 2",
                CategoryName = "Vehicles"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "BMW X5",
                Description = "Luxury SUV",
                Price = 50000m,
                Status = ProductStatus.Sold,
                CategoryId = categoryId,
                SellerId = Guid.NewGuid(),
                SellerName = "Seller 3",
                CategoryName = "Vehicles"
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Detach to force fresh query
        context.ChangeTracker.Clear();

        // Act
        var result = await repository.SearchAsync("Toyota", null, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Toyota Camry");
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByPriceRange()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var categoryId = Guid.NewGuid();

        var category = new Category
        {
            Id = categoryId,
            Name = "Category",
            Slug = "category",
            Level = 0,
            IsActive = true
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var products = new[]
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product A",
                Description = "Cheap product",
                Price = 100m,
                Status = ProductStatus.Active,
                CategoryId = categoryId,
                SellerId = Guid.NewGuid(),
                SellerName = "Seller 1",
                CategoryName = "Category"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product B",
                Description = "Medium product",
                Price = 500m,
                Status = ProductStatus.Active,
                CategoryId = categoryId,
                SellerId = Guid.NewGuid(),
                SellerName = "Seller 2",
                CategoryName = "Category"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product C",
                Description = "Expensive product",
                Price = 1000m,
                Status = ProductStatus.Active,
                CategoryId = categoryId,
                SellerId = Guid.NewGuid(),
                SellerName = "Seller 3",
                CategoryName = "Category"
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Detach to force fresh query
        context.ChangeTracker.Clear();

        // Act
        var result = await repository.SearchAsync(null, null, 400m, 600m);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Product B");
        result.First().Price.Should().Be(500m);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteProduct()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00m,
            CategoryId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            SellerName = "Test Seller",
            CategoryName = "Test Category",
            IsDeleted = false
        };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(product.Id);

        // Assert
        var deletedProduct = await context.Products.FindAsync(product.Id);
        deletedProduct.Should().NotBeNull();
        deletedProduct!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetBySellerAsync_ShouldReturnOnlySellerProducts()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var sellerId = Guid.NewGuid();
        var otherSellerId = Guid.NewGuid();

        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Category",
            Slug = "category",
            Level = 0,
            IsActive = true
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var products = new[]
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                Description = "Description 1",
                Price = 100m,
                CategoryId = categoryId,
                SellerId = sellerId,
                SellerName = "Target Seller",
                CategoryName = "Category"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 2",
                Description = "Description 2",
                Price = 200m,
                CategoryId = categoryId,
                SellerId = sellerId,
                SellerName = "Target Seller",
                CategoryName = "Category"
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 3",
                Description = "Description 3",
                Price = 300m,
                CategoryId = categoryId,
                SellerId = otherSellerId,
                SellerName = "Other Seller",
                CategoryName = "Category"
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Detach to force fresh query
        context.ChangeTracker.Clear();

        // Act
        var result = await repository.GetBySellerAsync(sellerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.SellerId == sellerId);
    }
}
