// =============================================================================
// LLM Gateway — Prometheus Metrics Endpoint
//
// Exposes cost and distribution metrics in Prometheus text format at /metrics.
// These gauges are scraped by Prometheus every 15s and trigger alert rules in
// llm-cost-alerts.yml for the three cost thresholds ($300/$500/$700).
//
// Metrics exposed:
//   okla_llm_cost_usd_monthly         — gauge — current month total cost in USD
//   okla_llm_cost_usd_daily           — gauge — today's cost in USD
//   okla_llm_aggressive_mode_active   — gauge — 1 if aggressive cache mode on
//   okla_llm_requests_by_provider     — gauge — request % per provider
//   okla_llm_cost_by_provider_usd     — gauge — cost per provider this month
// =============================================================================

using CarDealer.Shared.LlmGateway.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text;

namespace CarDealer.Shared.LlmGateway.Endpoints;

public static class LlmCostMetricsEndpoint
{
    /// <summary>
    /// Map the /metrics/llm endpoint for Prometheus scraping.
    /// Call alongside MapLlmGatewayEndpoints:
    ///   app.MapLlmCostMetrics();
    /// </summary>
    public static IEndpointRouteBuilder MapLlmCostMetrics(this IEndpointRouteBuilder app)
    {
        app.MapGet("/metrics/llm", async (HttpContext ctx, CancellationToken ct) =>
        {
            var sb = new StringBuilder(2048);
            var costTracker = ctx.RequestServices.GetService<IApiCostTracker>();
            var inv = CultureInfo.InvariantCulture;

            // ── COST METRICS ─────────────────────────────────────────────────
            if (costTracker != null)
            {
                var breakdown = await costTracker.GetCostBreakdownAsync(ct);

                sb.AppendLine("# HELP okla_llm_cost_usd_monthly Current month total LLM API cost in USD");
                sb.AppendLine("# TYPE okla_llm_cost_usd_monthly gauge");
                sb.AppendLine(string.Create(inv, $"okla_llm_cost_usd_monthly {breakdown.MonthlyTotalUsd:F4}"));

                sb.AppendLine("# HELP okla_llm_cost_usd_daily Today total LLM API cost in USD");
                sb.AppendLine("# TYPE okla_llm_cost_usd_daily gauge");
                sb.AppendLine(string.Create(inv, $"okla_llm_cost_usd_daily {breakdown.DailyTotalUsd:F4}"));

                sb.AppendLine("# HELP okla_llm_cost_usd_projected Projected monthly cost based on daily rate");
                sb.AppendLine("# TYPE okla_llm_cost_usd_projected gauge");
                sb.AppendLine(string.Create(inv, $"okla_llm_cost_usd_projected {breakdown.ProjectedMonthlyUsd:F4}"));

                sb.AppendLine("# HELP okla_llm_aggressive_mode_active Whether aggressive cache mode is active (1=yes 0=no)");
                sb.AppendLine("# TYPE okla_llm_aggressive_mode_active gauge");
                sb.AppendLine(string.Create(inv, $"okla_llm_aggressive_mode_active {(breakdown.IsAggressiveCacheModeActive ? 1 : 0)}"));

                // Per-provider cost breakdown
                sb.AppendLine("# HELP okla_llm_cost_by_provider_usd Monthly cost per LLM provider in USD");
                sb.AppendLine("# TYPE okla_llm_cost_by_provider_usd gauge");
                foreach (var (provider, cost) in breakdown.ByProvider)
                {
                    sb.AppendLine(string.Create(inv, $"okla_llm_cost_by_provider_usd{{provider=\"{provider}\"}} {cost:F4}"));
                }

                // Per-agent cost breakdown
                sb.AppendLine("# HELP okla_llm_cost_by_agent_usd Monthly cost per agent in USD");
                sb.AppendLine("# TYPE okla_llm_cost_by_agent_usd gauge");
                foreach (var (agent, cost) in breakdown.ByAgent)
                {
                    sb.AppendLine(string.Create(inv, $"okla_llm_cost_by_agent_usd{{agent=\"{agent}\"}} {cost:F4}"));
                }
            }

            // ── DISTRIBUTION METRICS ─────────────────────────────────────────
            var dist = LlmGatewayService.GetModelDistribution();

            sb.AppendLine("# HELP okla_llm_requests_by_provider_percent Percentage of requests per provider");
            sb.AppendLine("# TYPE okla_llm_requests_by_provider_percent gauge");
            sb.AppendLine(string.Create(inv, $"okla_llm_requests_by_provider_percent{{provider=\"claude\"}} {dist.Claude:F1}"));
            sb.AppendLine(string.Create(inv, $"okla_llm_requests_by_provider_percent{{provider=\"gemini\"}} {dist.Gemini:F1}"));
            sb.AppendLine(string.Create(inv, $"okla_llm_requests_by_provider_percent{{provider=\"llama\"}} {dist.Llama:F1}"));
            sb.AppendLine(string.Create(inv, $"okla_llm_requests_by_provider_percent{{provider=\"cache\"}} {dist.Cache:F1}"));

            sb.AppendLine("# HELP okla_llm_requests_total_count Total LLM requests since startup");
            sb.AppendLine("# TYPE okla_llm_requests_total_count gauge");
            sb.AppendLine(string.Create(inv, $"okla_llm_requests_total_count {dist.TotalRequests}"));

            ctx.Response.ContentType = "text/plain; version=0.0.4; charset=utf-8";
            await ctx.Response.WriteAsync(sb.ToString(), ct);
        })
        .WithName("LlmCostMetrics")
        .WithDescription("Prometheus metrics endpoint for LLM cost tracking")
        .AllowAnonymous()  // Prometheus needs unauthenticated access
        .ExcludeFromDescription();  // Hide from Swagger/OpenAPI

        return app;
    }
}
