using Microsoft.Extensions.Logging;

namespace SchedulerService.Infrastructure.Jobs;

/// <summary>
/// Example job: Health check for all services
/// </summary>
public class HealthCheckJob : IScheduledJob
{
    private readonly ILogger<HealthCheckJob> _logger;

    public HealthCheckJob(ILogger<HealthCheckJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("Starting health check for all services...");

        try
        {
            var services = parameters.ContainsKey("Services")
                ? parameters["Services"].Split(',')
                : new[] { "authservice", "userservice", "vehicleservice" };

            foreach (var service in services)
            {
                _logger.LogInformation("Checking health of service: {Service}", service.Trim());
                
                // TODO: Implement actual health check logic
                // This would make HTTP calls to /health endpoints
                await Task.Delay(500); // Simulate health check

                _logger.LogInformation("Service {Service} is healthy", service.Trim());
            }

            _logger.LogInformation("Health check completed for all services");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check job execution");
            throw;
        }
    }
}
