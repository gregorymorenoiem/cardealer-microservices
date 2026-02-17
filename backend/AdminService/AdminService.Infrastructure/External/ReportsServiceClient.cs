using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AdminService.Infrastructure.External
{
    /// <summary>
    /// HTTP client implementation for ReportsService content moderation endpoints.
    /// Calls ReportsService API at /api/contentreports/*.
    /// </summary>
    public class ReportsServiceClient : IReportsServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReportsServiceClient> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ReportsServiceClient(HttpClient httpClient, ILogger<ReportsServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ContentReportSearchResponse?> GetReportsAsync(
            ContentReportSearchFilters filters, CancellationToken ct = default)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"page={filters.Page}",
                    $"pageSize={filters.PageSize}"
                };

                if (!string.IsNullOrEmpty(filters.Type))
                    queryParams.Add($"type={Uri.EscapeDataString(filters.Type)}");
                if (!string.IsNullOrEmpty(filters.Status))
                    queryParams.Add($"status={Uri.EscapeDataString(filters.Status)}");
                if (!string.IsNullOrEmpty(filters.Priority))
                    queryParams.Add($"priority={Uri.EscapeDataString(filters.Priority)}");
                if (!string.IsNullOrEmpty(filters.Search))
                    queryParams.Add($"search={Uri.EscapeDataString(filters.Search)}");

                var url = $"/api/contentreports?{string.Join("&", queryParams)}";
                _logger.LogDebug("Fetching content reports from ReportsService: {Url}", url);

                var response = await _httpClient.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "ReportsService returned {StatusCode} for GET {Url}",
                        response.StatusCode, url);
                    return new ContentReportSearchResponse
                    {
                        Items = new List<ContentReportItemDto>(),
                        Total = 0,
                        Page = filters.Page,
                        PageSize = filters.PageSize,
                        TotalPages = 0
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<ContentReportSearchResponse>(JsonOptions, ct);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching content reports from ReportsService");
                return new ContentReportSearchResponse
                {
                    Items = new List<ContentReportItemDto>(),
                    Total = 0,
                    Page = filters.Page,
                    PageSize = filters.PageSize,
                    TotalPages = 0
                };
            }
        }

        public async Task<ContentReportDetailResponse?> GetReportByIdAsync(
            Guid reportId, CancellationToken ct = default)
        {
            try
            {
                var url = $"/api/contentreports/{reportId}";
                _logger.LogDebug("Fetching content report {ReportId} from ReportsService", reportId);

                var response = await _httpClient.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "ReportsService returned {StatusCode} for GET content report {ReportId}",
                        response.StatusCode, reportId);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<ContentReportDetailResponse>(JsonOptions, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching content report {ReportId} from ReportsService", reportId);
                return null;
            }
        }

        public async Task<ContentReportStatsResponse> GetReportStatsAsync(CancellationToken ct = default)
        {
            try
            {
                var url = "/api/contentreports/stats";
                _logger.LogDebug("Fetching content report stats from ReportsService");

                var response = await _httpClient.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "ReportsService returned {StatusCode} for GET content report stats",
                        response.StatusCode);
                    return new ContentReportStatsResponse();
                }

                var result = await response.Content.ReadFromJsonAsync<ContentReportStatsResponse>(JsonOptions, ct);
                return result ?? new ContentReportStatsResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching content report stats from ReportsService");
                return new ContentReportStatsResponse();
            }
        }

        public async Task<bool> UpdateReportStatusAsync(
            Guid reportId, string status, string? resolution = null,
            string? resolvedById = null, CancellationToken ct = default)
        {
            try
            {
                var url = $"/api/contentreports/{reportId}/status";
                _logger.LogDebug(
                    "Updating content report {ReportId} status to {Status} via ReportsService",
                    reportId, status);

                var payload = new { status, resolution, resolvedById };
                var response = await _httpClient.PatchAsJsonAsync(url, payload, JsonOptions, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "ReportsService returned {StatusCode} for PATCH content report {ReportId} status",
                        response.StatusCode, reportId);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating content report {ReportId} status via ReportsService", reportId);
                return false;
            }
        }
    }
}
