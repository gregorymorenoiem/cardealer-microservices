using CarDealer.Shared.ErrorHandling.Models;
using FluentAssertions;

namespace CarDealer.Shared.Tests.ErrorHandling;

public class ErrorHandlingOptionsTests
{
    [Fact]
    public void ErrorHandlingOptions_DefaultValues_ShouldBeCorrect()
    {
        var options = new ErrorHandlingOptions();

        options.ServiceName.Should().Be("UnknownService");
        options.Environment.Should().Be("Development");
        options.IncludeStackTrace.Should().BeFalse();
        options.IncludeExceptionDetails.Should().BeFalse();
        options.PublishToErrorService.Should().BeTrue();
        options.RabbitMQHost.Should().Be("rabbitmq");
        options.RabbitMQPort.Should().Be(5672);
        options.RabbitMQUser.Should().Be("guest");
        options.RabbitMQPassword.Should().Be("guest");
    }

    [Fact]
    public void ErrorHandlingOptions_SectionName_ShouldBeCorrect()
    {
        ErrorHandlingOptions.SectionName.Should().Be("ErrorHandling");
    }

    [Fact]
    public void ErrorHandlingOptions_ErrorServiceOptions_ShouldHaveDefaults()
    {
        var options = new ErrorHandlingOptions();

        options.ErrorService.Should().NotBeNull();
        options.ErrorService.Enabled.Should().BeTrue();
        options.ErrorService.BaseUrl.Should().Be("http://errorservice:8080");
        options.ErrorService.TimeoutSeconds.Should().Be(5);
    }

    [Fact]
    public void ErrorHandlingOptions_RabbitMQErrorOptions_ShouldHaveDefaults()
    {
        var options = new ErrorHandlingOptions();

        options.RabbitMQ.Should().NotBeNull();
        options.RabbitMQ.Enabled.Should().BeTrue();
        options.RabbitMQ.Hostname.Should().Be("rabbitmq");
        options.RabbitMQ.Port.Should().Be(5672);
        options.RabbitMQ.Username.Should().Be("guest");
        options.RabbitMQ.Password.Should().Be("guest");
        options.RabbitMQ.Exchange.Should().Be("errors.exchange");
        options.RabbitMQ.Queue.Should().Be("errors.queue");
        options.RabbitMQ.RoutingKey.Should().Be("error.created");
    }

    [Fact]
    public void ErrorHandlingOptions_CustomValues_ShouldBeRetained()
    {
        var options = new ErrorHandlingOptions
        {
            ServiceName = "MyService",
            Environment = "Production",
            IncludeStackTrace = true,
            IncludeExceptionDetails = true,
            PublishToErrorService = false,
            RabbitMQHost = "custom-rabbitmq",
            RabbitMQPort = 5673,
            RabbitMQUser = "admin",
            RabbitMQPassword = "secret"
        };

        options.ServiceName.Should().Be("MyService");
        options.Environment.Should().Be("Production");
        options.IncludeStackTrace.Should().BeTrue();
        options.IncludeExceptionDetails.Should().BeTrue();
        options.PublishToErrorService.Should().BeFalse();
        options.RabbitMQHost.Should().Be("custom-rabbitmq");
        options.RabbitMQPort.Should().Be(5673);
        options.RabbitMQUser.Should().Be("admin");
        options.RabbitMQPassword.Should().Be("secret");
    }

    [Fact]
    public void ErrorServiceOptions_CustomValues_ShouldBeRetained()
    {
        var serviceOptions = new ErrorServiceOptions
        {
            Enabled = false,
            BaseUrl = "http://custom:9090",
            TimeoutSeconds = 30
        };

        serviceOptions.Enabled.Should().BeFalse();
        serviceOptions.BaseUrl.Should().Be("http://custom:9090");
        serviceOptions.TimeoutSeconds.Should().Be(30);
    }

    [Fact]
    public void RabbitMQErrorOptions_CustomValues_ShouldBeRetained()
    {
        var mqOptions = new RabbitMQErrorOptions
        {
            Enabled = false,
            Hostname = "mq-prod",
            Port = 5671,
            Username = "prod-user",
            Password = "prod-pass",
            Exchange = "custom.exchange",
            Queue = "custom.queue",
            RoutingKey = "custom.routing"
        };

        mqOptions.Enabled.Should().BeFalse();
        mqOptions.Hostname.Should().Be("mq-prod");
        mqOptions.Port.Should().Be(5671);
        mqOptions.Username.Should().Be("prod-user");
        mqOptions.Password.Should().Be("prod-pass");
        mqOptions.Exchange.Should().Be("custom.exchange");
        mqOptions.Queue.Should().Be("custom.queue");
        mqOptions.RoutingKey.Should().Be("custom.routing");
    }
}
