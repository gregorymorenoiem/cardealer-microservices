using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ErrorService.Infrastructure.Persistence;

namespace ErrorService.Tests.Integration
{
    /// <summary>
    /// Custom WebApplicationFactory for E2E integration tests.
    /// Configures the application to use correct PostgreSQL port (25432) and other test settings.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Override connection string to use correct PostgreSQL port
                var connectionString = "Host=localhost;Port=25432;Database=errorservice;Username=postgres;Password=password;Pooling=true;";

                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = connectionString,

                    // JWT Configuration - match appsettings.Development.json
                    ["Jwt:Issuer"] = "cardealer-auth-dev",
                    ["Jwt:Audience"] = "cardealer-services-dev",
                    ["Jwt:SecretKey"] = "development-jwt-secret-key-minimum-32-chars-long-for-testing!",

                    // Disable rate limiting for tests (avoid 429 Too Many Requests)
                    ["RateLimiting:WindowSeconds"] = "3600",
                    ["RateLimiting:MaxRequests"] = "10000",

                    // Use simplified logging for tests
                    ["Logging:LogLevel:Default"] = "Warning",
                    ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
                    ["Logging:LogLevel:ErrorService"] = "Information"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Use InMemory database for integration tests (avoids PostgreSQL connection issues)
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("ErrorServiceTestDb");
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Ensure database is created for integration tests
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

                try
                {
                    // Ensure in-memory database is created
                    db.Database.EnsureCreated();
                    logger.LogInformation("✅ In-memory database created successfully for integration tests");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "❌ An error occurred while creating in-memory database for integration tests");
                    throw;
                }
            });
        }
    }
}
