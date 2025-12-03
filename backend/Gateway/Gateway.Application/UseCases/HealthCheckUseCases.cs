using Gateway.Domain.Interfaces;

namespace Gateway.Application.UseCases;

/// <summary>
/// Use case for getting health status of all services
/// </summary>
public class GetServicesHealthUseCase
{
    private readonly IHealthCheckService _healthCheckService;

    public GetServicesHealthUseCase(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    public async Task<Dictionary<string, bool>> ExecuteAsync()
    {
        return await _healthCheckService.GetAllServicesHealth();
    }
}

/// <summary>
/// Use case for checking if a specific service is healthy
/// </summary>
public class CheckServiceHealthUseCase
{
    private readonly IHealthCheckService _healthCheckService;

    public CheckServiceHealthUseCase(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    public async Task<bool> ExecuteAsync(string serviceName)
    {
        return await _healthCheckService.IsServiceHealthy(serviceName);
    }
}
