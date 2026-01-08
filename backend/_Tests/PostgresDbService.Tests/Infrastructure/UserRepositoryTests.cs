using Xunit;
using FluentAssertions;
using PostgresDbService.Infrastructure.Repositories;
using PostgresDbService.Tests.Helpers;
using System.Text.Json;

namespace PostgresDbService.Tests.Infrastructure;

/// <summary>
/// Tests for UserRepository
/// </summary>
public class UserRepositoryTests : IDisposable
{
    private readonly Infrastructure.Persistence.CentralizedDbContext _context;
    private readonly GenericRepository _genericRepository;
    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext(nameof(UserRepositoryTests));
        _genericRepository = new GenericRepository(_context);
        _userRepository = new UserRepository(_genericRepository);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateUser_WithCorrectData()
    {
        // Arrange
        var userData = new
        {
            Email = "newuser@test.com",
            FullName = "New Test User",
            Role = "Buyer",
            IsActive = true,
            City = "Santo Domingo",
            Province = "Distrito Nacional",
            Phone = "809-555-0123"
        };

        // Act
        var result = await _userRepository.CreateUserAsync(userData, "system");

        // Assert
        result.Should().NotBeNull();
        result.ServiceName.Should().Be("UserService");
        result.EntityType.Should().Be("User");
        result.CreatedBy.Should().Be("system");
        
        var data = JsonSerializer.Deserialize<JsonElement>(result.DataJson);
        data.GetProperty("Email").GetString().Should().Be("newuser@test.com");
        data.GetProperty("FullName").GetString().Should().Be("New Test User");
        data.GetProperty("Role").GetString().Should().Be("Buyer");
    }

    [Fact]
    public async Task GetUserByEmailAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var userData = new
        {
            Email = "findme@test.com",
            FullName = "Find Me User",
            Role = "Seller"
        };
        await _userRepository.CreateUserAsync(userData, "system");

        // Act
        var result = await _userRepository.GetUserByEmailAsync("findme@test.com");

        // Assert
        result.Should().NotBeNull();
        var data = JsonSerializer.Deserialize<JsonElement>(result!.DataJson);
        data.GetProperty("Email").GetString().Should().Be("findme@test.com");
        data.GetProperty("FullName").GetString().Should().Be("Find Me User");
    }

    [Fact]
    public async Task GetUserByEmailAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _userRepository.GetUserByEmailAsync("notfound@test.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var userData = new { Email = "getbyid@test.com", FullName = "Get By ID User", Role = "Admin" };
        var created = await _userRepository.CreateUserAsync(userData, "system");
        var userId = Guid.Parse(created.EntityId);

        // Act
        var result = await _userRepository.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.EntityId.Should().Be(userId.ToString());
        var data = JsonSerializer.Deserialize<JsonElement>(result.DataJson);
        data.GetProperty("Role").GetString().Should().Be("Admin");
    }

    [Fact]
    public async Task GetUsersByRoleAsync_ShouldReturnMatchingUsers()
    {
        // Arrange
        var users = new[]
        {
            new { Email = "buyer1@test.com", FullName = "Buyer 1", Role = "Buyer" },
            new { Email = "buyer2@test.com", FullName = "Buyer 2", Role = "Buyer" },
            new { Email = "seller1@test.com", FullName = "Seller 1", Role = "Seller" }
        };

        foreach (var user in users)
        {
            await _userRepository.CreateUserAsync(user, "system");
        }

        // Act
        var buyers = await _userRepository.GetUsersByRoleAsync("Buyer");

        // Assert
        buyers.Should().HaveCount(2);
        buyers.All(u => 
        {
            var data = JsonSerializer.Deserialize<JsonElement>(u.DataJson);
            return data.GetProperty("Role").GetString() == "Buyer";
        }).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateUserData()
    {
        // Arrange
        var originalData = new
        {
            Email = "update@test.com",
            FullName = "Original Name",
            Role = "Buyer",
            City = "Santiago"
        };
        var created = await _userRepository.CreateUserAsync(originalData, "system");
        var userId = Guid.Parse(created.EntityId);

        var updateData = new
        {
            Email = "update@test.com",
            FullName = "Updated Name",
            Role = "Seller",
            City = "Santo Domingo",
            Phone = "809-123-4567"
        };

        // Act
        var result = await _userRepository.UpdateUserAsync(userId, updateData, "updater");

        // Assert
        result.Should().NotBeNull();
        result.UpdatedBy.Should().Be("updater");
        result.UpdatedAt.Should().NotBeNull();
        
        var data = JsonSerializer.Deserialize<JsonElement>(result.DataJson);
        data.GetProperty("FullName").GetString().Should().Be("Updated Name");
        data.GetProperty("Role").GetString().Should().Be("Seller");
        data.GetProperty("City").GetString().Should().Be("Santo Domingo");
        data.GetProperty("Phone").GetString().Should().Be("809-123-4567");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}