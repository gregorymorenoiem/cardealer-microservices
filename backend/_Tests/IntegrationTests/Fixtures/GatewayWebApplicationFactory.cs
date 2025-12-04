using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IntegrationTests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory for Gateway integration testing
/// </summary>
public class GatewayWebApplicationFactory : WebApplicationFactory<global::Program>
{
    public string? PostgresConnectionString { get; set; }
    public string? RedisConnectionString { get; set; }
    public string? RabbitMqHost { get; set; }
    public int RabbitMqPort { get; set; } = 5672;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Clear default configuration
            config.Sources.Clear();

            // Add test configuration
            config.AddJsonFile("appsettings.Test.json", optional: true);

            // Override with test values
            var testSettings = new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
                ["ServiceDiscovery:Enabled"] = "false",
                ["Consul:Enabled"] = "false"
            };

            if (!string.IsNullOrEmpty(PostgresConnectionString))
            {
                testSettings["ConnectionStrings:DefaultConnection"] = PostgresConnectionString;
            }

            if (!string.IsNullOrEmpty(RedisConnectionString))
            {
                testSettings["Redis:ConnectionString"] = RedisConnectionString;
            }

            if (!string.IsNullOrEmpty(RabbitMqHost))
            {
                testSettings["RabbitMQ:HostName"] = RabbitMqHost;
                testSettings["RabbitMQ:Port"] = RabbitMqPort.ToString();
            }

            config.AddInMemoryCollection(testSettings);
        });

        builder.ConfigureServices(services =>
        {
            // Remove Consul registration if it exists
            var consulDescriptor = services.FirstOrDefault(d =>
                d.ServiceType.Name.Contains("Consul"));
            if (consulDescriptor != null)
            {
                services.Remove(consulDescriptor);
            }
        });
    }
}
