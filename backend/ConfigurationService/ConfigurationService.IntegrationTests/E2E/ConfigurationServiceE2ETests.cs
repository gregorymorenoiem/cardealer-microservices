using System.Net;
using System.Net.Http.Json;
using ConfigurationService.Application.Commands;
using ConfigurationService.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigurationService.IntegrationTests.E2E;

/// <summary>
/// End-to-End tests that simulate real-world scenarios for Configuration Service
/// </summary>
public class ConfigurationServiceE2ETests : IClassFixture<ConfigurationServiceWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ConfigurationServiceWebApplicationFactory _factory;

    public ConfigurationServiceE2ETests(ConfigurationServiceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Scenario_ApplicationStartup_LoadConfigurations()
    {
        // Scenario: A microservice starts up and loads its configurations

        // Arrange - Setup configurations for a microservice
        var serviceName = "PaymentService";
        var environment = "Production";

        var configs = new[]
        {
            new CreateConfigurationCommand(
                Key: $"{serviceName}:ApiTimeout",
                Value: "30000",
                Environment: environment,
                CreatedBy: "DevOps",
                Description: "API timeout in milliseconds"
            ),
            new CreateConfigurationCommand(
                Key: $"{serviceName}:MaxRetries",
                Value: "3",
                Environment: environment,
                CreatedBy: "DevOps",
                Description: "Maximum retry attempts"
            ),
            new CreateConfigurationCommand(
                Key: $"{serviceName}:DatabaseConnectionString",
                Value: "Host=db.prod.local;Database=payments",
                Environment: environment,
                CreatedBy: "DevOps"
            )
        };

        // Act - Create configurations
        foreach (var config in configs)
        {
            var response = await _client.PostAsJsonAsync("/api/configurations", config);
            response.EnsureSuccessStatusCode();
        }

        // Act - Load all configurations for the service
        var loadResponse = await _client.GetAsync($"/api/configurations?environment={environment}");

        // Assert
        loadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loadedConfigs = await loadResponse.Content.ReadFromJsonAsync<List<ConfigurationItem>>();

        loadedConfigs.Should().NotBeNull();
        loadedConfigs!.Count(c => c.Key.StartsWith(serviceName)).Should().Be(3);
        loadedConfigs.First(c => c.Key.EndsWith("ApiTimeout")).Value.Should().Be("30000");
    }

    [Fact]
    public async Task Scenario_HotConfigurationUpdate_WithHistoryTracking()
    {
        // Scenario: Update a configuration in production without restarting the service

        // Arrange - Create initial configuration
        var createCommand = new CreateConfigurationCommand(
            Key: "FeatureService:RateLimit",
            Value: "100",
            Environment: "Production",
            CreatedBy: "Admin",
            Description: "Requests per minute"
        );

        var createResponse = await _client.PostAsJsonAsync("/api/configurations", createCommand);
        var createdConfig = await createResponse.Content.ReadFromJsonAsync<ConfigurationItem>();

        // Act - Update configuration due to high load
        var updateCommand1 = new UpdateConfigurationCommand(
            Id: createdConfig!.Id,
            Value: "200",
            UpdatedBy: "Admin",
            ChangeReason: "Increased capacity due to high demand"
        );

        var update1Response = await _client.PutAsJsonAsync($"/api/configurations/{createdConfig.Id}", updateCommand1);

        // Act - Update again after optimization
        var updatedConfig1 = await update1Response.Content.ReadFromJsonAsync<ConfigurationItem>();
        var updateCommand2 = new UpdateConfigurationCommand(
            Id: updatedConfig1!.Id,
            Value: "150",
            UpdatedBy: "Admin",
            ChangeReason: "Optimized backend, reducing limit"
        );

        var update2Response = await _client.PutAsJsonAsync($"/api/configurations/{updatedConfig1.Id}", updateCommand2);

        // Assert
        update1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        update2Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var finalConfig = await update2Response.Content.ReadFromJsonAsync<ConfigurationItem>();
        finalConfig!.Value.Should().Be("150");
        finalConfig.Version.Should().Be(3); // Initial + 2 updates
        finalConfig.UpdatedBy.Should().Be("Admin");
    }

    [Fact]
    public async Task Scenario_FeatureRollout_GradualDeployment()
    {
        // Scenario: Gradual rollout of a new feature to 25%, 50%, 75%, then 100% of users

        // Arrange - Create feature flag with 0% rollout
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConfigurationService.Infrastructure.Data.ConfigurationDbContext>();

        var featureKey = "new_checkout_flow_" + Guid.NewGuid();
        var flag = new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = "New Checkout Flow",
            Key = featureKey,
            IsEnabled = true,
            Environment = "Production",
            CreatedBy = "ProductTeam",
            CreatedAt = DateTime.UtcNow,
            RolloutPercentage = 0
        };

        context.FeatureFlags.Add(flag);
        await context.SaveChangesAsync();

        // Simulate 100 users
        var users = Enumerable.Range(1, 100).Select(i => $"user_{i}").ToList();

        // Act & Assert - Phase 1: 0% rollout
        var phase1EnabledCount = 0;
        foreach (var user in users.Take(10))
        {
            var response = await _client.GetAsync($"/api/featureflags/{featureKey}/enabled?environment=Production&userId={user}");
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (result!["isEnabled"].ToString() == "True")
                phase1EnabledCount++;
        }
        phase1EnabledCount.Should().Be(0);

        // Phase 2: 25% rollout
        flag.RolloutPercentage = 25;
        context.Update(flag);
        await context.SaveChangesAsync();

        var phase2EnabledCount = 0;
        foreach (var user in users)
        {
            var response = await _client.GetAsync($"/api/featureflags/{featureKey}/enabled?environment=Production&userId={user}");
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (result!["isEnabled"].ToString() == "True")
                phase2EnabledCount++;
        }
        phase2EnabledCount.Should().BeInRange(15, 35); // ~25% with some variance

        // Phase 3: 100% rollout
        flag.RolloutPercentage = 100;
        context.Update(flag);
        await context.SaveChangesAsync();

        var phase3EnabledCount = 0;
        foreach (var user in users)
        {
            var response = await _client.GetAsync($"/api/featureflags/{featureKey}/enabled?environment=Production&userId={user}");
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (result!["isEnabled"].ToString() == "True")
                phase3EnabledCount++;
        }
        phase3EnabledCount.Should().Be(100);
    }

    [Fact]
    public async Task Scenario_MultiTenantSaaS_IsolatedConfigurations()
    {
        // Scenario: SaaS platform with multiple tenants, each with their own configurations

        // Arrange - Setup configurations for two different tenants
        var environment = "Production";
        var tenant1 = "acme_corp";
        var tenant2 = "widgets_inc";

        var tenant1Configs = new[]
        {
            new CreateConfigurationCommand(
                Key: "App:MaxUsers",
                Value: "100",
                Environment: environment,
                CreatedBy: "System",
                TenantId: tenant1,
                Description: "Acme Corp - Small plan"
            ),
            new CreateConfigurationCommand(
                Key: "App:Features",
                Value: "basic,reports",
                Environment: environment,
                CreatedBy: "System",
                TenantId: tenant1
            )
        };

        var tenant2Configs = new[]
        {
            new CreateConfigurationCommand(
                Key: "App:MaxUsers",
                Value: "1000",
                Environment: environment,
                CreatedBy: "System",
                TenantId: tenant2,
                Description: "Widgets Inc - Enterprise plan"
            ),
            new CreateConfigurationCommand(
                Key: "App:Features",
                Value: "basic,reports,analytics,api",
                Environment: environment,
                CreatedBy: "System",
                TenantId: tenant2
            )
        };

        // Act - Create configurations for both tenants
        foreach (var config in tenant1Configs.Concat(tenant2Configs))
        {
            await _client.PostAsJsonAsync("/api/configurations", config);
        }

        // Act - Retrieve configurations for each tenant
        var tenant1Response = await _client.GetAsync($"/api/configurations/App:MaxUsers?environment={environment}&tenantId={tenant1}");
        var tenant2Response = await _client.GetAsync($"/api/configurations/App:MaxUsers?environment={environment}&tenantId={tenant2}");

        // Assert - Each tenant gets their own configuration
        tenant1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        tenant2Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tenant1Config = await tenant1Response.Content.ReadFromJsonAsync<ConfigurationItem>();
        var tenant2Config = await tenant2Response.Content.ReadFromJsonAsync<ConfigurationItem>();

        tenant1Config!.Value.Should().Be("100");
        tenant2Config!.Value.Should().Be("1000");
        tenant1Config.TenantId.Should().Be(tenant1);
        tenant2Config.TenantId.Should().Be(tenant2);
    }

    [Fact]
    public async Task Scenario_EnvironmentPromotion_DevToStagingToProduction()
    {
        // Scenario: Promote configuration changes through environments

        // Arrange - Create configuration in Dev
        var configKey = "Service:NewFeatureEnabled";

        var devConfig = new CreateConfigurationCommand(
            Key: configKey,
            Value: "true",
            Environment: "Dev",
            CreatedBy: "Developer",
            Description: "Enable new feature"
        );

        // Act - Create in Dev
        var devResponse = await _client.PostAsJsonAsync("/api/configurations", devConfig);
        devResponse.EnsureSuccessStatusCode();

        // Simulate testing phase - retrieve from Dev
        var devGetResponse = await _client.GetAsync($"/api/configurations/{configKey}?environment=Dev");
        devGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Promote to Staging
        var stagingConfig = new CreateConfigurationCommand(
            Key: configKey,
            Value: "true",
            Environment: "Staging",
            CreatedBy: "DevOps",
            Description: "Enable new feature - Promoted from Dev"
        );

        var stagingResponse = await _client.PostAsJsonAsync("/api/configurations", stagingConfig);
        stagingResponse.EnsureSuccessStatusCode();

        // Act - After successful staging tests, promote to Production
        var prodConfig = new CreateConfigurationCommand(
            Key: configKey,
            Value: "true",
            Environment: "Production",
            CreatedBy: "ReleaseManager",
            Description: "Enable new feature - Promoted from Staging"
        );

        var prodResponse = await _client.PostAsJsonAsync("/api/configurations", prodConfig);

        // Assert - Configuration exists in all environments
        var devFinal = await _client.GetAsync($"/api/configurations/{configKey}?environment=Dev");
        var stagingFinal = await _client.GetAsync($"/api/configurations/{configKey}?environment=Staging");
        var prodFinal = await _client.GetAsync($"/api/configurations/{configKey}?environment=Production");

        devFinal.StatusCode.Should().Be(HttpStatusCode.OK);
        stagingFinal.StatusCode.Should().Be(HttpStatusCode.OK);
        prodFinal.StatusCode.Should().Be(HttpStatusCode.OK);

        // All should have the same value but different metadata
        var devConfigResult = await devFinal.Content.ReadFromJsonAsync<ConfigurationItem>();
        var stagingConfigResult = await stagingFinal.Content.ReadFromJsonAsync<ConfigurationItem>();
        var prodConfigResult = await prodFinal.Content.ReadFromJsonAsync<ConfigurationItem>();

        devConfigResult!.Value.Should().Be("true");
        stagingConfigResult!.Value.Should().Be("true");
        prodConfigResult!.Value.Should().Be("true");

        devConfigResult.CreatedBy.Should().Be("Developer");
        stagingConfigResult.CreatedBy.Should().Be("DevOps");
        prodConfigResult.CreatedBy.Should().Be("ReleaseManager");
    }

    [Fact]
    public async Task Scenario_EmergencyKillSwitch_DisableFeature()
    {
        // Scenario: Emergency - disable a problematic feature immediately in production

        // Arrange - Feature flag is enabled in production
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConfigurationService.Infrastructure.Data.ConfigurationDbContext>();

        var featureKey = "payment_processing_v2_" + Guid.NewGuid();
        var flag = new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = "Payment Processing V2",
            Key = featureKey,
            IsEnabled = true,
            Environment = "Production",
            CreatedBy = "ProductTeam",
            CreatedAt = DateTime.UtcNow,
            RolloutPercentage = 100
        };

        context.FeatureFlags.Add(flag);
        await context.SaveChangesAsync();

        // Verify feature is enabled
        var initialResponse = await _client.GetAsync($"/api/featureflags/{featureKey}/enabled?environment=Production");
        var initialResult = await initialResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        initialResult!["isEnabled"].ToString().Should().Be("True");

        // Act - Emergency: Disable the feature (Kill switch)
        flag.IsEnabled = false;
        context.Update(flag);
        await context.SaveChangesAsync();

        // Assert - Feature is immediately disabled
        var disabledResponse = await _client.GetAsync($"/api/featureflags/{featureKey}/enabled?environment=Production");
        var disabledResult = await disabledResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        disabledResult!["isEnabled"].ToString().Should().Be("False");

        // Simulate multiple concurrent requests - all should get disabled status
        var tasks = Enumerable.Range(1, 10).Select(async i =>
        {
            var response = await _client.GetAsync($"/api/featureflags/{featureKey}/enabled?environment=Production&userId=user_{i}");
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return result!["isEnabled"].ToString();
        });

        var results = await Task.WhenAll(tasks);
        results.Should().AllBe("False");
    }

    [Fact]
    public async Task Scenario_ABTesting_SplitTrafficBetweenVariants()
    {
        // Scenario: A/B testing with 50/50 split between control and variant

        // Arrange - Create two feature flags for A/B test
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConfigurationService.Infrastructure.Data.ConfigurationDbContext>();

        var testId = Guid.NewGuid();
        var variantAKey = $"checkout_variant_a_{testId}";
        var variantBKey = $"checkout_variant_b_{testId}";

        var variantA = new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = "Checkout Variant A (Control)",
            Key = variantAKey,
            IsEnabled = true,
            Environment = "Production",
            CreatedBy = "ExperimentTeam",
            CreatedAt = DateTime.UtcNow,
            RolloutPercentage = 50
        };

        var variantB = new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = "Checkout Variant B (New Design)",
            Key = variantBKey,
            IsEnabled = true,
            Environment = "Production",
            CreatedBy = "ExperimentTeam",
            CreatedAt = DateTime.UtcNow,
            RolloutPercentage = 50
        };

        context.FeatureFlags.AddRange(variantA, variantB);
        await context.SaveChangesAsync();

        // Act - Simulate 100 users and check their assignment
        var users = Enumerable.Range(1, 100).Select(i => $"testuser_{i}").ToList();
        var variantACount = 0;
        var variantBCount = 0;

        foreach (var user in users)
        {
            var responseA = await _client.GetAsync($"/api/featureflags/{variantAKey}/enabled?environment=Production&userId={user}");
            var resultA = await responseA.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            var responseB = await _client.GetAsync($"/api/featureflags/{variantBKey}/enabled?environment=Production&userId={user}");
            var resultB = await responseB.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            if (resultA!["isEnabled"].ToString() == "True")
                variantACount++;
            if (resultB!["isEnabled"].ToString() == "True")
                variantBCount++;
        }

        // Assert - Approximately 50/50 split (with some variance)
        variantACount.Should().BeInRange(35, 65);
        variantBCount.Should().BeInRange(35, 65);
    }
}
