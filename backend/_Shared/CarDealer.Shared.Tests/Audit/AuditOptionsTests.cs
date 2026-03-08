using CarDealer.Shared.Audit.Configuration;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Audit;

public class AuditOptionsTests
{
    [Fact]
    public void SectionName_ShouldBeAudit()
    {
        AuditOptions.SectionName.Should().Be("Audit");
    }

    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var options = new AuditOptions();

        options.ServiceName.Should().Be("UnknownService");
        options.Enabled.Should().BeTrue();
        options.ExchangeName.Should().Be("audit.events");
        options.RoutingKey.Should().Be("audit.event");
    }

    [Fact]
    public void RabbitMq_Defaults_ShouldBeCorrect()
    {
        var options = new AuditOptions();
        var rabbit = options.RabbitMq;

        rabbit.Should().NotBeNull();
        rabbit.Host.Should().Be("rabbitmq");
        rabbit.Port.Should().Be(5672);
        rabbit.Username.Should().Be("guest");
        rabbit.Password.Should().Be("guest");
        rabbit.VirtualHost.Should().Be("/");
    }

    [Fact]
    public void AutoAudit_Defaults_ShouldBeCorrect()
    {
        var options = new AuditOptions();
        var autoAudit = options.AutoAudit;

        autoAudit.Should().NotBeNull();
        autoAudit.HttpRequests.Should().BeFalse();
        autoAudit.OnlyMutations.Should().BeTrue();
    }

    [Fact]
    public void AutoAudit_ExcludePaths_ShouldContainDefaults()
    {
        var options = new AuditOptions();

        options.AutoAudit.ExcludePaths.Should().Contain("/health");
        options.AutoAudit.ExcludePaths.Should().Contain("/metrics");
        options.AutoAudit.ExcludePaths.Should().Contain("/swagger");
        options.AutoAudit.ExcludePaths.Should().Contain("/api/health");
    }

    [Fact]
    public void AutoAudit_AuditableEvents_ShouldContainCriticalEvents()
    {
        var options = new AuditOptions();

        options.AutoAudit.AuditableEvents.Should().Contain("Login");
        options.AutoAudit.AuditableEvents.Should().Contain("Logout");
        options.AutoAudit.AuditableEvents.Should().Contain("PasswordChange");
        options.AutoAudit.AuditableEvents.Should().Contain("PaymentCreated");
        options.AutoAudit.AuditableEvents.Should().Contain("VehicleCreated");
    }

    [Fact]
    public void RabbitMqConfig_ShouldBeSettable()
    {
        var config = new RabbitMqConfig
        {
            Host = "prod-rabbit",
            Port = 5673,
            Username = "admin",
            Password = "secret",
            VirtualHost = "/prod"
        };

        config.Host.Should().Be("prod-rabbit");
        config.Port.Should().Be(5673);
        config.Username.Should().Be("admin");
        config.Password.Should().Be("secret");
        config.VirtualHost.Should().Be("/prod");
    }
}
