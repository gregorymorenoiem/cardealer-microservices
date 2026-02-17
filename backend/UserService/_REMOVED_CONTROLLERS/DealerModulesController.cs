using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Api.Controllers
{
    /// <summary>
    /// API para consultar módulos add-on activos de dealers
    /// Usado por otros microservicios para verificar acceso
    /// </summary>
    [ApiController]
    [Route("api/dealers")]
    [Authorize]
    public class DealerModulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DealerModulesController> _logger;

        public DealerModulesController(
            ApplicationDbContext context,
            ILogger<DealerModulesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene lista de códigos de módulos activos para un dealer
        /// Incluye módulos del plan base + add-ons suscritos
        /// </summary>
        /// <param name="dealerId">ID del dealer</param>
        /// <returns>Lista de códigos de módulos (ej: ["crm-advanced", "invoicing-cfdi"])</returns>
        [HttpGet("{dealerId}/active-modules")]
        [ProducesResponseType(typeof(List<string>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetActiveModules(Guid dealerId)
        {
            try
            {
                // 1. Obtener suscripción del dealer
                var subscription = await _context.DealerSubscriptions
                    .FirstOrDefaultAsync(s => s.DealerId == dealerId && s.Status == SubscriptionStatus.Active);

                if (subscription == null)
                {
                    _logger.LogWarning("No active subscription found for dealer {DealerId}", dealerId);
                    return Ok(new List<string>()); // Sin suscripción = sin módulos
                }

                // 2. Obtener features del plan base
                var planFeatures = DealerPlanLimits.GetFeatures(subscription.Plan);
                var activeModules = new List<string>();

                // 3. Mapear features booleanos a códigos de módulos
                if (planFeatures.CRMAdvanced) activeModules.Add("crm-advanced");
                if (planFeatures.InvoicingCFDI) activeModules.Add("invoicing-cfdi");
                if (planFeatures.FinanceAdvanced) activeModules.Add("finance-accounting");
                if (planFeatures.MarketingAutomation) activeModules.Add("marketing-automation");
                if (planFeatures.WhatsappIntegration) activeModules.Add("integration-whatsapp");
                if (planFeatures.PublicAPI) activeModules.Add("integration-public-api");
                if (planFeatures.AdvancedReports) activeModules.Add("analytics-reports");

                // 4. Agregar módulos add-on suscritos (adicionales al plan)
                var addOnModules = await _context.DealerModuleSubscriptions
                    .Include(dms => dms.ModuleAddon)
                    .Where(dms => dms.DealerId == dealerId &&
                                  (dms.Status == SubscriptionStatus.Active || dms.Status == SubscriptionStatus.Trial) &&
                                  (dms.EndDate == null || dms.EndDate > DateTime.UtcNow))
                    .Select(dms => dms.ModuleAddon.Code)
                    .ToListAsync();

                activeModules.AddRange(addOnModules);

                // 5. Remover duplicados
                activeModules = activeModules.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                _logger.LogInformation(
                    "Dealer {DealerId} has {Count} active modules: {Modules}",
                    dealerId, activeModules.Count, string.Join(", ", activeModules));

                return Ok(activeModules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active modules for dealer {DealerId}", dealerId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene información detallada de módulos activos
        /// </summary>
        [HttpGet("{dealerId}/modules-details")]
        [ProducesResponseType(typeof(DealerModulesDetailResponse), 200)]
        public async Task<IActionResult> GetModulesDetails(Guid dealerId)
        {
            try
            {
                var subscription = await _context.DealerSubscriptions
                    .FirstOrDefaultAsync(s => s.DealerId == dealerId && s.Status == SubscriptionStatus.Active);

                if (subscription == null)
                {
                    return Ok(new DealerModulesDetailResponse
                    {
                        DealerId = dealerId,
                        Plan = "None",
                        ActiveModules = new List<string>(),
                        ModuleDetails = new List<ModuleDetail>()
                    });
                }

                // Módulos incluidos en plan base
                var planFeatures = DealerPlanLimits.GetFeatures(subscription.Plan);
                var includedModules = GetIncludedModulesFromPlan(planFeatures);

                // Módulos add-on
                var addOnModules = await _context.DealerModuleSubscriptions
                    .Include(dms => dms.ModuleAddon)
                    .Where(dms => dms.DealerId == dealerId &&
                                  (dms.Status == SubscriptionStatus.Active || dms.Status == SubscriptionStatus.Trial) &&
                                  (dms.EndDate == null || dms.EndDate > DateTime.UtcNow))
                    .ToListAsync();

                var moduleDetails = new List<ModuleDetail>();

                // Detalles de módulos incluidos
                foreach (var moduleCode in includedModules)
                {
                    moduleDetails.Add(new ModuleDetail
                    {
                        Code = moduleCode,
                        Source = "plan",
                        Status = "active",
                        ExpiresAt = subscription.EndDate
                    });
                }

                // Detalles de add-ons
                foreach (var addOn in addOnModules)
                {
                    moduleDetails.Add(new ModuleDetail
                    {
                        Code = addOn.ModuleAddon.Code,
                        Source = addOn.Status == SubscriptionStatus.Trial ? "trial" : "addon",
                        Status = addOn.Status.ToString().ToLower(),
                        ExpiresAt = addOn.Status == SubscriptionStatus.Trial ? addOn.TrialEndDate : addOn.EndDate,
                        MonthlyPrice = addOn.MonthlyPrice
                    });
                }

                var response = new DealerModulesDetailResponse
                {
                    DealerId = dealerId,
                    Plan = subscription.Plan.ToString(),
                    ActiveModules = moduleDetails.Select(m => m.Code).Distinct().ToList(),
                    ModuleDetails = moduleDetails
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching modules details for dealer {DealerId}", dealerId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Suscribir dealer a un módulo add-on
        /// </summary>
        [HttpPost("{dealerId}/modules/{moduleCode}/subscribe")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SubscribeToModule(Guid dealerId, string moduleCode)
        {
            try
            {
                // 1. Verificar que el dealer existe y tiene suscripción activa
                var subscription = await _context.DealerSubscriptions
                    .FirstOrDefaultAsync(s => s.DealerId == dealerId && s.Status == SubscriptionStatus.Active);

                if (subscription == null)
                {
                    return BadRequest(new { error = "No active subscription found" });
                }

                // 2. Verificar que el módulo existe
                var module = await _context.ModuleAddons
                    .FirstOrDefaultAsync(m => m.Code == moduleCode && m.IsActive);

                if (module == null)
                {
                    return NotFound(new { error = "Module not found" });
                }

                // 3. Verificar si ya está suscrito
                var existingSubscription = await _context.DealerModuleSubscriptions
                    .FirstOrDefaultAsync(dms => dms.DealerId == dealerId &&
                                                dms.ModuleAddonId == module.Id &&
                                                dms.Status == SubscriptionStatus.Active);

                if (existingSubscription != null)
                {
                    return BadRequest(new { error = "Already subscribed to this module" });
                }

                // 4. Crear suscripción (con trial si está disponible)
                var newSubscription = new DealerModuleSubscription
                {
                    Id = Guid.NewGuid(),
                    DealerId = dealerId,
                    ModuleAddonId = module.Id,
                    Status = module.HasFreeTrial ? SubscriptionStatus.Trial : SubscriptionStatus.Active,
                    StartDate = DateTime.UtcNow,
                    TrialEndDate = module.HasFreeTrial ? DateTime.UtcNow.AddDays(module.TrialDays) : null,
                    MonthlyPrice = module.MonthlyPrice,
                    IsYearlyBilling = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DealerModuleSubscriptions.Add(newSubscription);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Dealer {DealerId} subscribed to module {ModuleCode} (Trial: {IsTrial})",
                    dealerId, moduleCode, module.HasFreeTrial);

                return Ok(new
                {
                    message = module.HasFreeTrial
                        ? $"Trial started. Expires in {module.TrialDays} days."
                        : "Module activated successfully",
                    status = newSubscription.Status.ToString().ToLower(),
                    expiresAt = newSubscription.TrialEndDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing dealer {DealerId} to module {ModuleCode}", dealerId, moduleCode);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Cancelar suscripción a módulo add-on
        /// </summary>
        [HttpDelete("{dealerId}/modules/{moduleCode}/unsubscribe")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UnsubscribeFromModule(Guid dealerId, string moduleCode)
        {
            try
            {
                var module = await _context.ModuleAddons
                    .FirstOrDefaultAsync(m => m.Code == moduleCode);

                if (module == null)
                {
                    return NotFound(new { error = "Module not found" });
                }

                var subscription = await _context.DealerModuleSubscriptions
                    .FirstOrDefaultAsync(dms => dms.DealerId == dealerId &&
                                                dms.ModuleAddonId == module.Id &&
                                                dms.Status == SubscriptionStatus.Active);

                if (subscription == null)
                {
                    return NotFound(new { error = "No active subscription found for this module" });
                }

                // Cancelar (no eliminar, por historial)
                subscription.Status = SubscriptionStatus.Canceled;
                subscription.EndDate = DateTime.UtcNow;
                subscription.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Dealer {DealerId} unsubscribed from module {ModuleCode}", dealerId, moduleCode);

                return Ok(new { message = "Module unsubscribed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing dealer {DealerId} from module {ModuleCode}", dealerId, moduleCode);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Helper: Extraer módulos incluidos en plan base
        private List<string> GetIncludedModulesFromPlan(DealerPlanFeatures features)
        {
            var modules = new List<string>();

            if (features.CRMAdvanced) modules.Add("crm-advanced");
            if (features.InvoicingCFDI) modules.Add("invoicing-cfdi");
            if (features.FinanceAdvanced) modules.Add("finance-accounting");
            if (features.MarketingAutomation) modules.Add("marketing-automation");
            if (features.WhatsappIntegration) modules.Add("integration-whatsapp");
            if (features.PublicAPI) modules.Add("integration-public-api");
            if (features.AdvancedReports) modules.Add("analytics-reports");

            return modules;
        }
    }

    // DTOs
    public class DealerModulesDetailResponse
    {
        public Guid DealerId { get; set; }
        public string Plan { get; set; } = string.Empty;
        public List<string> ActiveModules { get; set; } = new();
        public List<ModuleDetail> ModuleDetails { get; set; } = new();
    }

    public class ModuleDetail
    {
        public string Code { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // "plan", "addon", "trial"
        public string Status { get; set; } = string.Empty; // "active", "trial", "canceled"
        public DateTime? ExpiresAt { get; set; }
        public decimal? MonthlyPrice { get; set; }
    }
}
