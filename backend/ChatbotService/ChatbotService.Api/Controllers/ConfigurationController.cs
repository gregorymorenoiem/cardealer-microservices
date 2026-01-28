using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatbotService.Application.DTOs;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Dealer")]
public class ConfigurationController : ControllerBase
{
    private readonly ILogger<ConfigurationController> _logger;
    private readonly IChatbotConfigurationRepository _configRepository;
    private readonly IQuickResponseRepository _quickResponseRepository;
    private readonly IChatbotVehicleRepository _vehicleRepository;

    public ConfigurationController(
        ILogger<ConfigurationController> logger,
        IChatbotConfigurationRepository configRepository,
        IQuickResponseRepository quickResponseRepository,
        IChatbotVehicleRepository vehicleRepository)
    {
        _logger = logger;
        _configRepository = configRepository;
        _quickResponseRepository = quickResponseRepository;
        _vehicleRepository = vehicleRepository;
    }

    /// <summary>
    /// Get chatbot configuration
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ChatbotConfigurationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetConfiguration(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var config = await _configRepository.GetByIdAsync(id, cancellationToken);
            if (config == null)
            {
                return NotFound(new { error = "Configuration not found" });
            }

            var dto = new ChatbotConfigurationDto
            {
                Id = config.Id,
                DealerId = config.DealerId,
                Name = config.Name,
                IsActive = config.IsActive,
                Plan = config.Plan,
                FreeInteractionsPerMonth = config.FreeInteractionsPerMonth,
                CostPerInteraction = config.CostPerInteraction,
                MaxInteractionsPerSession = config.MaxInteractionsPerSession,
                MaxInteractionsPerUserPerDay = config.MaxInteractionsPerUserPerDay,
                MaxInteractionsPerUserPerMonth = config.MaxInteractionsPerUserPerMonth,
                BotName = config.BotName,
                BotAvatarUrl = config.BotAvatarUrl,
                WelcomeMessage = config.WelcomeMessage,
                EnableWebChat = config.EnableWebChat,
                EnableWhatsApp = config.EnableWhatsApp,
                EnableFacebook = config.EnableFacebook,
                EnableAutoInventorySync = config.EnableAutoInventorySync,
                EnableAutoReports = config.EnableAutoReports,
                EnableAutoLearning = config.EnableAutoLearning,
                EnableHealthMonitoring = config.EnableHealthMonitoring
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get configuration by dealer
    /// </summary>
    [HttpGet("dealer/{dealerId:guid}")]
    [ProducesResponseType(typeof(ChatbotConfigurationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetConfigurationByDealer(Guid dealerId, CancellationToken cancellationToken)
    {
        try
        {
            var config = await _configRepository.GetByDealerIdAsync(dealerId, cancellationToken);
            if (config == null)
            {
                return NotFound(new { error = "Configuration not found for dealer" });
            }

            var dto = new ChatbotConfigurationDto
            {
                Id = config.Id,
                DealerId = config.DealerId,
                Name = config.Name,
                IsActive = config.IsActive,
                Plan = config.Plan,
                FreeInteractionsPerMonth = config.FreeInteractionsPerMonth,
                CostPerInteraction = config.CostPerInteraction,
                MaxInteractionsPerSession = config.MaxInteractionsPerSession,
                MaxInteractionsPerUserPerDay = config.MaxInteractionsPerUserPerDay,
                MaxInteractionsPerUserPerMonth = config.MaxInteractionsPerUserPerMonth,
                BotName = config.BotName,
                BotAvatarUrl = config.BotAvatarUrl,
                WelcomeMessage = config.WelcomeMessage,
                EnableWebChat = config.EnableWebChat,
                EnableWhatsApp = config.EnableWhatsApp,
                EnableFacebook = config.EnableFacebook,
                EnableAutoInventorySync = config.EnableAutoInventorySync,
                EnableAutoReports = config.EnableAutoReports,
                EnableAutoLearning = config.EnableAutoLearning,
                EnableHealthMonitoring = config.EnableHealthMonitoring
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration for dealer {DealerId}", dealerId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get quick responses for a configuration
    /// </summary>
    [HttpGet("{configId:guid}/quick-responses")]
    [ProducesResponseType(typeof(IEnumerable<QuickResponseDto>), 200)]
    public async Task<IActionResult> GetQuickResponses(Guid configId, CancellationToken cancellationToken)
    {
        try
        {
            var responses = await _quickResponseRepository.GetByConfigurationIdAsync(configId, cancellationToken);

            var dtos = responses.Select(r => new QuickResponseDto
            {
                Id = r.Id,
                Category = r.Category,
                Name = r.Name,
                Triggers = r.GetTriggers(),
                Response = r.Response,
                Priority = r.Priority,
                IsActive = r.IsActive,
                BypassDialogflow = r.BypassDialogflow,
                UsageCount = r.UsageCount,
                LastUsedAt = r.LastUsedAt
            });

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quick responses");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get vehicles for chatbot
    /// </summary>
    [HttpGet("{configId:guid}/vehicles")]
    [ProducesResponseType(typeof(IEnumerable<VehicleCardDto>), 200)]
    public async Task<IActionResult> GetVehicles(Guid configId, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var vehicles = await _vehicleRepository.GetByConfigurationIdAsync(configId, cancellationToken);

            var dtos = vehicles.Take(limit).Select(v => new VehicleCardDto
            {
                VehicleId = v.VehicleId,
                Title = $"{v.Year} {v.Make} {v.Model}",
                Subtitle = v.Description ?? "",
                ImageUrl = v.ImageUrl ?? "",
                Price = v.Price
            });

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicles");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Search vehicles
    /// </summary>
    [HttpGet("{configId:guid}/vehicles/search")]
    [ProducesResponseType(typeof(IEnumerable<VehicleCardDto>), 200)]
    public async Task<IActionResult> SearchVehicles(Guid configId, [FromQuery] string q, [FromQuery] int limit = 5, CancellationToken cancellationToken = default)
    {
        try
        {
            var vehicles = await _vehicleRepository.SearchAsync(configId, q, limit, cancellationToken);

            var dtos = vehicles.Select(v => new VehicleCardDto
            {
                VehicleId = v.VehicleId,
                Title = $"{v.Year} {v.Make} {v.Model}",
                Subtitle = v.Description ?? "",
                ImageUrl = v.ImageUrl ?? "",
                Price = v.Price
            });

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching vehicles");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get featured vehicles
    /// </summary>
    [HttpGet("{configId:guid}/vehicles/featured")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<VehicleCardDto>), 200)]
    public async Task<IActionResult> GetFeaturedVehicles(Guid configId, [FromQuery] int limit = 5, CancellationToken cancellationToken = default)
    {
        try
        {
            var vehicles = await _vehicleRepository.GetFeaturedAsync(configId, limit, cancellationToken);

            var dtos = vehicles.Select(v => new VehicleCardDto
            {
                VehicleId = v.VehicleId,
                Title = $"{v.Year} {v.Make} {v.Model}",
                Subtitle = v.Description ?? "",
                ImageUrl = v.ImageUrl ?? "",
                Price = v.Price
            });

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured vehicles");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
