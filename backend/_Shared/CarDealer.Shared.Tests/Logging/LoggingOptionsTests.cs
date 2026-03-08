using CarDealer.Shared.Logging.Models;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Logging;

public class LoggingOptionsTests
{
    // ── SectionName ──────────────────────────────────────────────────
    [Fact]
    public void SectionName_ShouldBe_Logging()
    {
        LoggingOptions.SectionName.Should().Be("Logging");
    }

    // ── Top-level defaults ───────────────────────────────────────────
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var opts = new LoggingOptions();

        opts.ServiceName.Should().Be("UnknownService");
        opts.Environment.Should().Be("Development");
        opts.MinimumLevel.Should().Be("Information");
        opts.EnableConsole.Should().BeTrue();
    }

    // ── Seq convenience properties ───────────────────────────────────
    [Fact]
    public void SeqEnabled_ShouldDelegateToNestedSeqOptions()
    {
        var opts = new LoggingOptions();

        // Default from nested SeqOptions
        opts.SeqEnabled.Should().BeTrue();

        // Set via convenience → read from nested
        opts.SeqEnabled = false;
        opts.Seq.Enabled.Should().BeFalse();
    }

    [Fact]
    public void SeqServerUrl_ShouldDelegateToNestedSeqOptions()
    {
        var opts = new LoggingOptions();
        opts.SeqServerUrl.Should().Be("http://seq:5341");

        opts.SeqServerUrl = "http://custom:1234";
        opts.Seq.ServerUrl.Should().Be("http://custom:1234");
    }

    // ── File convenience properties ──────────────────────────────────
    [Fact]
    public void FileEnabled_ShouldDelegateToNestedFileOptions()
    {
        var opts = new LoggingOptions();
        opts.FileEnabled.Should().BeFalse();

        opts.FileEnabled = true;
        opts.File.Enabled.Should().BeTrue();
    }

    [Fact]
    public void FilePath_ShouldDelegateToNestedFileOptions()
    {
        var opts = new LoggingOptions();
        opts.FilePath.Should().Be("logs/log-.txt");

        opts.FilePath = "/var/log/custom.txt";
        opts.File.Path.Should().Be("/var/log/custom.txt");
    }

    // ── RabbitMQ convenience property ────────────────────────────────
    [Fact]
    public void RabbitMQEnabled_ShouldDelegateToNestedRabbitMQOptions()
    {
        var opts = new LoggingOptions();
        opts.RabbitMQEnabled.Should().BeFalse();

        opts.RabbitMQEnabled = true;
        opts.RabbitMQ.Enabled.Should().BeTrue();
    }

    // ── Nested SeqOptions defaults ───────────────────────────────────
    [Fact]
    public void SeqOptions_DefaultValues_ShouldBeCorrect()
    {
        var seq = new SeqOptions();

        seq.Enabled.Should().BeTrue();
        seq.ServerUrl.Should().Be("http://seq:5341");
        seq.ApiKey.Should().BeNull();
        seq.MinimumLevel.Should().Be("Information");
    }

    // ── Nested RabbitMQLogOptions defaults ───────────────────────────
    [Fact]
    public void RabbitMQLogOptions_DefaultValues_ShouldBeCorrect()
    {
        var rmq = new RabbitMQLogOptions();

        rmq.Enabled.Should().BeFalse();
        rmq.Hostnames.Should().ContainSingle().Which.Should().Be("rabbitmq");
        rmq.Port.Should().Be(5672);
        rmq.Username.Should().Be("guest");
        rmq.Password.Should().Be("guest");
        rmq.Exchange.Should().Be("logs.exchange");
        rmq.ExchangeType.Should().Be("topic");
        rmq.RouteKey.Should().Be("logs.{Level}");
        rmq.MinimumLevel.Should().Be("Warning");
    }

    // ── Nested FileLogOptions defaults ───────────────────────────────
    [Fact]
    public void FileLogOptions_DefaultValues_ShouldBeCorrect()
    {
        var file = new FileLogOptions();

        file.Enabled.Should().BeFalse();
        file.Path.Should().Be("logs/log-.txt");
        file.RollingInterval.Should().Be("Day");
        file.RetainedFileCountLimit.Should().Be(7);
    }
}
