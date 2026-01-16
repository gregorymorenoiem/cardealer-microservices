using Microsoft.AspNetCore.Mvc;
using VehiclesSaleService.Infrastructure.Seeding;

namespace VehiclesSaleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedingController : ControllerBase
{
    private readonly DatabaseSeedingService _seedingService;
    private readonly ILogger<SeedingController> _logger;

    public SeedingController(
        DatabaseSeedingService seedingService,
        ILogger<SeedingController> logger)
    {
        _seedingService = seedingService;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta el seeding completo de la base de datos
    /// </summary>
    /// <returns>Resultado del seeding</returns>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteSeeding()
    {
        try
        {
            _logger.LogInformation("🚀 Iniciando seeding desde API endpoint");
            await _seedingService.SeedAllAsync();
            
            return Ok(new 
            { 
                success = true, 
                message = "Seeding completado exitosamente",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando seeding");
            return StatusCode(500, new 
            { 
                success = false, 
                message = "Error durante el seeding", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Valida el estado del seeding
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetSeedingStatus()
    {
        // TODO: Implementar validación completa
        return Ok(new
        {
            message = "Endpoint de validación - TODO",
            timestamp = DateTime.UtcNow
        });
    }
}
