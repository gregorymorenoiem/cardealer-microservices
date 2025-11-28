namespace AuthService.Infrastructure.HealthChecks;

public class HealthCheckSettings
{
    public bool Enabled { get; set; } = true;
    public int DatabaseTimeout { get; set; } = 30;
    public int RedisTimeout { get; set; } = 10;
    public int ExternalServicesTimeout { get; set; } = 30;
}