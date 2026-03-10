// =============================================================================
// LLM Gateway — Admin Dashboard Endpoints
//
// Provides real-time monitoring of:
//   - Request distribution by model (Claude/Gemini/Llama/Cache) in last 24h
//   - Health status of all providers
//   - Fallback event history
//   - Force degraded mode toggle
//
// Route: /api/admin/llm-gateway
// Authorization: Admin only
// =============================================================================

using CarDealer.Shared.LlmGateway.Abstractions;
using CarDealer.Shared.LlmGateway.Configuration;
using CarDealer.Shared.LlmGateway.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CarDealer.Shared.LlmGateway.Endpoints;

public static class LlmGatewayEndpoints
{
    /// <summary>
    /// Map LLM Gateway admin endpoints to the route builder.
    /// Usage: app.MapLlmGatewayEndpoints();
    /// </summary>
    public static IEndpointRouteBuilder MapLlmGatewayEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/llm-gateway")
            .RequireAuthorization("AdminOnly")
            .WithTags("LLM Gateway Admin");

        // GET /api/admin/llm-gateway/distribution
        // Returns real-time % of requests per model
        group.MapGet("/distribution", () =>
        {
            var dist = LlmGatewayService.GetModelDistribution();
            return Results.Ok(new
            {
                dist.Claude,
                dist.Gemini,
                dist.Llama,
                dist.Cache,
                dist.TotalRequests,
                dist.Since,
                Summary = $"Claude: {dist.Claude}% | Gemini: {dist.Gemini}% | " +
                          $"Llama: {dist.Llama}% | Cache: {dist.Cache}%"
            });
        })
        .WithName("GetLlmModelDistribution")
        .WithDescription("Get real-time percentage of requests resolved by each LLM model in the last 24 hours");

        // GET /api/admin/llm-gateway/health
        // Returns health status of all providers
        group.MapGet("/health", async (ILlmGateway gateway, CancellationToken ct) =>
        {
            var report = await gateway.GetHealthAsync(ct);
            return Results.Ok(new
            {
                report.CheckedAt,
                report.AllHealthy,
                Providers = report.ProviderHealth.ToDictionary(
                    kv => kv.Key.ToString(),
                    kv => kv.Value ? "Healthy" : "Unhealthy")
            });
        })
        .WithName("GetLlmProviderHealth")
        .WithDescription("Check health status of all LLM providers (Claude, Gemini, Llama, Redis Cache)");

        // GET /api/admin/llm-gateway/config
        // Returns current gateway configuration (redacted secrets)
        group.MapGet("/config", (IOptions<LlmGatewayOptions> options) =>
        {
            var opts = options.Value;
            return Results.Ok(new
            {
                opts.ProviderTimeout,
                opts.TotalTimeout,
                opts.EnableCacheFallback,
                opts.CacheTtl,
                opts.ForceDegradedMode,
                Claude = new
                {
                    opts.Claude.Enabled,
                    opts.Claude.Model,
                    opts.Claude.BaseUrl,
                    ApiKey = MaskSecret(opts.Claude.ApiKey),
                    opts.Claude.EnablePromptCaching
                },
                Gemini = new
                {
                    opts.Gemini.Enabled,
                    opts.Gemini.Model,
                    opts.Gemini.BaseUrl,
                    ApiKey = MaskSecret(opts.Gemini.ApiKey)
                },
                Llama = new
                {
                    opts.Llama.Enabled,
                    opts.Llama.Model,
                    opts.Llama.BaseUrl,
                    ApiKey = MaskSecret(opts.Llama.ApiKey ?? "")
                }
            });
        })
        .WithName("GetLlmGatewayConfig")
        .WithDescription("Get current LLM Gateway configuration with redacted API keys");

        // POST /api/admin/llm-gateway/test-fallback
        // Send a test request with forced provider to verify cascade
        group.MapPost("/test-fallback", async (
            ILlmGateway gateway,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var forceHeader = ctx.Request.Headers["X-Test-Force-Fallback"].FirstOrDefault();
            var request = new LlmRequest
            {
                UserMessage = "Test cascade fallback — respond with 'OK'",
                MaxTokens = 10,
                Temperature = 0,
                CallerAgent = "AdminDashboard-TestFallback",
                SkipCache = true
            };

            var response = await gateway.CompleteAsync(request, ct);
            return Results.Ok(new
            {
                response.Content,
                Provider = response.Provider.ToString(),
                response.ModelId,
                response.FallbackLevel,
                TotalLatencyMs = response.TotalLatency.TotalMilliseconds,
                ProviderLatencyMs = response.ProviderLatency.TotalMilliseconds,
                response.FromCache,
                response.InputTokens,
                response.OutputTokens
            });
        })
        .WithName("TestLlmFallback")
        .WithDescription("Send a test request to verify the cascade fallback chain");

        // GET /api/admin/llm-gateway/cost
        // Returns current month cost breakdown with threshold status
        group.MapGet("/cost", async (HttpContext ctx, CancellationToken ct) =>
        {
            var costTracker = ctx.RequestServices.GetService<IApiCostTracker>();
            if (costTracker == null)
                return Results.Ok(new { Error = "Cost tracking not configured", Enabled = false });

            var breakdown = await costTracker.GetCostBreakdownAsync(ct);
            return Results.Ok(new
            {
                breakdown.Month,
                MonthlyTotalUsd = Math.Round(breakdown.MonthlyTotalUsd, 2),
                DailyTotalUsd = Math.Round(breakdown.DailyTotalUsd, 2),
                ProjectedMonthlyUsd = Math.Round(breakdown.ProjectedMonthlyUsd, 2),
                breakdown.Thresholds,
                breakdown.IsAggressiveCacheModeActive,
                Status = GetCostStatus(breakdown),
                breakdown.ByProvider,
                breakdown.ByAgent,
                breakdown.GeneratedAt
            });
        })
        .WithName("GetLlmCostBreakdown")
        .WithDescription("Get current month API cost breakdown with threshold status and per-provider/agent breakdown");

        // POST /api/admin/llm-gateway/cost/aggressive-mode
        // Toggle aggressive cache mode manually (CTO override)
        group.MapPost("/cost/aggressive-mode", async (
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var costTracker = ctx.RequestServices.GetService<IApiCostTracker>();
            if (costTracker == null)
                return Results.BadRequest(new { Error = "Cost tracking not configured" });

            var body = await ctx.Request.ReadFromJsonAsync<AggressiveModeToggle>(ct);
            var active = body?.Active ?? false;
            await costTracker.SetAggressiveCacheModeAsync(active, ct);

            return Results.Ok(new
            {
                AggressiveCacheModeActive = active,
                Message = active
                    ? "Aggressive cache mode ACTIVATED — traffic split active"
                    : "Aggressive cache mode DEACTIVATED — normal cascade restored"
            });
        })
        .WithName("ToggleAggressiveCacheMode")
        .WithDescription("Manually toggle aggressive cache mode (CTO override). Splits traffic: 40% Claude, 40% Gemini, 20% Llama");

        // GET /api/admin/llm-gateway/cost/history?days=30
        // Returns daily cost history for trending chart
        group.MapGet("/cost/history", async (
            HttpContext ctx,
            CancellationToken ct,
            int days = 30) =>
        {
            var costTracker = ctx.RequestServices.GetService<IApiCostTracker>();
            if (costTracker == null)
                return Results.Ok(new { Error = "Cost tracking not configured", Entries = Array.Empty<object>() });

            var entries = await costTracker.GetDailyCostHistoryAsync(days, ct);
            return Results.Ok(new
            {
                Days = days,
                Entries = entries.Select(e => new { e.Date, CostUsd = Math.Round(e.CostUsd, 4) }),
                TotalUsd = Math.Round(entries.Sum(e => e.CostUsd), 2),
                AverageDailyUsd = entries.Count > 0 ? Math.Round(entries.Average(e => e.CostUsd), 2) : 0m
            });
        })
        .WithName("GetLlmCostHistory")
        .WithDescription("Get daily cost history for the last N days for trending charts");

        return app;
    }

    private static string GetCostStatus(CostBreakdown breakdown)
    {
        if (breakdown.IsAggressiveCacheModeActive)
            return $"🚨 AGGRESSIVE_CACHE_MODE (${breakdown.MonthlyTotalUsd:F2} ≥ ${breakdown.Thresholds.AggressiveCacheUsd})";

        if (breakdown.MonthlyTotalUsd >= breakdown.Thresholds.CriticalUsd)
            return $"🔴 CRITICAL (${breakdown.MonthlyTotalUsd:F2} ≥ ${breakdown.Thresholds.CriticalUsd})";

        if (breakdown.MonthlyTotalUsd >= breakdown.Thresholds.WarningUsd)
            return $"⚠️ WARNING (${breakdown.MonthlyTotalUsd:F2} ≥ ${breakdown.Thresholds.WarningUsd})";

        return $"✅ NORMAL (${breakdown.MonthlyTotalUsd:F2} < ${breakdown.Thresholds.WarningUsd})";
    }

    private sealed class AggressiveModeToggle
    {
        public bool Active { get; set; }
    }

    private static string MaskSecret(string secret)
    {
        if (string.IsNullOrEmpty(secret)) return "(not set)";
        if (secret.Length <= 8) return "****";
        return $"{secret[..4]}...{secret[^4..]}";
    }
}
