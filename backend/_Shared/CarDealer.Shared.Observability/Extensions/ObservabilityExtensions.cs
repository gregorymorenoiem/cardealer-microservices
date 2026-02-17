using CarDealer.Shared.Observability.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CarDealer.Shared.Observability.Extensions;

/// <summary>
/// Extension methods for adding OpenTelemetry observability
/// </summary>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Adds standardized OpenTelemetry tracing and metrics
    /// </summary>
    public static IServiceCollection AddStandardObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        string? serviceVersion = null)
    {
        var options = new ObservabilityOptions
        {
            ServiceName = serviceName,
            ServiceVersion = serviceVersion ?? "1.0.0"
        };
        configuration.GetSection(ObservabilityOptions.SectionName).Bind(options);

        services.Configure<ObservabilityOptions>(opt =>
        {
            opt.ServiceName = serviceName;
            opt.ServiceVersion = serviceVersion ?? "1.0.0";
            configuration.GetSection(ObservabilityOptions.SectionName).Bind(opt);
        });

        // Configure OpenTelemetry
        var otelBuilder = services.AddOpenTelemetry();

        // Configure resource
        otelBuilder.ConfigureResource(resource =>
        {
            resource
                .AddService(
                    serviceName: options.ServiceName,
                    serviceVersion: options.ServiceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = options.Environment,
                    ["host.name"] = Environment.MachineName
                });
        });

        // Configure tracing
        if (options.Tracing.Enabled)
        {
            otelBuilder.WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(aspnetOptions =>
                    {
                        aspnetOptions.RecordException = options.Tracing.RecordException;
                        aspnetOptions.Filter = httpContext =>
                        {
                            // Exclude health check endpoints from tracing
                            var path = httpContext.Request.Path.Value?.ToLowerInvariant();
                            return path != "/health" && path != "/healthz" && path != "/ready";
                        };
                    });

                if (options.Tracing.InstrumentHttpClient)
                {
                    tracing.AddHttpClientInstrumentation(httpOptions =>
                    {
                        httpOptions.RecordException = options.Tracing.RecordException;
                    });
                }

                if (options.Tracing.InstrumentEfCore)
                {
                    // Note: EF Core instrumentation API changed - using simple version without options
                    tracing.AddEntityFrameworkCoreInstrumentation();
                }

                // Set sampler
                if (options.Tracing.SamplingRatio < 1.0)
                {
                    tracing.SetSampler(new TraceIdRatioBasedSampler(options.Tracing.SamplingRatio));
                }
                else
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                // Add OTLP exporter
                if (options.Tracing.Otlp.Enabled)
                {
                    tracing.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(options.Tracing.Otlp.Endpoint);
                        otlpOptions.Protocol = options.Tracing.Otlp.Protocol.ToLowerInvariant() == "grpc"
                            ? OtlpExportProtocol.Grpc
                            : OtlpExportProtocol.HttpProtobuf;
                        otlpOptions.TimeoutMilliseconds = options.Tracing.Otlp.TimeoutMilliseconds;

                        if (!string.IsNullOrEmpty(options.Tracing.Otlp.Headers))
                        {
                            otlpOptions.Headers = options.Tracing.Otlp.Headers;
                        }
                    });
                }

                // Add console exporter for debugging
                if (options.Tracing.EnableConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }
            });
        }

        // Configure metrics
        if (options.Metrics.Enabled)
        {
            otelBuilder.WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
                    // Note: AddRuntimeInstrumentation() requires OpenTelemetry.Instrumentation.Runtime package
                    // Uncomment when package is added: .AddRuntimeInstrumentation();

                // Add OTLP exporter for metrics
                if (options.Metrics.Otlp.Enabled)
                {
                    metrics.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(options.Metrics.Otlp.Endpoint);
                        otlpOptions.Protocol = options.Metrics.Otlp.Protocol.ToLowerInvariant() == "grpc"
                            ? OtlpExportProtocol.Grpc
                            : OtlpExportProtocol.HttpProtobuf;
                    });
                }

                // Add Prometheus exporter
                if (options.Metrics.EnablePrometheus)
                {
                    metrics.AddPrometheusExporter();
                }

                // Add console exporter for debugging
                if (options.Metrics.EnableConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }
            });
        }

        return services;
    }

    /// <summary>
    /// Adds standardized OpenTelemetry tracing and metrics with a simplified configuration callback
    /// </summary>
    public static IServiceCollection AddStandardObservability(
        this IServiceCollection services,
        string serviceName,
        Action<ObservabilityOptions>? configureOptions = null)
    {
        var options = new ObservabilityOptions
        {
            ServiceName = serviceName,
            ServiceVersion = "1.0.0"
        };
        
        // Allow programmatic configuration
        configureOptions?.Invoke(options);

        services.Configure<ObservabilityOptions>(opt =>
        {
            opt.ServiceName = serviceName;
            opt.ServiceVersion = options.ServiceVersion;
            opt.Environment = options.Environment;
            opt.Tracing = options.Tracing;
            opt.Metrics = options.Metrics;
        });

        // Configure OpenTelemetry
        var otelBuilder = services.AddOpenTelemetry();

        // Configure resource
        otelBuilder.ConfigureResource(resource =>
        {
            resource
                .AddService(
                    serviceName: options.ServiceName,
                    serviceVersion: options.ServiceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = options.Environment,
                    ["host.name"] = Environment.MachineName
                });
        });

        // Configure tracing
        if (options.Tracing.Enabled)
        {
            otelBuilder.WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(aspnetOptions =>
                    {
                        aspnetOptions.RecordException = options.Tracing.RecordException;
                        aspnetOptions.Filter = httpContext =>
                        {
                            // Exclude health check and other endpoints from tracing
                            var path = httpContext.Request.Path.Value?.ToLowerInvariant();
                            if (path == null) return true;
                            
                            // Check against excluded paths
                            foreach (var excludedPath in options.Tracing.ExcludedPaths)
                            {
                                if (path.StartsWith(excludedPath.ToLowerInvariant()))
                                    return false;
                            }
                            return true;
                        };
                    });

                if (options.Tracing.InstrumentHttpClient)
                {
                    tracing.AddHttpClientInstrumentation(httpOptions =>
                    {
                        httpOptions.RecordException = options.Tracing.RecordException;
                    });
                }

                if (options.Tracing.InstrumentEfCore)
                {
                    // Note: EF Core instrumentation API changed - using simple version without options
                    tracing.AddEntityFrameworkCoreInstrumentation();
                }

                // Set sampler
                if (options.Tracing.SamplingRatio < 1.0)
                {
                    tracing.SetSampler(new TraceIdRatioBasedSampler(options.Tracing.SamplingRatio));
                }
                else
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                // Add OTLP exporter
                if (options.Tracing.Otlp.Enabled)
                {
                    tracing.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(options.Tracing.Otlp.Endpoint);
                        otlpOptions.Protocol = options.Tracing.Otlp.Protocol.ToLowerInvariant() == "grpc"
                            ? OtlpExportProtocol.Grpc
                            : OtlpExportProtocol.HttpProtobuf;
                        otlpOptions.TimeoutMilliseconds = options.Tracing.Otlp.TimeoutMilliseconds;

                        if (!string.IsNullOrEmpty(options.Tracing.Otlp.Headers))
                        {
                            otlpOptions.Headers = options.Tracing.Otlp.Headers;
                        }
                    });
                }

                // Add console exporter for debugging
                if (options.Tracing.EnableConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }
            });
        }

        // Configure metrics
        if (options.Metrics.Enabled)
        {
            otelBuilder.WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                // Add OTLP exporter for metrics
                if (options.Metrics.Otlp.Enabled)
                {
                    metrics.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(options.Metrics.Otlp.Endpoint);
                        otlpOptions.Protocol = options.Metrics.Otlp.Protocol.ToLowerInvariant() == "grpc"
                            ? OtlpExportProtocol.Grpc
                            : OtlpExportProtocol.HttpProtobuf;
                    });
                }

                // Add Prometheus exporter
                if (options.Metrics.EnablePrometheus)
                {
                    metrics.AddPrometheusExporter();
                }

                // Add console exporter for debugging
                if (options.Metrics.EnableConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }
            });
        }

        return services;
    }

    /// <summary>
    /// Maps Prometheus scraping endpoint (call after UseRouting)
    /// </summary>
    public static IApplicationBuilder UsePrometheusScrapingEndpoint(
        this IApplicationBuilder app,
        string endpoint = "/metrics")
    {
        // OpenTelemetry Prometheus exporter adds the endpoint automatically
        // This method is for compatibility and configuration
        return app.UseOpenTelemetryPrometheusScrapingEndpoint();
    }
}
