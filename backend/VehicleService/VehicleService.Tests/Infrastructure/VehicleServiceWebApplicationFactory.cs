using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace VehicleService.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for VehicleService integration testing
/// </summary>
public class VehicleServiceWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add in-memory configuration for testing
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Consul:Address"] = "http://localhost:8500",
                ["Consul:ServiceName"] = "VehicleService",
                ["Consul:ServiceHost"] = "localhost",
                ["Consul:ServicePort"] = "5009"
            });
        });

        builder.UseEnvironment("Testing");
    }
}
