namespace CarDealer.Shared.Observability.Models;

/// <summary>
/// Configuration options for OpenTelemetry observability
/// </summary>
public class ObservabilityOptions
{
    public const string SectionName = "Observability";

    /// <summary>
    /// Name of the service for tracing
    /// </summary>
    public string ServiceName { get; set; } = "UnknownService";

    /// <summary>
    /// Service version
    /// </summary>
    public string ServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Environment name
    /// </summary>
    public string Environment { get; set; } = "Development";

    // ============================================================================
    // Convenience properties for flat configuration
    // ============================================================================

    /// <summary>
    /// Enable tracing (convenience property)
    /// </summary>
    public bool TracingEnabled { get => Tracing.Enabled; set => Tracing.Enabled = value; }

    /// <summary>
    /// Enable metrics (convenience property)
    /// </summary>
    public bool MetricsEnabled { get => Metrics.Enabled; set => Metrics.Enabled = value; }

    /// <summary>
    /// OTLP endpoint URL (convenience property)
    /// </summary>
    public string OtlpEndpoint { get => Tracing.Otlp.Endpoint; set => Tracing.Otlp.Endpoint = value; }

    /// <summary>
    /// Sampling ratio 0.0 to 1.0 (convenience property)
    /// </summary>
    public double SamplingRatio { get => Tracing.SamplingRatio; set => Tracing.SamplingRatio = value; }

    /// <summary>
    /// Enable Prometheus metrics (convenience property)
    /// </summary>
    public bool PrometheusEnabled { get => Metrics.EnablePrometheus; set => Metrics.EnablePrometheus = value; }

    /// <summary>
    /// Paths to exclude from tracing (convenience property)
    /// </summary>
    public string[] ExcludedPaths { get => Tracing.ExcludedPaths; set => Tracing.ExcludedPaths = value; }

    // ============================================================================
    // Nested configuration objects
    // ============================================================================

    /// <summary>
    /// Tracing configuration
    /// </summary>
    public TracingOptions Tracing { get; set; } = new();

    /// <summary>
    /// Metrics configuration
    /// </summary>
    public MetricsOptions Metrics { get; set; } = new();
}

/// <summary>
/// Tracing (distributed tracing) configuration
/// </summary>
public class TracingOptions
{
    /// <summary>
    /// Enable distributed tracing
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// OTLP exporter configuration
    /// </summary>
    public OtlpExporterOptions Otlp { get; set; } = new();

    /// <summary>
    /// Enable console exporter (for debugging)
    /// </summary>
    public bool EnableConsoleExporter { get; set; } = false;

    /// <summary>
    /// Sampling ratio (0.0 to 1.0) - 1.0 means 100% sampling
    /// </summary>
    public double SamplingRatio { get; set; } = 1.0;

    /// <summary>
    /// Instrument HTTP client calls
    /// </summary>
    public bool InstrumentHttpClient { get; set; } = true;

    /// <summary>
    /// Instrument Entity Framework Core
    /// </summary>
    public bool InstrumentEfCore { get; set; } = true;

    /// <summary>
    /// Record exception details in spans
    /// </summary>
    public bool RecordException { get; set; } = true;

    /// <summary>
    /// Paths to exclude from tracing (e.g., /health, /metrics)
    /// </summary>
    public string[] ExcludedPaths { get; set; } = new[] { "/health", "/metrics", "/swagger" };
}

/// <summary>
/// Metrics configuration
/// </summary>
public class MetricsOptions
{
    /// <summary>
    /// Enable metrics collection
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Enable Prometheus exporter
    /// </summary>
    public bool EnablePrometheus { get; set; } = true;

    /// <summary>
    /// Prometheus scraping endpoint path
    /// </summary>
    public string PrometheusEndpoint { get; set; } = "/metrics";

    /// <summary>
    /// OTLP exporter for metrics
    /// </summary>
    public OtlpExporterOptions Otlp { get; set; } = new();

    /// <summary>
    /// Enable console exporter (for debugging)
    /// </summary>
    public bool EnableConsoleExporter { get; set; } = false;
}

/// <summary>
/// OTLP exporter configuration
/// </summary>
public class OtlpExporterOptions
{
    /// <summary>
    /// Enable OTLP exporter
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// OTLP endpoint URL (e.g., http://jaeger:4317 or http://otel-collector:4317)
    /// </summary>
    public string Endpoint { get; set; } = "http://jaeger:4317";

    /// <summary>
    /// Protocol: grpc or http/protobuf
    /// </summary>
    public string Protocol { get; set; } = "grpc";

    /// <summary>
    /// Timeout in milliseconds
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 10000;

    /// <summary>
    /// Additional headers (key=value format, comma separated)
    /// </summary>
    public string? Headers { get; set; }
}
