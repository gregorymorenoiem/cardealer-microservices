using CarDealer.Shared.Observability.Models;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Observability;

public class ObservabilityOptionsTests
{
    // ── SectionName ──────────────────────────────────────────────────
    [Fact]
    public void SectionName_ShouldBe_Observability()
    {
        ObservabilityOptions.SectionName.Should().Be("Observability");
    }

    // ── Top-level defaults ───────────────────────────────────────────
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var opts = new ObservabilityOptions();

        opts.ServiceName.Should().Be("UnknownService");
        opts.ServiceVersion.Should().Be("1.0.0");
        opts.Environment.Should().Be("Development");
    }

    // ── Convenience property delegation ──────────────────────────────
    [Fact]
    public void TracingEnabled_ShouldDelegateToNestedTracing()
    {
        var opts = new ObservabilityOptions();
        opts.TracingEnabled.Should().BeTrue();

        opts.TracingEnabled = false;
        opts.Tracing.Enabled.Should().BeFalse();
    }

    [Fact]
    public void MetricsEnabled_ShouldDelegateToNestedMetrics()
    {
        var opts = new ObservabilityOptions();
        opts.MetricsEnabled.Should().BeTrue();

        opts.MetricsEnabled = false;
        opts.Metrics.Enabled.Should().BeFalse();
    }

    [Fact]
    public void OtlpEndpoint_ShouldDelegateToNestedTracing()
    {
        var opts = new ObservabilityOptions();
        opts.OtlpEndpoint.Should().Be("http://jaeger:4317");

        opts.OtlpEndpoint = "http://custom:4317";
        opts.Tracing.Otlp.Endpoint.Should().Be("http://custom:4317");
    }

    [Fact]
    public void SamplingRatio_ShouldDelegateToNestedTracing()
    {
        var opts = new ObservabilityOptions();
        opts.SamplingRatio.Should().Be(1.0);

        opts.SamplingRatio = 0.5;
        opts.Tracing.SamplingRatio.Should().Be(0.5);
    }

    [Fact]
    public void PrometheusEnabled_ShouldDelegateToNestedMetrics()
    {
        var opts = new ObservabilityOptions();
        opts.PrometheusEnabled.Should().BeTrue();

        opts.PrometheusEnabled = false;
        opts.Metrics.EnablePrometheus.Should().BeFalse();
    }

    [Fact]
    public void ExcludedPaths_ShouldDelegateToNestedTracing()
    {
        var opts = new ObservabilityOptions();
        opts.ExcludedPaths.Should().BeEquivalentTo(new[] { "/health", "/metrics", "/swagger" });

        opts.ExcludedPaths = new[] { "/custom" };
        opts.Tracing.ExcludedPaths.Should().BeEquivalentTo(new[] { "/custom" });
    }

    // ── TracingOptions defaults ──────────────────────────────────────
    [Fact]
    public void TracingOptions_DefaultValues_ShouldBeCorrect()
    {
        var tracing = new TracingOptions();

        tracing.Enabled.Should().BeTrue();
        tracing.EnableConsoleExporter.Should().BeFalse();
        tracing.SamplingRatio.Should().Be(1.0);
        tracing.InstrumentHttpClient.Should().BeTrue();
        tracing.InstrumentEfCore.Should().BeTrue();
        tracing.RecordException.Should().BeTrue();
        tracing.ExcludedPaths.Should().Contain("/health");
        tracing.ExcludedPaths.Should().Contain("/metrics");
        tracing.ExcludedPaths.Should().Contain("/swagger");
    }

    // ── MetricsOptions defaults ──────────────────────────────────────
    [Fact]
    public void MetricsOptions_DefaultValues_ShouldBeCorrect()
    {
        var metrics = new MetricsOptions();

        metrics.Enabled.Should().BeTrue();
        metrics.EnablePrometheus.Should().BeTrue();
        metrics.PrometheusEndpoint.Should().Be("/metrics");
        metrics.EnableConsoleExporter.Should().BeFalse();
    }

    // ── OtlpExporterOptions defaults ─────────────────────────────────
    [Fact]
    public void OtlpExporterOptions_DefaultValues_ShouldBeCorrect()
    {
        var otlp = new OtlpExporterOptions();

        otlp.Enabled.Should().BeTrue();
        otlp.Endpoint.Should().Be("http://jaeger:4317");
        otlp.Protocol.Should().Be("grpc");
        otlp.TimeoutMilliseconds.Should().Be(10000);
        otlp.Headers.Should().BeNull();
    }
}
