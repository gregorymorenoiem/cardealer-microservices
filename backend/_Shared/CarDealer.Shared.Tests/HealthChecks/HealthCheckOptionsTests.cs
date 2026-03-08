using CarDealer.Shared.HealthChecks.Configuration;
using FluentAssertions;

namespace CarDealer.Shared.Tests.HealthChecks;

public class StandardHealthCheckOptionsTests
{
    [Fact]
    public void SectionName_ShouldBeHealthChecks()
    {
        StandardHealthCheckOptions.SectionName.Should().Be("HealthChecks");
    }

    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var options = new StandardHealthCheckOptions();

        options.Enabled.Should().BeTrue();
        options.PostgreSQL.Should().NotBeNull();
        options.Redis.Should().NotBeNull();
        options.RabbitMQ.Should().NotBeNull();
        options.ExternalServices.Should().NotBeNull();
        options.Endpoints.Should().NotBeNull();
    }
}

public class DatabaseHealthCheckOptionsTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var options = new DatabaseHealthCheckOptions();

        options.Enabled.Should().BeTrue();
        options.ConnectionString.Should().BeNull();
        options.Name.Should().Be("postgresql");
        options.Tags.Should().Contain("ready");
        options.TimeoutSeconds.Should().Be(10);
    }
}

public class RedisHealthCheckOptionsTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var options = new RedisHealthCheckOptions();

        options.Enabled.Should().BeFalse(); // Redis disabled by default
        options.ConnectionString.Should().Be("localhost:6379");
        options.Name.Should().Be("redis");
        options.Tags.Should().Contain("ready");
        options.Tags.Should().Contain("cache");
        options.TimeoutSeconds.Should().Be(5);
    }
}

public class RabbitMQHealthCheckOptionsTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var options = new RabbitMQHealthCheckOptions();

        options.Enabled.Should().BeFalse(); // RabbitMQ disabled by default
        options.Host.Should().Be("localhost");
        options.Port.Should().Be(5672);
        options.Username.Should().Be("guest");
        options.Password.Should().Be("guest");
        options.VirtualHost.Should().Be("/");
        options.Name.Should().Be("rabbitmq");
        options.Tags.Should().Contain("ready");
        options.Tags.Should().Contain("messaging");
        options.TimeoutSeconds.Should().Be(5);
    }
}

public class ExternalServicesHealthCheckOptionsTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var options = new ExternalServicesHealthCheckOptions();

        options.Enabled.Should().BeFalse();
        options.Services.Should().BeEmpty();
    }

    [Fact]
    public void Services_ShouldBeSettable()
    {
        var options = new ExternalServicesHealthCheckOptions();
        options.Services["payment-gateway"] = new ExternalServiceConfig
        {
            Url = "https://api.azul.com.do/health",
            Name = "Azul Payment Gateway",
            TimeoutSeconds = 15
        };

        options.Services.Should().HaveCount(1);
        options.Services["payment-gateway"].TimeoutSeconds.Should().Be(15);
    }
}

public class ExternalServiceConfigTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var config = new ExternalServiceConfig();

        config.Url.Should().BeEmpty();
        config.Name.Should().BeEmpty();
        config.Tags.Should().Contain("ready");
        config.TimeoutSeconds.Should().Be(10);
    }
}

public class HealthCheckEndpointsOptionsTests
{
    [Fact]
    public void Defaults_ShouldMatchProjectStandards()
    {
        var options = new HealthCheckEndpointsOptions();

        options.LivePath.Should().Be("/health/live");
        options.ReadyPath.Should().Be("/health/ready");
        options.HealthPath.Should().Be("/health");
        options.IncludeDetails.Should().BeTrue();
    }
}
