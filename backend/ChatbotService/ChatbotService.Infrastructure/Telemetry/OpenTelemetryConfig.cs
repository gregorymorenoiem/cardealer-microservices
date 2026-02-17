using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ChatbotService.Infrastructure.Telemetry;

/// <summary>
/// R15 (MLOps): OpenTelemetry configuration for ChatbotService.
/// Provides distributed tracing between .NET ChatbotService and Python LLM Server.
///
/// Trace ID propagation:
///   Browser → ChatbotService (.NET) → LLM Server (Python)
///
/// The trace context is propagated via W3C TraceContext headers:
///   - traceparent: 00-{traceId}-{spanId}-{flags}
///   - tracestate: optional vendor-specific data
///
/// This enables end-to-end request tracing across the .NET ↔ Python boundary.
/// </summary>
public static class OpenTelemetryConfig
{
    /// <summary>
    /// ActivitySource for ChatbotService custom spans.
    /// Use this to create manual spans around important operations.
    /// </summary>
    public static readonly ActivitySource ChatbotActivitySource = new("OKLA.ChatbotService", "1.0.0");

    /// <summary>
    /// Registers OpenTelemetry tracing for ChatbotService.
    /// Call from Program.cs or DependencyInjection.cs.
    ///
    /// Requires NuGet packages:
    ///   - OpenTelemetry.Extensions.Hosting
    ///   - OpenTelemetry.Instrumentation.AspNetCore
    ///   - OpenTelemetry.Instrumentation.Http
    ///   - OpenTelemetry.Instrumentation.EntityFrameworkCore
    ///   - OpenTelemetry.Exporter.Prometheus (or OTLP)
    /// </summary>
    public static IServiceCollection AddChatbotTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Note: Full OpenTelemetry setup requires adding the NuGet packages above.
        // This configuration is ready to activate once packages are added.
        //
        // builder.Services.AddOpenTelemetry()
        //     .WithTracing(tracing =>
        //     {
        //         tracing
        //             .AddSource(ChatbotActivitySource.Name)
        //             .SetResourceBuilder(ResourceBuilder.CreateDefault()
        //                 .AddService("ChatbotService", serviceVersion: "1.0.0"))
        //             .AddAspNetCoreInstrumentation(options =>
        //             {
        //                 options.Filter = (httpContext) =>
        //                     !httpContext.Request.Path.StartsWithSegments("/health") &&
        //                     !httpContext.Request.Path.StartsWithSegments("/metrics");
        //             })
        //             .AddHttpClientInstrumentation(options =>
        //             {
        //                 // Enrich spans with LLM-specific attributes
        //                 options.EnrichWithHttpResponseMessage = (activity, response) =>
        //                 {
        //                     if (response.RequestMessage?.RequestUri?.Host == "llm-server")
        //                     {
        //                         activity.SetTag("llm.server", "okla-llama3-8b");
        //                         activity.SetTag("llm.endpoint", response.RequestMessage.RequestUri.PathAndQuery);
        //                     }
        //                 };
        //             })
        //             .AddEntityFrameworkCoreInstrumentation()
        //             .AddOtlpExporter(options =>
        //             {
        //                 options.Endpoint = new Uri(
        //                     configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://otel-collector:4317");
        //             });
        //     })
        //     .WithMetrics(metrics =>
        //     {
        //         metrics
        //             .AddAspNetCoreInstrumentation()
        //             .AddHttpClientInstrumentation()
        //             .AddPrometheusExporter();
        //     });

        return services;
    }
}

/// <summary>
/// Extension methods for adding tracing spans to LLM operations.
/// </summary>
public static class LlmTracingExtensions
{
    /// <summary>
    /// Creates a tracing span for an LLM inference call.
    /// The span includes LLM-specific attributes like model, tokens, etc.
    /// </summary>
    public static Activity? StartLlmInferenceSpan(
        string sessionId,
        string query,
        string model = "okla-llama3-8b")
    {
        var activity = OpenTelemetryConfig.ChatbotActivitySource.StartActivity(
            "llm.inference",
            ActivityKind.Client);

        if (activity != null)
        {
            activity.SetTag("llm.model", model);
            activity.SetTag("llm.session_id", sessionId);
            activity.SetTag("llm.query_length", query.Length);
            activity.SetTag("llm.query_preview", query[..Math.Min(50, query.Length)]);
        }

        return activity;
    }

    /// <summary>
    /// Enriches an existing span with LLM response details.
    /// </summary>
    public static void EnrichWithLlmResponse(
        this Activity? activity,
        string? intent,
        float confidence,
        int tokensUsed,
        long responseTimeMs,
        bool isFallback)
    {
        if (activity == null) return;

        activity.SetTag("llm.intent", intent ?? "unknown");
        activity.SetTag("llm.confidence", confidence);
        activity.SetTag("llm.tokens_used", tokensUsed);
        activity.SetTag("llm.response_time_ms", responseTimeMs);
        activity.SetTag("llm.is_fallback", isFallback);

        if (isFallback)
            activity.SetStatus(ActivityStatusCode.Error, "LLM returned fallback response");
        else
            activity.SetStatus(ActivityStatusCode.Ok);
    }
}
