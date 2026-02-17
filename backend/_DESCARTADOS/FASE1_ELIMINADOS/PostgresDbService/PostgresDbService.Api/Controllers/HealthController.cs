using Microsoft.AspNetCore.Mvc;

namespace PostgresDbService.Api.Controllers;

/// <summary>
/// Health check controller
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "PostgresDbService",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }

    /// <summary>
    /// Database health check endpoint
    /// </summary>
    [HttpGet("database")]
    public async Task<IActionResult> DatabaseHealth(
        [FromServices] PostgresDbService.Infrastructure.Persistence.CentralizedDbContext context)
    {
        try
        {
            // Simple query to check database connectivity
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return ServiceUnavailable(new
                {
                    Status = "Unhealthy",
                    Service = "PostgresDbService",
                    Component = "Database",
                    Error = "Cannot connect to database",
                    Timestamp = DateTime.UtcNow
                });
            }

            // Check if tables exist
            var tableCount = await context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'").FirstAsync();
            
            return Ok(new
            {
                Status = "Healthy",
                Service = "PostgresDbService",
                Component = "Database",
                TablesCount = tableCount,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return ServiceUnavailable(new
            {
                Status = "Unhealthy",
                Service = "PostgresDbService",
                Component = "Database",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    private ObjectResult ServiceUnavailable(object value)
    {
        return new ObjectResult(value) { StatusCode = 503 };
    }
}