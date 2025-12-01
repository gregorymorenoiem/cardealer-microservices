using System.Net.Http.Json;

namespace UserService.Infrastructure.Services
{
    /// <summary>
    /// Cliente HTTP para comunicarse con RoleService
    /// </summary>
    public interface IRoleServiceClient
    {
        Task<bool> RoleExistsAsync(Guid roleId);
        Task<List<RoleDto>> GetRolesByIdsAsync(List<Guid> roleIds);
        Task<List<PermissionDto>> GetRolePermissionsAsync(Guid roleId);
        Task<bool> CheckUserPermissionAsync(Guid userId, string resource, string action);
    }

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
                _logger.LogError(ex, "Error checking if role {RoleId} exists", roleId);
                return false;
            }
        }

        public async Task<List<RoleDto>> GetRolesByIdsAsync(List<Guid> roleIds)
        {
            try
            {
                // Opción 1: Llamadas individuales
                var tasks = roleIds.Select(id => _httpClient.GetFromJsonAsync<RoleDto>($"/api/roles/{id}"));
                var results = await Task.WhenAll(tasks);
                return results.Where(r => r != null).ToList()!;

                // Opción 2: Endpoint batch (si existe en RoleService)
                // var response = await _httpClient.PostAsJsonAsync("/api/roles/batch", roleIds);
                // return await response.Content.ReadFromJsonAsync<List<RoleDto>>() ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles by IDs");
                return new List<RoleDto>();
            }
        }

        public async Task<List<PermissionDto>> GetRolePermissionsAsync(Guid roleId)
        {
            try
            {
                var role = await _httpClient.GetFromJsonAsync<RoleDetailsDto>($"/api/roles/{roleId}");
                return role?.Permissions ?? new List<PermissionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for role {RoleId}", roleId);
                return new List<PermissionDto>();
            }
        }

        public async Task<bool> CheckUserPermissionAsync(Guid userId, string resource, string action)
        {
            // Esta lógica la implementarías en UserService:
            // 1. Obtener roles del usuario desde UserRoles
            // 2. Para cada rol, verificar permisos en RoleService
            // 3. Retornar true si algún rol tiene el permiso

            throw new NotImplementedException("Check permission logic needed");
        }
    }

    // DTOs para comunicación con RoleService
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoleDetailsDto : RoleDto
    {
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    public class PermissionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
    }
}

// Configuración en Program.cs de UserService:
// builder.Services.AddHttpClient<IRoleServiceClient, RoleServiceClient>(client =>
// {
//     client.BaseAddress = new Uri("https://localhost:5001"); // URL de RoleService
//     client.Timeout = TimeSpan.FromSeconds(30);
// });
