using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gateway.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for Gateway integration tests
/// </summary>
public class GatewayWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test configuration
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Consul:Address"] = "http://localhost:8500",
                ["Consul:ServiceName"] = "gateway-test",
                ["Consul:ServiceHost"] = "localhost",
                ["Consul:ServicePort"] = "5000"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Configure test services here if needed
            // Example: Mock external dependencies
        });
    }
}
