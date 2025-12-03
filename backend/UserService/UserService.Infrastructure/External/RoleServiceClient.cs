using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using ServiceDiscovery.Application.Interfaces;

namespace UserService.Infrastructure.External
{
    public class RoleServiceClient : IRoleServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RoleServiceClient> _logger;
        private readonly IServiceDiscovery _serviceDiscovery;
        private readonly IMemoryCache _cache;
        private const int CacheTtlMinutes = 5;

        public RoleServiceClient(
            HttpClient httpClient,
            ILogger<RoleServiceClient> logger,
            IServiceDiscovery serviceDiscovery,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serviceDiscovery = serviceDiscovery;
            _cache = cache;
        }

        private async Task<string> GetServiceUrlAsync()
        {
            try
            {
                var instance = await _serviceDiscovery.FindServiceInstanceAsync("RoleService");
                return instance != null ? $"http://{instance.Host}:{instance.Port}" : "http://roleservice:80";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error resolving RoleService from Consul, using fallback");
                return "http://roleservice:80";
            }
        }

        public async Task<bool> RoleExistsAsync(Guid roleId)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var response = await _httpClient.GetAsync($"{baseUrl}/api/roles/{roleId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if role {RoleId} exists in RoleService", roleId);
                return false;
            }
        }

        public async Task<RoleDetailsDto?> GetRoleByIdAsync(Guid roleId)
        {
            try
            {
                // Check cache first
                var cacheKey = $"role:{roleId}";
                if (_cache.TryGetValue(cacheKey, out RoleDetailsDto? cachedRole))
                {
                    _logger.LogDebug("Role {RoleId} retrieved from cache", roleId);
                    return cachedRole;
                }

                var baseUrl = await GetServiceUrlAsync();
                var response = await _httpClient.GetAsync($"{baseUrl}/api/roles/{roleId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Role {RoleId} not found in RoleService. Status: {StatusCode}",
                        roleId, response.StatusCode);
                    return null;
                }

                // Parse the ApiResponse wrapper from RoleService
                var apiResponse = await response.Content.ReadFromJsonAsync<RoleServiceApiResponse>();
                if (apiResponse?.Data == null)
                {
                    _logger.LogWarning("Role {RoleId} returned null data from RoleService", roleId);
                    return null;
                }

                // Map from RoleService DTO to UserService DTO
                var roleDetails = MapToUserServiceDto(apiResponse.Data);

                // Cache the result for 5 minutes
                _cache.Set(cacheKey, roleDetails, TimeSpan.FromMinutes(CacheTtlMinutes));

                _logger.LogDebug("Role {RoleId} cached for {Minutes} minutes", roleId, CacheTtlMinutes);

                return roleDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role {RoleId} from RoleService", roleId);
                return null;
            }
        }

        public async Task<List<RoleDetailsDto>> GetRolesByIdsAsync(List<Guid> roleIds)
        {
            try
            {
                if (!roleIds.Any())
                    return new List<RoleDetailsDto>();

                // Check cache for all roles first
                var cachedRoles = new List<RoleDetailsDto>();
                var missingRoleIds = new List<Guid>();

                foreach (var roleId in roleIds)
                {
                    var cacheKey = $"role:{roleId}";
                    if (_cache.TryGetValue(cacheKey, out RoleDetailsDto? cachedRole) && cachedRole != null)
                    {
                        cachedRoles.Add(cachedRole);
                    }
                    else
                    {
                        missingRoleIds.Add(roleId);
                    }
                }

                _logger.LogDebug("Found {CachedCount} cached roles, fetching {MissingCount} from service",
                    cachedRoles.Count, missingRoleIds.Count);

                // Fetch missing roles in parallel
                if (missingRoleIds.Any())
                {
                    var fetchTasks = missingRoleIds.Select(id => GetRoleByIdAsync(id));
                    var fetchedRoles = await Task.WhenAll(fetchTasks);

                    cachedRoles.AddRange(fetchedRoles.Where(r => r != null)!);
                }

                return cachedRoles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles by IDs from RoleService");
                return new List<RoleDetailsDto>();
            }
        }

        /// <summary>
        /// Maps RoleService DTO to UserService DTO
        /// </summary>
        private RoleDetailsDto MapToUserServiceDto(RoleServiceRoleDetailsDto roleServiceDto)
        {
            return new RoleDetailsDto
            {
                Id = roleServiceDto.Id,
                Name = roleServiceDto.Name,
                Description = roleServiceDto.Description,
                Priority = roleServiceDto.Priority,
                IsActive = roleServiceDto.IsActive,
                IsSystemRole = roleServiceDto.IsSystemRole,
                Permissions = roleServiceDto.Permissions.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Resource = p.Resource,
                    Action = p.Action,
                    Module = p.Module
                }).ToList()
            };
        }

        /// <summary>
        /// API Response wrapper from RoleService
        /// </summary>
        private class RoleServiceApiResponse
        {
            public RoleServiceRoleDetailsDto? Data { get; set; }
        }

        /// <summary>
        /// RoleService's RoleDetailsDto (record type from RoleService)
        /// </summary>
        private class RoleServiceRoleDetailsDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int Priority { get; set; }
            public bool IsActive { get; set; }
            public bool IsSystemRole { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public string CreatedBy { get; set; } = string.Empty;
            public string? UpdatedBy { get; set; }
            public List<RoleServicePermissionDto> Permissions { get; set; } = new();
        }

        private class RoleServicePermissionDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Resource { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public string Module { get; set; } = string.Empty;
        }
    }
}
