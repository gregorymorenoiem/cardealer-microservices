using System.Net;
using System.Net.Http.Json;
using ConfigurationService.Application.Commands;
using ConfigurationService.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigurationService.IntegrationTests;

public class FeatureFlagsControllerIntegrationTests : IClassFixture<ConfigurationServiceWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ConfigurationServiceWebApplicationFactory _factory;

    public FeatureFlagsControllerIntegrationTests(ConfigurationServiceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task IsFeatureEnabled_WhenFlagExists_ShouldReturnCorrectStatus()
    {
        // Arrange - Create a feature flag using direct database access
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConfigurationService.Infrastructure.Data.ConfigurationDbContext>();

        var flagKey = "test_feature_" + Guid.NewGuid();
        var flag = new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = "Test Feature",
            Key = flagKey,
            IsEnabled = true,
            Environment = "Dev",
            CreatedBy = "IntegrationTest",
            CreatedAt = DateTime.UtcNow,
            RolloutPercentage = 100
        };

        context.FeatureFlags.Add(flag);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/featureflags/{flagKey}/enabled?environment=Dev");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().NotBeNull();
        result!.Should().ContainKey("isEnabled");
        result["isEnabled"].ToString().Should().Be("True");
    }

    [Fact]
    public async Task IsFeatureEnabled_WhenDisabled_ShouldReturnFalse()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConfigurationService.Infrastructure.Data.ConfigurationDbContext>();

        var flagKey = "disabled_feature_" + Guid.NewGuid();
        var flag = new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = "Disabled Feature",
            Key = flagKey,
            IsEnabled = false,
            Environment = "Dev",
            CreatedBy = "IntegrationTest",
            CreatedAt = DateTime.UtcNow,
            RolloutPercentage = 100
        };

        context.FeatureFlags.Add(flag);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/featureflags/{flagKey}/enabled?environment=Dev");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result!["isEnabled"].ToString().Should().Be("False");
    }

    [Fact]
    public async Task IsFeatureEnabled_WhenNotExists_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentKey = "nonexistent_feature_" + Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/featureflags/{nonExistentKey}/enabled?environment=Dev");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result!["isEnabled"].ToString().Should().Be("False");
    }

    [Fact]
    public async Task IsFeatureEnabled_WithRolloutPercentage_ShouldBeConsistentForSameUser()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConfigurationService.Infrastructure.Data.ConfigurationDbContext>();

        var flagKey = "rollout_feature_" + Guid.NewGuid();
        var flag = new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = "Rollout Feature",
            Key = flagKey,
            IsEnabled = true,
            Environment = "Dev",
            CreatedBy = "IntegrationTest",
            CreatedAt = DateTime.UtcNow,
            RolloutPercentage = 50 // 50% rollout
        };

        context.FeatureFlags.Add(flag);
        await context.SaveChangesAsync();

        var userId = "user_" + Guid.NewGuid();

        // Act - Call multiple times with same user
        var response1 = await _client.GetAsync($"/api/featureflags/{flagKey}/enabled?environment=Dev&userId={userId}");
        var response2 = await _client.GetAsync($"/api/featureflags/{flagKey}/enabled?environment=Dev&userId={userId}");
        var response3 = await _client.GetAsync($"/api/featureflags/{flagKey}/enabled?environment=Dev&userId={userId}");

        // Assert - Should get same result every time
        var result1 = await response1.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var result2 = await response2.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var result3 = await response3.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        result1!["isEnabled"].ToString().Should().Be(result2!["isEnabled"].ToString());
        result2["isEnabled"].ToString().Should().Be(result3!["isEnabled"].ToString());
    }

    [Fact]
    public async Task IsFeatureEnabled_MultiTenant_ShouldIsolateTenants()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConfigurationService.Infrastructure.Data.ConfigurationDbContext>();

        var flagKey = "tenant_feature_" + Guid.NewGuid();

        var tenant1Flag = new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = "Tenant 1 Feature",
            Key = flagKey,
            IsEnabled = true,
            Environment = "Dev",
            TenantId = "tenant1",
            CreatedBy = "IntegrationTest",
            CreatedAt = DateTime.UtcNow,
            RolloutPercentage = 100
        };

        var tenant2Flag = new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = "Tenant 2 Feature",
            Key = flagKey,
            IsEnabled = false,
            Environment = "Dev",
            TenantId = "tenant2",
            CreatedBy = "IntegrationTest",
            CreatedAt = DateTime.UtcNow,
            RolloutPercentage = 100
        };

        context.FeatureFlags.Add(tenant1Flag);
        context.FeatureFlags.Add(tenant2Flag);
        await context.SaveChangesAsync();

        // Act
        var tenant1Response = await _client.GetAsync($"/api/featureflags/{flagKey}/enabled?environment=Dev&tenantId=tenant1");
        var tenant2Response = await _client.GetAsync($"/api/featureflags/{flagKey}/enabled?environment=Dev&tenantId=tenant2");

        // Assert
        var tenant1Result = await tenant1Response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var tenant2Result = await tenant2Response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        tenant1Result!["isEnabled"].ToString().Should().Be("True");
        tenant2Result!["isEnabled"].ToString().Should().Be("False");
    }
}
