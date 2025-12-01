using System.Net;
using System.Net.Http.Json;
using ConfigurationService.Application.Commands;
using ConfigurationService.Domain.Entities;
using FluentAssertions;

namespace ConfigurationService.IntegrationTests;

public class ConfigurationsControllerIntegrationTests : IClassFixture<ConfigurationServiceWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ConfigurationServiceWebApplicationFactory _factory;

    public ConfigurationsControllerIntegrationTests(ConfigurationServiceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateConfiguration_ShouldReturn201Created()
    {
        // Arrange
        var command = new CreateConfigurationCommand(
            Key: "TestKey_" + Guid.NewGuid(),
            Value: "TestValue",
            Environment: "Dev",
            CreatedBy: "IntegrationTest",
            Description: "Test configuration"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/configurations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var config = await response.Content.ReadFromJsonAsync<ConfigurationItem>();
        config.Should().NotBeNull();
        config!.Key.Should().Be(command.Key);
        config.Value.Should().Be(command.Value);
        config.Environment.Should().Be(command.Environment);
    }

    [Fact]
    public async Task GetConfiguration_WhenExists_ShouldReturn200OK()
    {
        // Arrange - Create a configuration first
        var createCommand = new CreateConfigurationCommand(
            Key: "GetTestKey_" + Guid.NewGuid(),
            Value: "GetTestValue",
            Environment: "Dev",
            CreatedBy: "IntegrationTest"
        );

        var createResponse = await _client.PostAsJsonAsync("/api/configurations", createCommand);
        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync($"/api/configurations/{createCommand.Key}?environment=Dev");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var config = await response.Content.ReadFromJsonAsync<ConfigurationItem>();
        config.Should().NotBeNull();
        config!.Key.Should().Be(createCommand.Key);
        config.Value.Should().Be(createCommand.Value);
    }

    [Fact]
    public async Task GetConfiguration_WhenNotExists_ShouldReturn404NotFound()
    {
        // Arrange
        var nonExistentKey = "NonExistentKey_" + Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/configurations/{nonExistentKey}?environment=Dev");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllConfigurations_ShouldReturnList()
    {
        // Arrange - Create multiple configurations
        var testEnv = "TestEnv_" + Guid.NewGuid().ToString().Substring(0, 8);

        var command1 = new CreateConfigurationCommand(
            Key: "Key1_" + Guid.NewGuid(),
            Value: "Value1",
            Environment: testEnv,
            CreatedBy: "IntegrationTest"
        );

        var command2 = new CreateConfigurationCommand(
            Key: "Key2_" + Guid.NewGuid(),
            Value: "Value2",
            Environment: testEnv,
            CreatedBy: "IntegrationTest"
        );

        await _client.PostAsJsonAsync("/api/configurations", command1);
        await _client.PostAsJsonAsync("/api/configurations", command2);

        // Act
        var response = await _client.GetAsync($"/api/configurations?environment={testEnv}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var configs = await response.Content.ReadFromJsonAsync<List<ConfigurationItem>>();
        configs.Should().NotBeNull();
        configs!.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateConfiguration_ShouldReturn200OK()
    {
        // Arrange - Create a configuration first
        var createCommand = new CreateConfigurationCommand(
            Key: "UpdateTestKey_" + Guid.NewGuid(),
            Value: "OldValue",
            Environment: "Dev",
            CreatedBy: "IntegrationTest"
        );

        var createResponse = await _client.PostAsJsonAsync("/api/configurations", createCommand);
        var createdConfig = await createResponse.Content.ReadFromJsonAsync<ConfigurationItem>();

        var updateCommand = new UpdateConfigurationCommand(
            Id: createdConfig!.Id,
            Value: "NewValue",
            UpdatedBy: "IntegrationTest",
            ChangeReason: "Testing update"
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/configurations/{createdConfig.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedConfig = await response.Content.ReadFromJsonAsync<ConfigurationItem>();
        updatedConfig.Should().NotBeNull();
        updatedConfig!.Value.Should().Be("NewValue");
        updatedConfig.Version.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task DeleteConfiguration_ShouldReturn204NoContent()
    {
        // Arrange - Create a configuration first
        var createCommand = new CreateConfigurationCommand(
            Key: "DeleteTestKey_" + Guid.NewGuid(),
            Value: "ValueToDelete",
            Environment: "Dev",
            CreatedBy: "IntegrationTest"
        );

        var createResponse = await _client.PostAsJsonAsync("/api/configurations", createCommand);
        var createdConfig = await createResponse.Content.ReadFromJsonAsync<ConfigurationItem>();

        // Act
        var response = await _client.DeleteAsync($"/api/configurations/{createdConfig!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted (soft delete - should return 404)
        var getResponse = await _client.GetAsync($"/api/configurations/{createCommand.Key}?environment=Dev");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MultiTenantConfiguration_ShouldIsolateTenants()
    {
        // Arrange
        var key = "SharedKey_" + Guid.NewGuid();

        var tenant1Command = new CreateConfigurationCommand(
            Key: key,
            Value: "Tenant1Value",
            Environment: "Dev",
            CreatedBy: "IntegrationTest",
            TenantId: "tenant1"
        );

        var tenant2Command = new CreateConfigurationCommand(
            Key: key,
            Value: "Tenant2Value",
            Environment: "Dev",
            CreatedBy: "IntegrationTest",
            TenantId: "tenant2"
        );

        await _client.PostAsJsonAsync("/api/configurations", tenant1Command);
        await _client.PostAsJsonAsync("/api/configurations", tenant2Command);

        // Act
        var tenant1Response = await _client.GetAsync($"/api/configurations/{key}?environment=Dev&tenantId=tenant1");
        var tenant2Response = await _client.GetAsync($"/api/configurations/{key}?environment=Dev&tenantId=tenant2");

        // Assert
        tenant1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        tenant2Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tenant1Config = await tenant1Response.Content.ReadFromJsonAsync<ConfigurationItem>();
        var tenant2Config = await tenant2Response.Content.ReadFromJsonAsync<ConfigurationItem>();

        tenant1Config!.Value.Should().Be("Tenant1Value");
        tenant2Config!.Value.Should().Be("Tenant2Value");
    }
}
