using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Consul;
using Moq;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Domain.Entities;
using SdEnums = ServiceDiscovery.Domain.Enums;

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
            // Clear existing configuration
            config.Sources.Clear();

            // Add test configuration with ocelot.json structure
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Consul:Address"] = "http://localhost:8500",
                ["Consul:ServiceName"] = "gateway-test",
                ["Consul:ServiceHost"] = "localhost",
                ["Consul:ServicePort"] = "5000",
                // Minimal Ocelot configuration
                ["Routes:0:DownstreamPathTemplate"] = "/api/test",
                ["Routes:0:DownstreamScheme"] = "http",
                ["Routes:0:DownstreamHostAndPorts:0:Host"] = "localhost",
                ["Routes:0:DownstreamHostAndPorts:0:Port"] = "5001",
                ["Routes:0:UpstreamPathTemplate"] = "/test",
                ["Routes:0:UpstreamHttpMethod:0"] = "GET",
                ["GlobalConfiguration:BaseUrl"] = "http://localhost:5000",
                // SwaggerEndPoints configuration (required for Swagger for Ocelot)
                ["SwaggerEndPoints:0:Key"] = "test-service",
                ["SwaggerEndPoints:0:Config:0:Name"] = "Test API",
                ["SwaggerEndPoints:0:Config:0:Version"] = "v1",
                ["SwaggerEndPoints:0:Config:0:Url"] = "http://localhost:5001/swagger/v1/swagger.json"
            });
        }); builder.ConfigureServices(services =>
        {
            // Remove real Consul client registration
            var consulDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IConsulClient));
            if (consulDescriptor != null)
            {
                services.Remove(consulDescriptor);
            }

            // Add mock Consul client
            var mockConsulClient = new Mock<IConsulClient>();
            services.AddSingleton<IConsulClient>(mockConsulClient.Object);

            // Mock Service Registry
            var mockServiceRegistry = new Mock<IServiceRegistry>();
            var testInstance = new ServiceInstance
            {
                Id = "gateway-test-1",
                ServiceName = "gateway-test",
                Host = "localhost",
                Port = 5000,
                Status = SdEnums.ServiceStatus.Active,
                HealthStatus = SdEnums.HealthStatus.Healthy
            };

            mockServiceRegistry
                .Setup(x => x.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testInstance);

            var registryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IServiceRegistry));
            if (registryDescriptor != null)
            {
                services.Remove(registryDescriptor);
            }
            services.AddScoped<IServiceRegistry>(sp => mockServiceRegistry.Object);

            // Mock Service Discovery
            var mockServiceDiscovery = new Mock<IServiceDiscovery>();
            var healthyInstances = new List<ServiceInstance> { testInstance };

            mockServiceDiscovery
                .Setup(x => x.GetHealthyInstancesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthyInstances);

            mockServiceDiscovery
                .Setup(x => x.FindServiceInstanceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testInstance);

            var discoveryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IServiceDiscovery));
            if (discoveryDescriptor != null)
            {
                services.Remove(discoveryDescriptor);
            }
            services.AddScoped<IServiceDiscovery>(sp => mockServiceDiscovery.Object);

            // Mock Health Checker
            var mockHealthChecker = new Mock<IHealthChecker>();
            var healthCheckResult = new HealthCheckResult
            {
                InstanceId = testInstance.Id,
                Status = SdEnums.HealthStatus.Healthy,
                ResponseTimeMs = 50,
                StatusCode = 200,
                CheckedAt = DateTime.UtcNow,
                Message = "Service is healthy"
            };

            mockHealthChecker
                .Setup(x => x.CheckHealthAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthCheckResult);

            var healthDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHealthChecker));
            if (healthDescriptor != null)
            {
                services.Remove(healthDescriptor);
            }
            services.AddScoped<IHealthChecker>(sp => mockHealthChecker.Object);
        });
    }
}