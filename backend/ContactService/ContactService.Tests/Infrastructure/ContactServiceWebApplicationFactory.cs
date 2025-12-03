using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ContactService.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration testing
/// </summary>
public class ContactServiceWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test configuration
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Consul:Address"] = "http://localhost:8500",
                ["Consul:ServiceName"] = "ContactService",
                ["Consul:ServiceHost"] = "localhost",
                ["Consul:ServicePort"] = "5007"
            });
        });

        builder.UseEnvironment("Testing");
    }
}
