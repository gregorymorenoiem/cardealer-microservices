using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.External
{
    public class RoleServiceClient : IRoleServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RoleServiceClient> _logger;

        public RoleServiceClient(HttpClient httpClient, ILogger<RoleServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> RoleExistsAsync(Guid roleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/roles/{roleId}");
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
                return await _httpClient.GetFromJsonAsync<RoleDetailsDto>($"/api/roles/{roleId}");
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
                var results = new List<RoleDetailsDto>();

                foreach (var roleId in roleIds)
                {
                    var role = await GetRoleByIdAsync(roleId);
                    if (role != null)
                    {
                        results.Add(role);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles by IDs from RoleService");
                return new List<RoleDetailsDto>();
            }
        }
    }
}
