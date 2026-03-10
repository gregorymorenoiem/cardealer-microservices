using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.UseCases.SendWhatsAppNotification;

namespace NotificationService.Infrastructure.BackgroundServices;

/// <summary>
/// Weekly background worker that runs every Monday morning (DR time, UTC-4).
/// For each active dealer with WhatsApp-enabled plan (Pro/Elite), it:
///   1. Fetches the previous week's analytics from DealerAnalyticsService
///   2. Sends the metrics to AnalyticsAgent for LLM-powered insight generation
///   3. Picks the single most actionable recommendation
///   4. Delivers it via WhatsApp template through the notification pipeline
///
/// Example output:
///   "Tu Honda Civic 2018 tiene 80 vistas pero 0 consultas.
///    Prueba bajar el precio $500 o agrega 5 fotos del interior."
///
/// Safety:
///   - Tracks which ISO week was already processed (prevents duplicate sends on restart)
///   - Per-dealer try/catch (one failure doesn't block others)
///   - Respects Ley 172-13 consent gate in SendWhatsAppNotificationCommand pipeline
///   - Only targets Pro/Elite plans (WhatsApp-enabled per PlanConfiguration)
///   - Rate-limited: 50ms delay between dealers to avoid Meta API throttling
/// </summary>
public class WeeklyRecommendationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WeeklyRecommendationWorker> _logger;
    private readonly IConfiguration _configuration;

    // Track which ISO week number was already processed (prevents duplicate sends)
    private int _lastProcessedWeek;
    private int _lastProcessedYear;

    // Dominican Republic timezone (UTC-4, no DST)
    private static readonly TimeZoneInfo DrTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Hour of the day (DR time) to trigger the recommendation cycle.
    /// Default 8 AM — dealers see it when they start their day.
    /// </summary>
    private int TriggerHour => _configuration.GetValue("WeeklyRecommendation:TriggerHourDR", 8);

    public WeeklyRecommendationWorker(
        IServiceProvider serviceProvider,
        ILogger<WeeklyRecommendationWorker> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial delay: wait for other services to stabilize
        await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);

        _logger.LogInformation(
            "📬 WeeklyRecommendationWorker started. Sends every Monday at {Hour}:00 DR time.",
            TriggerHour);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var drNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, DrTimeZone);
                var isoWeek = System.Globalization.ISOWeek.GetWeekOfYear(drNow);
                var isoYear = System.Globalization.ISOWeek.GetYear(drNow);

                // Only run on Monday, at or past the trigger hour, and once per ISO week
                if (drNow.DayOfWeek == DayOfWeek.Monday
                    && drNow.Hour >= TriggerHour
                    && !(_lastProcessedYear == isoYear && _lastProcessedWeek == isoWeek))
                {
                    _logger.LogInformation(
                        "📬 Monday detected (ISO week {Week}/{Year}, DR time {DrNow:HH:mm}). Running weekly recommendation cycle...",
                        isoWeek, isoYear, drNow);

                    await RunWeeklyRecommendationCycleAsync(drNow, stoppingToken);

                    _lastProcessedYear = isoYear;
                    _lastProcessedWeek = isoWeek;

                    _logger.LogInformation(
                        "📬 Weekly recommendation cycle complete for ISO week {Week}/{Year}.",
                        isoWeek, isoYear);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled error in WeeklyRecommendationWorker. Will retry next hour.");
            }

            // Check every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("WeeklyRecommendationWorker stopped.");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Main cycle
    // ═══════════════════════════════════════════════════════════════════════

    private async Task RunWeeklyRecommendationCycleAsync(DateTime drNow, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();
        if (httpClientFactory == null)
        {
            _logger.LogWarning("IHttpClientFactory not available. Skipping recommendation cycle.");
            return;
        }

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        // ── 1. Fetch dealers with WhatsApp-enabled plans (Pro/Elite) ────────
        var dealers = new List<DealerInfo>();

        var proDealers = await FetchDealersAsync(httpClientFactory, "plan=pro&status=active", ct);
        var eliteDealers = await FetchDealersAsync(httpClientFactory, "plan=elite&status=active", ct);
        dealers.AddRange(proDealers);
        dealers.AddRange(eliteDealers);

        if (dealers.Count == 0)
        {
            _logger.LogInformation("📬 No Pro/Elite dealers found. Skipping recommendation cycle.");
            return;
        }

        _logger.LogInformation(
            "📬 Found {Count} WhatsApp-eligible dealers (Pro={Pro}, Elite={Elite})",
            dealers.Count, proDealers.Count, eliteDealers.Count);

        // ── 2. Process each dealer ──────────────────────────────────────────
        int sent = 0, skipped = 0, errored = 0;

        foreach (var dealer in dealers)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                // Must have a phone number for WhatsApp
                if (string.IsNullOrWhiteSpace(dealer.Phone))
                {
                    skipped++;
                    _logger.LogDebug(
                        "Dealer {DealerId} ({Name}) has no phone. Skipping.",
                        dealer.DealerId, dealer.Name);
                    continue;
                }

                // ── 2a. Fetch weekly report from DealerAnalyticsService ─────
                var weeklyReport = await FetchWeeklyReportAsync(
                    httpClientFactory, dealer.DealerId, drNow, ct);

                if (weeklyReport == null)
                {
                    skipped++;
                    _logger.LogDebug(
                        "No weekly report for dealer {DealerId}. Skipping.", dealer.DealerId);
                    continue;
                }

                // ── 2b. Build metrics and call AnalyticsAgent ───────────────
                var recommendation = await GenerateRecommendationAsync(
                    httpClientFactory, dealer, weeklyReport, ct);

                if (string.IsNullOrWhiteSpace(recommendation))
                {
                    skipped++;
                    _logger.LogDebug(
                        "AnalyticsAgent returned no recommendation for dealer {DealerId}.",
                        dealer.DealerId);
                    continue;
                }

                // ── 2c. Send via WhatsApp ───────────────────────────────────
                var waResult = await mediator.Send(new SendWhatsAppNotificationCommand(
                    To: dealer.Phone,
                    TemplateName: "weekly_recommendation",
                    TemplateParameters: new Dictionary<string, string>
                    {
                        ["dealer_name"] = dealer.Name ?? "Dealer",
                        ["recommendation"] = recommendation
                    },
                    LanguageCode: "es",
                    IsMarketing: true,
                    RecipientUserId: dealer.DealerId != Guid.Empty ? dealer.DealerId : null,
                    Metadata: new Dictionary<string, object>
                    {
                        ["source"] = "WeeklyRecommendationWorker",
                        ["iso_week"] = System.Globalization.ISOWeek.GetWeekOfYear(drNow),
                        ["dealer_plan"] = dealer.Plan ?? "unknown"
                    }
                ), ct);

                if (waResult.Success)
                {
                    sent++;
                    _logger.LogInformation(
                        "✅ Weekly recommendation sent to {Name} ({Phone}): {Rec}",
                        dealer.Name, MaskPhone(dealer.Phone), Truncate(recommendation, 80));
                }
                else
                {
                    errored++;
                    _logger.LogWarning(
                        "❌ WhatsApp send failed for dealer {DealerId}: {Error}",
                        dealer.DealerId, waResult.Error);
                }

                // Rate limit: 50ms between sends to avoid Meta API throttling
                await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
            }
            catch (Exception ex)
            {
                errored++;
                _logger.LogWarning(ex,
                    "Error processing weekly recommendation for dealer {DealerId}. Continuing.",
                    dealer.DealerId);
            }
        }

        _logger.LogInformation(
            "📬 Weekly recommendation cycle finished. Sent={Sent}, Skipped={Skipped}, Errors={Errors}, Total={Total}",
            sent, skipped, errored, dealers.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // AnalyticsAgent integration
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<string?> GenerateRecommendationAsync(
        IHttpClientFactory factory,
        DealerInfo dealer,
        WeeklyReportData report,
        CancellationToken ct)
    {
        try
        {
            var client = factory.CreateClient("AnalyticsAgentService");

            // Build metrics from the weekly report — include vehicle-level detail
            var metrics = BuildMetricsPayload(dealer, report);

            var requestBody = new
            {
                reportType = "dealer_performance",
                period = "week",
                dealerId = dealer.DealerId.ToString(),
                metrics
            };

            var json = JsonSerializer.Serialize(requestBody, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/analytics-agent/insights", content, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "AnalyticsAgent returned {Status} for dealer {DealerId}",
                    response.StatusCode, dealer.DealerId);
                return BuildFallbackRecommendation(dealer, report);
            }

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            var insights = JsonSerializer.Deserialize<AnalyticsAgentResponse>(responseJson, JsonOptions);

            if (insights?.Recomendaciones == null || insights.Recomendaciones.Count == 0)
            {
                return BuildFallbackRecommendation(dealer, report);
            }

            // Pick the highest-priority recommendation
            var topRec = insights.Recomendaciones
                .OrderByDescending(r => r.Prioridad == "alta" ? 3 : r.Prioridad == "media" ? 2 : 1)
                .First();

            // Format: "Titulo — Descripcion"
            var text = !string.IsNullOrWhiteSpace(topRec.Titulo)
                ? $"{topRec.Titulo} — {topRec.Descripcion}"
                : topRec.Descripcion;

            return Truncate(text ?? string.Empty, 500);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "AnalyticsAgent call failed for dealer {DealerId}. Using fallback.",
                dealer.DealerId);
            return BuildFallbackRecommendation(dealer, report);
        }
    }

    private static Dictionary<string, object> BuildMetricsPayload(
        DealerInfo dealer, WeeklyReportData report)
    {
        var metrics = new Dictionary<string, object>
        {
            ["total_views"] = report.Summary?.TotalViews ?? 0,
            ["views_change_pct"] = report.Summary?.ViewsChange ?? 0,
            ["total_contacts"] = report.Summary?.TotalContacts ?? 0,
            ["contacts_change_pct"] = report.Summary?.ContactsChange ?? 0,
            ["total_leads"] = report.Summary?.TotalLeads ?? 0,
            ["total_sales"] = report.Summary?.TotalSales ?? 0,
            ["conversion_rate"] = report.Summary?.ConversionRate ?? 0,
            ["avg_response_time_min"] = report.Summary?.AvgResponseTime ?? 0,
            ["active_listings"] = report.Summary?.ActiveListings ?? 0,
            ["dealer_name"] = dealer.Name ?? "Dealer",
            ["dealer_plan"] = dealer.Plan ?? "unknown"
        };

        // Include top 3 vehicles by views for vehicle-specific recommendations
        if (report.VehiclePerformance is { Count: > 0 })
        {
            var topVehicles = report.VehiclePerformance
                .OrderByDescending(v => v.Views)
                .Take(3)
                .Select(v => new Dictionary<string, object>
                {
                    ["title"] = v.VehicleTitle ?? $"{v.VehicleMake} {v.VehicleModel} {v.VehicleYear}",
                    ["views"] = v.Views,
                    ["contacts"] = v.Contacts,
                    ["price"] = v.VehiclePrice,
                    ["days_on_market"] = v.DaysOnMarket,
                    ["contact_rate"] = v.ContactRate,
                    ["ctr"] = v.ClickThroughRate
                })
                .ToList();

            metrics["top_vehicles"] = topVehicles;
        }

        // Include bottom 3 underperformers
        if (report.VehiclePerformance is { Count: > 3 })
        {
            var bottomVehicles = report.VehiclePerformance
                .Where(v => !v.IsSold)
                .OrderBy(v => v.EngagementScore)
                .Take(3)
                .Select(v => new Dictionary<string, object>
                {
                    ["title"] = v.VehicleTitle ?? $"{v.VehicleMake} {v.VehicleModel} {v.VehicleYear}",
                    ["views"] = v.Views,
                    ["contacts"] = v.Contacts,
                    ["price"] = v.VehiclePrice,
                    ["days_on_market"] = v.DaysOnMarket,
                    ["contact_rate"] = v.ContactRate
                })
                .ToList();

            metrics["underperforming_vehicles"] = bottomVehicles;
        }

        // Funnel data for conversion insights
        if (report.Funnel != null)
        {
            metrics["funnel_impressions"] = report.Funnel.Impressions;
            metrics["funnel_views"] = report.Funnel.Views;
            metrics["funnel_contacts"] = report.Funnel.Contacts;
            metrics["funnel_converted"] = report.Funnel.Converted;
            metrics["overall_conversion"] = report.Funnel.OverallConversion;
        }

        // Benchmark for competitive context
        if (report.Benchmark != null)
        {
            metrics["benchmark_avg_days_on_market"] = report.Benchmark.AvgDaysOnMarket;
            metrics["benchmark_conversion_rate"] = report.Benchmark.ConversionRate;
            metrics["benchmark_avg_response_time"] = report.Benchmark.AvgResponseTimeMinutes;
        }

        return metrics;
    }

    /// <summary>
    /// Rule-based fallback when AnalyticsAgent is unavailable.
    /// Produces a single actionable recommendation in Dominican Spanish.
    /// </summary>
    private static string? BuildFallbackRecommendation(DealerInfo dealer, WeeklyReportData report)
    {
        var summary = report.Summary;
        if (summary == null) return null;

        // Priority 1: High views but zero contacts → pricing or photo problem
        if (summary.TotalViews > 50 && summary.TotalContacts == 0)
        {
            var topVehicle = report.VehiclePerformance?
                .OrderByDescending(v => v.Views)
                .FirstOrDefault();

            if (topVehicle != null)
            {
                var title = topVehicle.VehicleTitle
                    ?? $"{topVehicle.VehicleMake} {topVehicle.VehicleModel} {topVehicle.VehicleYear}";
                return $"Tu {title} tiene {topVehicle.Views} vistas pero 0 consultas esta semana. "
                     + "Prueba bajar el precio un 5% o agrega más fotos del interior — eso aumenta las consultas hasta un 40%.";
            }

            return $"Tus vehículos tuvieron {summary.TotalViews} vistas pero 0 consultas. "
                 + "Revisa que los precios sean competitivos y que cada publicación tenga al menos 8 fotos.";
        }

        // Priority 2: Slow response time → losing leads
        if (summary.AvgResponseTime > 120) // More than 2 hours
        {
            return $"Tu tiempo de respuesta promedio es {summary.AvgResponseTime:F0} minutos. "
                 + "Los compradores contactan al siguiente dealer si no respondes en 1 hora. "
                 + "Activa las notificaciones en tu celular para no perder clientes.";
        }

        // Priority 3: Contacts but no sales → follow-up problem
        if (summary.TotalContacts > 5 && summary.TotalSales == 0)
        {
            return $"Recibiste {summary.TotalContacts} consultas esta semana pero no cerraste ventas. "
                 + "Haz seguimiento a cada interesado dentro de las primeras 2 horas — "
                 + "el 60% de los compradores deciden en las primeras 24 horas.";
        }

        // Priority 4: Views declining
        if (summary.ViewsChange < -20)
        {
            return $"Tus vistas bajaron un {Math.Abs(summary.ViewsChange):F0}% esta semana. "
                 + "Actualiza las fotos y descripciones de tus vehículos — "
                 + "las publicaciones frescas aparecen más arriba en las búsquedas.";
        }

        // Priority 5: Aging inventory
        var agingVehicle = report.VehiclePerformance?
            .Where(v => !v.IsSold && v.DaysOnMarket > 60)
            .OrderByDescending(v => v.DaysOnMarket)
            .FirstOrDefault();

        if (agingVehicle != null)
        {
            var title = agingVehicle.VehicleTitle
                ?? $"{agingVehicle.VehicleMake} {agingVehicle.VehicleModel} {agingVehicle.VehicleYear}";
            return $"Tu {title} lleva {agingVehicle.DaysOnMarket} días publicado sin venderse. "
                 + "Considera hacer un descuento del 5-10% o destacarlo como \"Precio especial\" para atraer compradores.";
        }

        // Priority 6: Positive reinforcement
        if (summary.TotalSales > 0)
        {
            return $"¡Buen trabajo! Cerraste {summary.TotalSales} venta(s) esta semana. "
                 + "Sigue respondiendo rápido y mantén tus precios actualizados para seguir creciendo.";
        }

        // Default
        return "Actualiza las fotos y precios de tus vehículos esta semana. "
             + "Las publicaciones con más de 8 fotos y precio competitivo reciben hasta 3x más consultas.";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HTTP helpers
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<List<DealerInfo>> FetchDealersAsync(
        IHttpClientFactory factory, string queryParams, CancellationToken ct)
    {
        try
        {
            var client = factory.CreateClient("UserService");
            var response = await client.GetAsync($"/api/internal/dealers?{queryParams}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "UserService returned {Status} for dealers query: {Query}",
                    response.StatusCode, queryParams);
                return new List<DealerInfo>();
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<DealerInfo>>(json, JsonOptions)
                   ?? new List<DealerInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dealers from UserService ({Query})", queryParams);
            return new List<DealerInfo>();
        }
    }

    private async Task<WeeklyReportData?> FetchWeeklyReportAsync(
        IHttpClientFactory factory, Guid dealerId, DateTime drNow, CancellationToken ct)
    {
        try
        {
            var client = factory.CreateClient("DealerAnalyticsService");

            // Previous week: Monday-to-Sunday
            var previousMonday = drNow.AddDays(-7).Date;
            var url = $"/api/dealer-analytics/reports/{dealerId}/weekly?startDate={previousMonday:yyyy-MM-dd}";

            var response = await client.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug(
                    "DealerAnalyticsService returned {Status} for dealer {DealerId} weekly report",
                    response.StatusCode, dealerId);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<WeeklyReportData>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching weekly report for dealer {DealerId}", dealerId);
            return null;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Utility methods
    // ═══════════════════════════════════════════════════════════════════════

    private static string MaskPhone(string phone)
    {
        if (phone.Length <= 6) return "***";
        return phone[..3] + "***" + phone[^3..];
    }

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= maxLength ? text : text[..maxLength] + "…";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DTOs (private, for JSON deserialization from inter-service calls)
    // ═══════════════════════════════════════════════════════════════════════

    private sealed class DealerInfo
    {
        public Guid DealerId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Plan { get; set; }
        public int ActiveListings { get; set; }
        public int TotalViews { get; set; }
    }

    private sealed class WeeklyReportData
    {
        public Guid DealerId { get; set; }
        public string DealerName { get; set; } = string.Empty;
        public KpiSummary? Summary { get; set; }
        public List<VehiclePerf> VehiclePerformance { get; set; } = new();
        public FunnelData? Funnel { get; set; }
        public BenchmarkData? Benchmark { get; set; }
        public List<string> KeyInsights { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    private sealed class KpiSummary
    {
        public int TotalViews { get; set; }
        public double ViewsChange { get; set; }
        public int TotalContacts { get; set; }
        public double ContactsChange { get; set; }
        public int TotalLeads { get; set; }
        public double LeadsChange { get; set; }
        public int TotalSales { get; set; }
        public double SalesChange { get; set; }
        public double ConversionRate { get; set; }
        public double AvgResponseTime { get; set; }
        public int ActiveListings { get; set; }
    }

    private sealed class VehiclePerf
    {
        public Guid VehicleId { get; set; }
        public string? VehicleTitle { get; set; }
        public string? VehicleMake { get; set; }
        public string? VehicleModel { get; set; }
        public int VehicleYear { get; set; }
        public decimal VehiclePrice { get; set; }
        public int Views { get; set; }
        public int Contacts { get; set; }
        public double ClickThroughRate { get; set; }
        public double ContactRate { get; set; }
        public double EngagementScore { get; set; }
        public int DaysOnMarket { get; set; }
        public bool IsSold { get; set; }
    }

    private sealed class FunnelData
    {
        public int Impressions { get; set; }
        public int Views { get; set; }
        public int Contacts { get; set; }
        public int Converted { get; set; }
        public double OverallConversion { get; set; }
    }

    private sealed class BenchmarkData
    {
        public double AvgDaysOnMarket { get; set; }
        public double ConversionRate { get; set; }
        public double AvgResponseTimeMinutes { get; set; }
    }

    // ── AnalyticsAgent response DTOs ──

    private sealed class AnalyticsAgentResponse
    {
        public string? Titulo { get; set; }
        public string? ResumenEjecutivo { get; set; }
        public List<AgentRecommendation> Recomendaciones { get; set; } = new();
    }

    private sealed class AgentRecommendation
    {
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? Prioridad { get; set; }
        public string? ImpactoEsperado { get; set; }
    }
}
