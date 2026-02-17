using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using RoleService.Infrastructure.Services;
using System.Text;
using Xunit;

namespace RoleService.Tests.Services;

/// <summary>
/// Tests para PermissionCacheService - Valida estrategia de cache Redis para permisos
/// 
/// CRÍTICO: Este servicio implementa cache-first strategy con TTL de 5 minutos
/// Key Pattern: perm:check:{roleId}:{resource}:{action}
/// Invalidation Pattern: perm:role:{roleId}:all
/// 
/// Tests validan:
/// - Cache hit/miss scenarios
/// - 5-minute TTL enforcement
/// - Key pattern generation
/// - Resilience (cache failures don't throw)
/// - Invalidation logic
/// 
/// NOTA: GetStringAsync/SetStringAsync son extension methods, mockeamos GetAsync/SetAsync base
/// </summary>
public class PermissionCacheServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<PermissionCacheService>> _loggerMock;
    private readonly PermissionCacheService _service;

    public PermissionCacheServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<PermissionCacheService>>();
        _service = new PermissionCacheService(_cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCachedPermissionCheckAsync_CacheHit_ShouldReturnCachedValue()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var resource = "vehicles";
        var action = "read";
        var cacheKey = $"perm:check:{roleId}:{resource}:{action}";
        var cachedBytes = Encoding.UTF8.GetBytes("1"); // true serializado como "1"
        
        _cacheMock.Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        // Act
        var result = await _service.GetCachedPermissionCheckAsync(roleId, resource, action);

        // Assert
        result.Should().BeTrue();
        _cacheMock.Verify(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCachedPermissionCheckAsync_CacheMiss_ShouldReturnNull()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var resource = "users";
        var action = "update";
        var cacheKey = $"perm:check:{roleId}:{resource}:{action}";
        
        _cacheMock.Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _service.GetCachedPermissionCheckAsync(roleId, resource, action);

        // Assert
        result.Should().BeNull();
        _cacheMock.Verify(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetCachedPermissionCheckAsync_ShouldStoreTrueValue()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var resource = "dealers";
        var action = "delete";
        var hasPermission = true;
        var cacheKey = $"perm:check:{roleId}:{resource}:{action}";

        _cacheMock.Setup(c => c.SetAsync(
            cacheKey,
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "1"),
            It.Is<DistributedCacheEntryOptions>(opts => 
                opts.AbsoluteExpirationRelativeToNow.HasValue &&
                opts.AbsoluteExpirationRelativeToNow.Value.TotalMinutes >= 4 && // 5 min TTL (con tolerancia ±1)
                opts.AbsoluteExpirationRelativeToNow.Value.TotalMinutes <= 6),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetCachedPermissionCheckAsync(roleId, resource, action, hasPermission);

        // Assert
        _cacheMock.Verify(c => c.SetAsync(
            cacheKey,
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "1"),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetCachedPermissionCheckAsync_ShouldStoreFalseValue()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var resource = "admin";
        var action = "access";
        var hasPermission = false;
        var cacheKey = $"perm:check:{roleId}:{resource}:{action}";

        _cacheMock.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetCachedPermissionCheckAsync(roleId, resource, action, hasPermission);

        // Assert
        _cacheMock.Verify(c => c.SetAsync(
            cacheKey,
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "0"),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateRolePermissionsAsync_ShouldRemoveRoleKey()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var invalidationKey = $"perm:role:{roleId}";

        _cacheMock.Setup(c => c.RemoveAsync(invalidationKey, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.InvalidateRolePermissionsAsync(roleId);

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync(invalidationKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("vehicles", "read")]
    [InlineData("users", "update")]
    [InlineData("dealers", "delete")]
    [InlineData("admin", "access")]
    public async Task GetCachedPermissionCheckAsync_DifferentResourceActions_ShouldUseDifferentKeys(
        string resource, 
        string action)
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var expectedKey = $"perm:check:{roleId}:{resource}:{action}";
        
        var cachedBytes = Encoding.UTF8.GetBytes("1");
        _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        // Act
        var result = await _service.GetCachedPermissionCheckAsync(roleId, resource, action);

        // Assert
        result.Should().BeTrue();
        _cacheMock.Verify(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CacheFailure_ShouldLogWarningAndReturnNull()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var resource = "vehicles";
        var action = "read";

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Redis connection lost"));

        // Act
        var result = await _service.GetCachedPermissionCheckAsync(roleId, resource, action);

        // Assert
        result.Should().BeNull(); // Debe ser resiliente: cache failure devuelve null, no lanza excepción
        
        // Verificar que se loguea el warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error getting cached permission check")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SetCachedPermissionCheckAsync_WithTTL_ShouldRespectCheckDuration()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var resource = "vehicles";
        var action = "read";
        var hasPermission = true;

        DistributedCacheEntryOptions? capturedOptions = null;
        
        _cacheMock.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (key, value, opts, ct) => capturedOptions = opts)
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetCachedPermissionCheckAsync(roleId, resource, action, hasPermission);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.AbsoluteExpirationRelativeToNow.Should().NotBeNull();
        
        // Verificar que el TTL es aproximadamente 5 minutos (con tolerancia de ±1 min)
        var ttl = capturedOptions.AbsoluteExpirationRelativeToNow!.Value;
        ttl.TotalMinutes.Should().BeInRange(4, 6);
    }
}
