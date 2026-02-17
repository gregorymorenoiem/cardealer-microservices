namespace HealthCheckService.Domain.Enums;

/// <summary>
/// Types of dependencies that can be checked
/// </summary>
public enum DependencyType
{
    Database = 0,
    Cache = 1,
    MessageQueue = 2,
    ExternalApi = 3,
    FileSystem = 4,
    ServiceDiscovery = 5,
    ConfigurationService = 6,
    Other = 99
}
