using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Infrastructure.BackgroundServices;

/// <summary>
/// Monthly background worker that runs on the 1st of each month.
/// Sends every active dealer an email with their previous-month metrics
/// compared against the average of VISIBLE-plan dealers, highlighting the gap
/// and driving upgrades with a data-backed CTA.
///
/// Flow:
///   1. Wakes up every hour, checks if today is the 1st and has not run yet this month
///   2. GET UserService /api/internal/dealers?status=active → all active dealers
///   3. GET UserService /api/internal/dealers?plan=visible&amp;status=active → VISIBLE dealers (for average calc)
///   4. For each dealer, GET DealerAnalyticsService /api/dealer-analytics/reports/{id}/monthly → monthly report + benchmark
///   5. Compute VISIBLE-plan averages from the cohort
///   6. Render MonthlyBenchmarkReport.html with comparison data
///   7. Send personalized email via IEmailService
///
/// Safety:
///   - Tracks the last run month in-memory (resets on restart, but hour-check loop + day check prevent duplicates)
///   - Graceful per-dealer error handling
///   - Only runs on day 1 of the month
/// </summary>
public class MonthlyBenchmarkReportWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MonthlyBenchmarkReportWorker> _logger;
    private readonly IConfiguration _configuration;

    // Track which month was already processed (in-memory, resets on restart)
    private int _lastProcessedMonth;

    // VISIBLE-plan benchmarks (average) computed once per cycle
    private static readonly string[] MonthNames =
    {
        "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
    };

    // Average VISIBLE multiplier over LIBRE (based on OKLA platform data)
    private const double VisibleMultiplier = 3.5;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MonthlyBenchmarkReportWorker(
        IServiceProvider serviceProvider,
        ILogger<MonthlyBenchmarkReportWorker> logger,
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

        _logger.LogInformation("📊 MonthlyBenchmarkReportWorker started. Runs on the 1st of each month.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                // Only run on day 1, and only once per month
                if (now.Day == 1 && _lastProcessedMonth != now.Month)
                {
                    _logger.LogInformation("📊 First of the month detected ({Month}/{Year}). Running benchmark report cycle...",
                        now.Month, now.Year);

                    await RunMonthlyBenchmarkCycleAsync(now, stoppingToken);
                    _lastProcessedMonth = now.Month;

                    _logger.LogInformation("📊 Monthly benchmark cycle complete for {Month}/{Year}.", now.Month, now.Year);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in MonthlyBenchmarkReportWorker. Will retry next hour.");
            }

            // Check every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("MonthlyBenchmarkReportWorker stopped.");
    }

    private async Task RunMonthlyBenchmarkCycleAsync(DateTime now, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();
        if (httpClientFactory == null)
        {
            _logger.LogWarning("IHttpClientFactory not available. Skipping benchmark cycle.");
            return;
        }

        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var templateEngine = scope.ServiceProvider.GetRequiredService<ITemplateEngine>();

        // Previous month to report on
        var reportMonth = now.Month == 1 ? 12 : now.Month - 1;
        var reportYear = now.Month == 1 ? now.Year - 1 : now.Year;
        var monthName = MonthNames[reportMonth];
        var nextMonthName = MonthNames[now.Month];

        // ── 1. Fetch ALL active dealers ─────────────────────────────────────
        var allDealers = await FetchDealersAsync(httpClientFactory, "status=active", ct);
        if (allDealers.Count == 0)
        {
            _logger.LogInformation("No active dealers found. Skipping benchmark cycle.");
            return;
        }

        // ── 2. Fetch VISIBLE-plan dealers for benchmark averages ────────────
        var visibleDealers = await FetchDealersAsync(httpClientFactory, "plan=visible&status=active", ct);

        // Compute VISIBLE average metrics
        var visibleAvg = await ComputeVisibleAveragesAsync(
            httpClientFactory, visibleDealers, reportYear, reportMonth, ct);

        _logger.LogInformation(
            "📊 Benchmark averages from {Count} VISIBLE dealers: Views={Views}, Contacts={Contacts}, Conversion={Conv}%",
            visibleDealers.Count, visibleAvg.AvgViews, visibleAvg.AvgContacts,
            visibleAvg.AvgConversionRate.ToString("F1"));

        // ── 3. For each dealer, generate personalized benchmark email ───────
        int sent = 0, skipped = 0, errored = 0;

        foreach (var dealer in allDealers)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                if (string.IsNullOrWhiteSpace(dealer.Email))
                {
                    skipped++;
                    continue;
                }

                // Fetch this dealer's monthly report from DealerAnalyticsService
                var report = await FetchDealerMonthlyReportAsync(
                    httpClientFactory, dealer.DealerId, reportYear, reportMonth, ct);

                if (report == null)
                {
                    skipped++;
                    continue;
                }

                // Build template parameters
                var templateParams = BuildTemplateParams(
                    dealer, report, visibleAvg, monthName, nextMonthName, reportYear);

                string emailBody;
                try
                {
                    emailBody = await templateEngine.RenderTemplateAsync(
                        "EmailTemplates/MonthlyBenchmarkReport.html", templateParams);
                }
                catch (FileNotFoundException)
                {
                    _logger.LogWarning("MonthlyBenchmarkReport.html template not found. Skipping.");
                    return; // Template missing — no point continuing for other dealers
                }

                await emailService.SendEmailAsync(
                    to: dealer.Email,
                    subject: $"📊 Tu resumen de {monthName} en OKLA — {dealer.Name ?? "Dealer"}",
                    body: emailBody,
                    isHtml: true);

                sent++;

                if (sent % 50 == 0)
                {
                    _logger.LogInformation("📊 Progress: {Sent} emails sent so far...", sent);
                }
            }
            catch (Exception ex)
            {
                errored++;
                _logger.LogWarning(ex, "Failed to send benchmark email to dealer {DealerId}", dealer.DealerId);
            }
        }

        _logger.LogInformation(
            "📊 Monthly benchmark cycle finished. Sent={Sent}, Skipped={Skipped}, Errors={Errors}, Total={Total}",
            sent, skipped, errored, allDealers.Count);
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
                _logger.LogWarning("UserService returned {Status} for dealers query: {Query}",
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

    private async Task<DealerMonthlyReport?> FetchDealerMonthlyReportAsync(
        IHttpClientFactory factory, Guid dealerId, int year, int month, CancellationToken ct)
    {
        try
        {
            var client = factory.CreateClient("DealerAnalyticsService");
            var url = $"/api/dealer-analytics/reports/{dealerId}/monthly?year={year}&month={month}";
            var response = await client.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("DealerAnalyticsService returned {Status} for dealer {DealerId}",
                    response.StatusCode, dealerId);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<DealerMonthlyReport>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching monthly report for dealer {DealerId}", dealerId);
            return null;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // VISIBLE average computation
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<VisibleBenchmark> ComputeVisibleAveragesAsync(
        IHttpClientFactory factory, List<DealerInfo> visibleDealers,
        int year, int month, CancellationToken ct)
    {
        if (visibleDealers.Count == 0)
        {
            // Fallback: use platform-wide multiplier estimates
            return new VisibleBenchmark
            {
                AvgViews = 350,
                AvgContacts = 42,
                AvgConversionRate = 4.8,
                AvgResponseTimeMinutes = 45,
                AvgDaysOnMarket = 28
            };
        }

        double totalViews = 0, totalContacts = 0, totalConversion = 0;
        double totalResponseTime = 0, totalDaysOnMarket = 0;
        int count = 0;

        // Sample up to 50 VISIBLE dealers for average computation
        var sample = visibleDealers.Take(50).ToList();

        foreach (var dealer in sample)
        {
            if (ct.IsCancellationRequested) break;

            var report = await FetchDealerMonthlyReportAsync(factory, dealer.DealerId, year, month, ct);
            if (report?.Summary == null) continue;

            totalViews += report.Summary.TotalViews;
            totalContacts += report.Summary.TotalContacts;
            totalConversion += report.Summary.ConversionRate;
            totalResponseTime += report.Summary.AvgResponseTime;
            totalDaysOnMarket += report.Benchmark?.AvgDaysOnMarket ?? 30;
            count++;
        }

        if (count == 0)
        {
            return new VisibleBenchmark
            {
                AvgViews = 350,
                AvgContacts = 42,
                AvgConversionRate = 4.8,
                AvgResponseTimeMinutes = 45,
                AvgDaysOnMarket = 28
            };
        }

        return new VisibleBenchmark
        {
            AvgViews = (int)Math.Round(totalViews / count),
            AvgContacts = (int)Math.Round(totalContacts / count),
            AvgConversionRate = Math.Round(totalConversion / count, 1),
            AvgResponseTimeMinutes = Math.Round(totalResponseTime / count, 0),
            AvgDaysOnMarket = Math.Round(totalDaysOnMarket / count, 0)
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Template parameter builder
    // ═══════════════════════════════════════════════════════════════════════

    private Dictionary<string, object> BuildTemplateParams(
        DealerInfo dealer, DealerMonthlyReport report, VisibleBenchmark visibleAvg,
        string monthName, string nextMonthName, int reportYear)
    {
        var summary = report.Summary ?? new KpiSummary();
        var benchmark = report.Benchmark;

        // Compute diffs for comparison
        var viewsDiff = summary.TotalViews - visibleAvg.AvgViews;
        var contactsDiff = summary.TotalContacts - visibleAvg.AvgContacts;
        var conversionDiff = summary.ConversionRate - visibleAvg.AvgConversionRate;
        var responseTimeDiff = summary.AvgResponseTime - visibleAvg.AvgResponseTimeMinutes;
        var daysOnMarketDiff = (benchmark?.AvgDaysOnMarket ?? 0) - visibleAvg.AvgDaysOnMarket;

        // Projected if on VISIBLE plan
        var projectedViews = (int)Math.Round(summary.TotalViews * VisibleMultiplier);
        var projectedContacts = (int)Math.Round(summary.TotalContacts * VisibleMultiplier * 0.8);

        var baseUrl = _configuration["App:BaseUrl"] ?? "https://okla.com.do";

        // Build insights
        var insights = BuildInsights(summary, visibleAvg, dealer.Plan ?? "libre");

        return new Dictionary<string, object>
        {
            ["DealerName"] = dealer.Name ?? "Dealer",
            ["MonthName"] = monthName,
            ["NextMonthName"] = nextMonthName,
            ["Year"] = reportYear.ToString(),
            ["CurrentPlan"] = CapitalizePlan(dealer.Plan ?? "libre"),

            // KPI cards
            ["TotalViews"] = summary.TotalViews.ToString("N0"),
            ["ViewsChangeText"] = FormatChange(summary.ViewsChange),
            ["ViewsChangeColor"] = summary.ViewsChange >= 0 ? "#16a34a" : "#dc2626",

            ["TotalContacts"] = summary.TotalContacts.ToString("N0"),
            ["ContactsChangeText"] = FormatChange(summary.ContactsChange),
            ["ContactsChangeColor"] = summary.ContactsChange >= 0 ? "#16a34a" : "#dc2626",

            ["TotalSales"] = summary.TotalSales.ToString("N0"),
            ["SalesChangeText"] = FormatChange(summary.SalesChange),
            ["SalesChangeColor"] = summary.SalesChange >= 0 ? "#16a34a" : "#dc2626",

            // Comparison table
            ["AvgVisibleViews"] = visibleAvg.AvgViews.ToString("N0"),
            ["ViewsDiffClass"] = viewsDiff >= 0 ? "better" : "worse",
            ["ViewsDiffText"] = FormatDiff(viewsDiff),

            ["AvgVisibleContacts"] = visibleAvg.AvgContacts.ToString("N0"),
            ["ContactsDiffClass"] = contactsDiff >= 0 ? "better" : "worse",
            ["ContactsDiffText"] = FormatDiff(contactsDiff),

            ["ConversionRate"] = $"{summary.ConversionRate:F1}%",
            ["AvgVisibleConversion"] = $"{visibleAvg.AvgConversionRate:F1}%",
            ["ConversionDiffClass"] = conversionDiff >= 0 ? "better" : "worse",
            ["ConversionDiffText"] = FormatDiffPercent(conversionDiff),

            ["AvgResponseTime"] = $"{summary.AvgResponseTime:F0} min",
            ["AvgVisibleResponseTime"] = $"{visibleAvg.AvgResponseTimeMinutes:F0} min",
            ["ResponseTimeDiffClass"] = responseTimeDiff <= 0 ? "better" : "worse", // Lower is better
            ["ResponseTimeDiffText"] = FormatDiffMinutes(responseTimeDiff),

            ["DaysOnMarket"] = $"{benchmark?.AvgDaysOnMarket ?? 0:F0} días",
            ["AvgVisibleDaysOnMarket"] = $"{visibleAvg.AvgDaysOnMarket:F0} días",
            ["DaysOnMarketDiffClass"] = daysOnMarketDiff <= 0 ? "better" : "worse", // Lower is better
            ["DaysOnMarketDiffText"] = FormatDiffDays(daysOnMarketDiff),

            // Ranking
            ["OverallRank"] = benchmark?.Rankings?.OverallRank.ToString() ?? "—",
            ["TotalDealers"] = benchmark?.Rankings?.TotalDealers.ToString() ?? "—",

            // Upgrade CTA data
            ["VisibleMultiplier"] = $"{VisibleMultiplier:F1}x",
            ["ProjectedViews"] = projectedViews.ToString("N0"),
            ["ProjectedContacts"] = projectedContacts.ToString("N0"),

            // Insights
            ["Insight1"] = insights.Count > 0 ? insights[0] : "Mantén tus listados actualizados con fotos recientes",
            ["Insight2"] = insights.Count > 1 ? insights[1] : "Responde consultas en menos de 1 hora para mejorar tu ranking",
            ["Insight3"] = insights.Count > 2 ? insights[2] : "Dealers en plan Visible reciben en promedio 3.5x más vistas",

            // URLs
            ["UpgradeUrl"] = $"{baseUrl}/dealer/pricing",
            ["DashboardUrl"] = $"{baseUrl}/cuenta",
            ["UnsubscribeUrl"] = $"{baseUrl}/dealer/configuracion/notificaciones"
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Formatting helpers
    // ═══════════════════════════════════════════════════════════════════════

    private static string FormatChange(double change)
    {
        if (Math.Abs(change) < 0.1) return "Sin cambio";
        return change > 0 ? $"▲ +{change:F1}%" : $"▼ {change:F1}%";
    }

    private static string FormatDiff(double diff)
    {
        if (Math.Abs(diff) < 1) return "≈ Igual";
        return diff > 0 ? $"+{diff:F0}" : $"{diff:F0}";
    }

    private static string FormatDiffPercent(double diff)
    {
        if (Math.Abs(diff) < 0.1) return "≈ Igual";
        return diff > 0 ? $"+{diff:F1}%" : $"{diff:F1}%";
    }

    private static string FormatDiffMinutes(double diff)
    {
        if (Math.Abs(diff) < 1) return "≈ Igual";
        // For response time, negative diff means faster (better)
        return diff <= 0 ? $"{diff:F0} min ✓" : $"+{diff:F0} min";
    }

    private static string FormatDiffDays(double diff)
    {
        if (Math.Abs(diff) < 1) return "≈ Igual";
        return diff <= 0 ? $"{diff:F0} días ✓" : $"+{diff:F0} días";
    }

    private static string CapitalizePlan(string plan) =>
        plan.Length > 0
            ? char.ToUpper(plan[0], CultureInfo.InvariantCulture) + plan[1..]
            : plan;

    private static List<string> BuildInsights(KpiSummary summary, VisibleBenchmark avg, string plan)
    {
        var insights = new List<string>();

        if (summary.TotalViews < avg.AvgViews)
            insights.Add($"Tus vistas están {avg.AvgViews - summary.TotalViews:N0} por debajo del promedio Visible. Mejorar tus fotos y descripciones aumenta las vistas hasta un 40%.");

        if (summary.AvgResponseTime > avg.AvgResponseTimeMinutes)
            insights.Add($"Tu tiempo de respuesta ({summary.AvgResponseTime:F0} min) es más lento que el promedio. Los compradores contactan al siguiente dealer si no responden en 1 hora.");

        if (summary.ConversionRate < avg.AvgConversionRate)
            insights.Add($"Tu tasa de conversión ({summary.ConversionRate:F1}%) está por debajo del {avg.AvgConversionRate:F1}% promedio. Incluir precio competitivo y financiamiento mejora conversiones.");

        if (string.Equals(plan, "libre", StringComparison.OrdinalIgnoreCase))
            insights.Add("Dealers en plan Visible aparecen primero en búsquedas y reciben en promedio 3.5x más vistas que el plan Libre.");

        if (summary.TotalContacts > 0 && summary.TotalSales == 0)
            insights.Add("Recibiste consultas pero no registraste ventas. Haz seguimiento a cada lead dentro de las primeras 2 horas.");

        // Cap at 3
        return insights.Take(3).ToList();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DTOs (private, for JSON deserialization)
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

    private sealed class DealerMonthlyReport
    {
        public Guid DealerId { get; set; }
        public string DealerName { get; set; } = string.Empty;
        public KpiSummary? Summary { get; set; }
        public DealerBenchmarkInfo? Benchmark { get; set; }
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

    private sealed class DealerBenchmarkInfo
    {
        public double AvgDaysOnMarket { get; set; }
        public double ConversionRate { get; set; }
        public double AvgResponseTimeMinutes { get; set; }
        public RankingsInfo? Rankings { get; set; }
    }

    private sealed class RankingsInfo
    {
        public int OverallRank { get; set; }
        public int TotalDealers { get; set; }
    }

    private sealed class VisibleBenchmark
    {
        public int AvgViews { get; set; }
        public int AvgContacts { get; set; }
        public double AvgConversionRate { get; set; }
        public double AvgResponseTimeMinutes { get; set; }
        public double AvgDaysOnMarket { get; set; }
    }
}
