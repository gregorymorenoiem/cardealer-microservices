namespace Gateway.Tests.Unit.Configuration;

/// <summary>
/// Unit tests for Ocelot configuration validation
/// </summary>
public class OcelotConfigurationTests
{
    [Fact]
    public void OcelotDevConfiguration_IsValid()
    {
        // Arrange
        var configPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..",
            "Gateway.Api",
            "ocelot.dev.json"
        );

        // Act
        var configExists = File.Exists(configPath);

        // Assert
        configExists.Should().BeTrue($"Ocelot dev configuration should exist at {configPath}");
    }

    [Fact]
    public void OcelotProdConfiguration_IsValid()
    {
        // Arrange
        var configPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..",
            "Gateway.Api",
            "ocelot.prod.json"
        );

        // Act
        var configExists = File.Exists(configPath);

        // Assert
        configExists.Should().BeTrue($"Ocelot prod configuration should exist at {configPath}");
    }

    [Theory]
    [InlineData("ocelot.dev.json")]
    [InlineData("ocelot.prod.json")]
    public void OcelotConfiguration_HasValidJsonFormat(string fileName)
    {
        // Arrange
        var configPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..",
            "Gateway.Api",
            fileName
        );

        // Act
        var content = File.ReadAllText(configPath);
        var act = () => System.Text.Json.JsonDocument.Parse(content);

        // Assert
        act.Should().NotThrow($"{fileName} should be valid JSON");
    }

    [Theory]
    [InlineData("ocelot.dev.json")]
    [InlineData("ocelot.prod.json")]
    public void OcelotConfiguration_HasRoutesProperty(string fileName)
    {
        // Arrange
        var configPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..",
            "Gateway.Api",
            fileName
        );

        // Act
        var content = File.ReadAllText(configPath);
        var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
        var hasRoutes = jsonDoc.RootElement.TryGetProperty("Routes", out var routes);

        // Assert
        hasRoutes.Should().BeTrue($"{fileName} should have Routes property");
        routes.GetArrayLength().Should().BeGreaterThan(0, $"{fileName} should have at least one route");
    }

    [Fact]
    public void OcelotDevConfiguration_HasQoSOptions()
    {
        // Arrange
        var configPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..",
            "Gateway.Api",
            "ocelot.dev.json"
        );

        // Act
        var content = File.ReadAllText(configPath);
        var hasQoS = content.Contains("QoSOptions");

        // Assert
        hasQoS.Should().BeTrue("Dev configuration should have QoS (Quality of Service) options for circuit breaker");
    }

    [Fact]
    public void OcelotConfiguration_HasCircuitBreakerSettings()
    {
        // Arrange
        var configPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..",
            "Gateway.Api",
            "ocelot.dev.json"
        );

        // Act
        var content = File.ReadAllText(configPath);
        var hasCircuitBreaker = content.Contains("ExceptionsAllowedBeforeBreaking") &&
                                 content.Contains("DurationOfBreak");

        // Assert
        hasCircuitBreaker.Should().BeTrue("Configuration should have circuit breaker settings");
    }
}
