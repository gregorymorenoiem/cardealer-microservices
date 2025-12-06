using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Services
{
    /// <summary>
    /// Servicio compartido para verificar acceso a módulos add-on
    /// Usado por TODOS los microservicios que implementan features vendibles
    /// </summary>
    public interface IModuleAccessService
    {
        /// <summary>
        /// Verifica si un dealer tiene acceso a un módulo específico
        /// </summary>
        Task<bool> HasModuleAccessAsync(Guid dealerId, string moduleCode);

        /// <summary>
        /// Obtiene lista de módulos activos para un dealer
        /// </summary>
        Task<List<string>> GetActiveModulesAsync(Guid dealerId);

        /// <summary>
        /// Verifica múltiples módulos a la vez
        /// </summary>
        Task<Dictionary<string, bool>> HasModulesAccessAsync(Guid dealerId, params string[] moduleCodes);

        /// <summary>
        /// Invalida cache de módulos para un dealer (cuando cambia suscripción)
        /// </summary>
        Task InvalidateCacheAsync(Guid dealerId);
    }

    public class ModuleAccessService : IModuleAccessService
    {
        private readonly IDistributedCache _cache;
        private readonly HttpClient _userServiceClient;
        private readonly ILogger<ModuleAccessService> _logger;

        // Cache por 5 minutos (cambios de suscripción no son frecuentes)
        private const int CACHE_MINUTES = 5;

        public ModuleAccessService(
            IDistributedCache cache,
            IHttpClientFactory httpClientFactory,
            ILogger<ModuleAccessService> logger)
        {
            _cache = cache;
            _userServiceClient = httpClientFactory.CreateClient("UserService");
            _logger = logger;
        }

        public async Task<bool> HasModuleAccessAsync(Guid dealerId, string moduleCode)
        {
            try
            {
                var activeModules = await GetActiveModulesAsync(dealerId);
                return activeModules.Contains(moduleCode, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking module access for dealer {DealerId}, module {ModuleCode}",
                    dealerId, moduleCode);

                // En caso de error, DENEGAR acceso por seguridad
                return false;
            }
        }

        public async Task<List<string>> GetActiveModulesAsync(Guid dealerId)
        {
            var cacheKey = GetCacheKey(dealerId);

            try
            {
                // 1. Check cache (Redis)
                var cachedModules = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedModules))
                {
                    _logger.LogDebug("Module access cache HIT for dealer {DealerId}", dealerId);
                    return JsonSerializer.Deserialize<List<string>>(cachedModules) ?? new List<string>();
                }

                _logger.LogDebug("Module access cache MISS for dealer {DealerId}, fetching from UserService", dealerId);

                // 2. Query UserService API
                var response = await _userServiceClient.GetAsync($"/api/dealers/{dealerId}/active-modules");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch modules for dealer {DealerId}. Status: {StatusCode}",
                        dealerId, response.StatusCode);
                    return new List<string>();
                }

                var activeModules = await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();

                // 3. Cache por 5 minutos
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_MINUTES)
                };

                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(activeModules),
                    cacheOptions
                );

                _logger.LogInformation("Cached {Count} active modules for dealer {DealerId}",
                    activeModules.Count, dealerId);

                return activeModules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active modules for dealer {DealerId}", dealerId);
                return new List<string>();
            }
        }

        public async Task<Dictionary<string, bool>> HasModulesAccessAsync(Guid dealerId, params string[] moduleCodes)
        {
            var result = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var activeModules = await GetActiveModulesAsync(dealerId);

                foreach (var moduleCode in moduleCodes)
                {
                    result[moduleCode] = activeModules.Contains(moduleCode, StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking multiple modules access for dealer {DealerId}", dealerId);

                // En caso de error, DENEGAR todos
                foreach (var moduleCode in moduleCodes)
                {
                    result[moduleCode] = false;
                }
            }

            return result;
        }

        public async Task InvalidateCacheAsync(Guid dealerId)
        {
            var cacheKey = GetCacheKey(dealerId);
            await _cache.RemoveAsync(cacheKey);
            _logger.LogInformation("Invalidated module access cache for dealer {DealerId}", dealerId);
        }

        private static string GetCacheKey(Guid dealerId) => $"dealer:{dealerId}:modules";
    }

    /// <summary>
    /// DTO para respuesta de módulos activos desde UserService
    /// </summary>
    public class DealerModulesResponse
    {
        public Guid DealerId { get; set; }
        public string Plan { get; set; } = string.Empty;
        public List<string> ActiveModules { get; set; } = new();
        public Dictionary<string, DateTime?> ModuleExpirations { get; set; } = new();
    }
}
