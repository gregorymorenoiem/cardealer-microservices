using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using UserService.Application.DTOs;
using UserService.Infrastructure.External;
using ServiceDiscovery.Application.Interfaces;
using Xunit;

namespace UserService.Tests.Infrastructure
{
    public class RoleServiceClientTests
    {
        private readonly Mock<ILogger<RoleServiceClient>> _loggerMock;
        private readonly Mock<IServiceDiscovery> _serviceDiscoveryMock;
        private readonly IMemoryCache _cache;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;

        public RoleServiceClientTests()
        {
            _loggerMock = new Mock<ILogger<RoleServiceClient>>();
            _serviceDiscoveryMock = new Mock<IServiceDiscovery>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://roleservice:80")
            };

            // Setup service discovery to return mock URL
            _serviceDiscoveryMock
                .Setup(x => x.FindServiceInstanceAsync("RoleService", It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceDiscovery.Domain.Entities.ServiceInstance?)null);
        }

        [Fact]
        public async Task RoleExistsAsync_RoleExists_ReturnsTrue()
        {
            // Arrange
            var roleId = Guid.NewGuid();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var client = new RoleServiceClient(_httpClient, _loggerMock.Object, _serviceDiscoveryMock.Object, _cache);

            // Act
            var result = await client.RoleExistsAsync(roleId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RoleExistsAsync_RoleNotFound_ReturnsFalse()
        {
            // Arrange
            var roleId = Guid.NewGuid();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            var client = new RoleServiceClient(_httpClient, _loggerMock.Object, _serviceDiscoveryMock.Object, _cache);

            // Act
            var result = await client.RoleExistsAsync(roleId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetRoleByIdAsync_WithValidRole_ReturnsRoleWithPermissions()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var apiResponse = new
            {
                Data = new
                {
                    Id = roleId,
                    Name = "Admin",
                    Description = "Administrator Role",
                    Priority = 1,
                    IsActive = true,
                    IsSystemRole = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null,
                    CreatedBy = "system",
                    UpdatedBy = (string?)null,
                    Permissions = new[]
                    {
                        new
                        {
                            Id = Guid.NewGuid(),
                            Name = "users.create",
                            Description = "Create users",
                            Resource = "users",
                            Action = "create",
                            Module = "UserManagement"
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(apiResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                });

            var client = new RoleServiceClient(_httpClient, _loggerMock.Object, _serviceDiscoveryMock.Object, _cache);

            // Act
            var result = await client.GetRoleByIdAsync(roleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(roleId, result.Id);
            Assert.Equal("Admin", result.Name);
            Assert.Single(result.Permissions);
            Assert.Equal("users.create", result.Permissions[0].Name);
        }

        [Fact]
        public async Task GetRoleByIdAsync_CachedRole_DoesNotCallService()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var cachedRole = new RoleDetailsDto
            {
                Id = roleId,
                Name = "CachedRole",
                Description = "Cached",
                Priority = 1,
                IsActive = true,
                IsSystemRole = false,
                Permissions = new List<PermissionDto>()
            };

            // Pre-populate cache
            _cache.Set($"role:{roleId}", cachedRole, TimeSpan.FromMinutes(5));

            var client = new RoleServiceClient(_httpClient, _loggerMock.Object, _serviceDiscoveryMock.Object, _cache);

            // Act
            var result = await client.GetRoleByIdAsync(roleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(roleId, result.Id);
            Assert.Equal("CachedRole", result.Name);

            // Verify HTTP was NOT called (no setup means no call should occur)
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetRolesByIdsAsync_MultipleRoles_ReturnsAllRoles()
        {
            // Arrange
            var roleId1 = Guid.NewGuid();
            var roleId2 = Guid.NewGuid();
            var roleIds = new List<Guid> { roleId1, roleId2 };

            var apiResponse1 = new
            {
                Data = new
                {
                    Id = roleId1,
                    Name = "Admin",
                    Description = "Admin Role",
                    Priority = 1,
                    IsActive = true,
                    IsSystemRole = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null,
                    CreatedBy = "system",
                    UpdatedBy = (string?)null,
                    Permissions = Array.Empty<object>()
                }
            };

            var apiResponse2 = new
            {
                Data = new
                {
                    Id = roleId2,
                    Name = "User",
                    Description = "User Role",
                    Priority = 2,
                    IsActive = true,
                    IsSystemRole = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null,
                    CreatedBy = "system",
                    UpdatedBy = (string?)null,
                    Permissions = Array.Empty<object>()
                }
            };

            var callCount = 0;
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(() =>
                {
                    callCount++;
                    var json = callCount == 1
                        ? JsonSerializer.Serialize(apiResponse1)
                        : JsonSerializer.Serialize(apiResponse2);

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                    };
                });

            var client = new RoleServiceClient(_httpClient, _loggerMock.Object, _serviceDiscoveryMock.Object, _cache);

            // Act
            var result = await client.GetRolesByIdsAsync(roleIds);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Name == "Admin");
            Assert.Contains(result, r => r.Name == "User");
        }

        [Fact]
        public async Task GetRolesByIdsAsync_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var client = new RoleServiceClient(_httpClient, _loggerMock.Object, _serviceDiscoveryMock.Object, _cache);

            // Act
            var result = await client.GetRolesByIdsAsync(new List<Guid>());

            // Assert
            Assert.Empty(result);

            // Verify no HTTP calls were made
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetRolesByIdsAsync_WithCachedAndNonCached_FetchesOnlyMissing()
        {
            // Arrange
            var cachedRoleId = Guid.NewGuid();
            var nonCachedRoleId = Guid.NewGuid();

            var cachedRole = new RoleDetailsDto
            {
                Id = cachedRoleId,
                Name = "CachedRole",
                Description = "Cached",
                Priority = 1,
                IsActive = true,
                IsSystemRole = false,
                Permissions = new List<PermissionDto>()
            };

            // Pre-populate cache with one role
            _cache.Set($"role:{cachedRoleId}", cachedRole, TimeSpan.FromMinutes(5));

            var apiResponse = new
            {
                Data = new
                {
                    Id = nonCachedRoleId,
                    Name = "FetchedRole",
                    Description = "Fetched",
                    Priority = 2,
                    IsActive = true,
                    IsSystemRole = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null,
                    CreatedBy = "system",
                    UpdatedBy = (string?)null,
                    Permissions = Array.Empty<object>()
                }
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(apiResponse), System.Text.Encoding.UTF8, "application/json")
                });

            var client = new RoleServiceClient(_httpClient, _loggerMock.Object, _serviceDiscoveryMock.Object, _cache);

            // Act
            var result = await client.GetRolesByIdsAsync(new List<Guid> { cachedRoleId, nonCachedRoleId });

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Name == "CachedRole");
            Assert.Contains(result, r => r.Name == "FetchedRole");

            // Verify HTTP was called only once (for non-cached role)
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
