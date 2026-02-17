using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AdminService.Infrastructure.External
{
    public class VehicleServiceClient : IVehicleServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VehicleServiceClient> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Vehicle status mapping (matches VehiclesSaleService enum)
        private static readonly Dictionary<int, string> StatusMap = new()
        {
            { 0, "draft" },
            { 1, "pending" },
            { 2, "active" },
            { 3, "reserved" },
            { 4, "sold" },
            { 5, "archived" },
            { 6, "rejected" }
        };

        public VehicleServiceClient(HttpClient httpClient, ILogger<VehicleServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<VehicleSearchResponse?> SearchVehiclesAsync(VehicleSearchFilters filters, CancellationToken ct = default)
        {
            try
            {
                // Build query string - VehiclesSaleService uses different param names
                var queryParams = new List<string>
                {
                    $"Page={filters.Page}",
                    $"PageSize={filters.PageSize}"
                };

                if (!string.IsNullOrWhiteSpace(filters.Search))
                    queryParams.Add($"Search={Uri.EscapeDataString(filters.Search)}");

                var url = $"/api/vehicles?{string.Join("&", queryParams)}";

                _logger.LogDebug("Searching vehicles: {Url}", url);

                var response = await _httpClient.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("VehiclesSaleService search returned {StatusCode}", response.StatusCode);
                    return new VehicleSearchResponse
                    {
                        Vehicles = new List<VehicleDto>(),
                        TotalCount = 0,
                        Page = filters.Page,
                        PageSize = filters.PageSize,
                        TotalPages = 0
                    };
                }

                var content = await response.Content.ReadAsStringAsync(ct);
                var rawResult = JsonSerializer.Deserialize<RawVehicleSearchResult>(content, JsonOptions);

                if (rawResult == null)
                {
                    return new VehicleSearchResponse
                    {
                        Vehicles = new List<VehicleDto>(),
                        TotalCount = 0,
                        Page = filters.Page,
                        PageSize = filters.PageSize,
                        TotalPages = 0
                    };
                }

                // Map raw vehicles to our DTOs and apply admin-specific filters
                var vehicles = (rawResult.Vehicles ?? new List<RawVehicle>())
                    .Select(MapToVehicleDto)
                    .ToList();

                // Apply admin-specific filters (status, sellerType, featured) client-side
                // since VehiclesSaleService search only filters Active vehicles by default
                if (!string.IsNullOrWhiteSpace(filters.Status))
                {
                    vehicles = vehicles.Where(v =>
                        string.Equals(v.StatusName, filters.Status, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(filters.SellerType))
                {
                    vehicles = vehicles.Where(v =>
                        string.Equals(v.SellerType, filters.SellerType, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (filters.Featured.HasValue)
                {
                    vehicles = vehicles.Where(v => v.IsFeatured == filters.Featured.Value).ToList();
                }

                return new VehicleSearchResponse
                {
                    Vehicles = vehicles,
                    TotalCount = rawResult.TotalCount,
                    Page = rawResult.Page,
                    PageSize = rawResult.PageSize,
                    TotalPages = rawResult.TotalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching vehicles from VehiclesSaleService");
                return new VehicleSearchResponse
                {
                    Vehicles = new List<VehicleDto>(),
                    TotalCount = 0,
                    Page = filters.Page,
                    PageSize = filters.PageSize,
                    TotalPages = 0
                };
            }
        }

        public async Task<VehicleDetailResponse?> GetVehicleByIdAsync(Guid vehicleId, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/vehicles/{vehicleId}", ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("VehiclesSaleService GetById returned {StatusCode} for {VehicleId}",
                        response.StatusCode, vehicleId);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(ct);
                var rawVehicle = JsonSerializer.Deserialize<RawVehicle>(content, JsonOptions);

                if (rawVehicle == null) return null;

                var dto = MapToVehicleDto(rawVehicle);
                return new VehicleDetailResponse
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    Make = dto.Make,
                    Model = dto.Model,
                    Year = dto.Year,
                    Price = dto.Price,
                    Currency = dto.Currency,
                    Status = dto.Status,
                    StatusName = dto.StatusName,
                    SellerId = dto.SellerId,
                    SellerName = dto.SellerName,
                    SellerType = dto.SellerType,
                    ViewCount = dto.ViewCount,
                    LeadCount = dto.LeadCount,
                    IsFeatured = dto.IsFeatured,
                    CreatedAt = dto.CreatedAt,
                    PublishedAt = dto.PublishedAt,
                    RejectionReason = dto.RejectionReason,
                    Images = dto.Images
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle {VehicleId} from VehiclesSaleService", vehicleId);
                return null;
            }
        }

        public async Task<VehicleStatsResponse> GetVehicleStatsAsync(CancellationToken ct = default)
        {
            try
            {
                // Fetch all vehicles to compute stats (no status filter = all statuses)
                // We do multiple small requests to get counts per status
                var stats = new VehicleStatsResponse();

                // Get total count by fetching page 1 with pageSize 1
                var allResult = await FetchVehiclesCount(null, ct);
                stats.Total = allResult;

                // Get active count
                var activeResult = await FetchVehiclesByStatus(2, ct); // Active = 2
                stats.Active = activeResult;

                // Get pending count
                var pendingResult = await FetchVehiclesByStatus(1, ct); // PendingReview = 1
                stats.Pending = pendingResult;

                // Get rejected count
                var rejectedResult = await FetchVehiclesByStatus(6, ct); // Rejected = 6
                stats.Rejected = rejectedResult;

                // Featured count - we'll get from the search
                var featuredResult = await FetchFeaturedCount(ct);
                stats.Featured = featuredResult;

                // WithReports - not available directly, default to 0
                stats.WithReports = 0;

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle stats from VehiclesSaleService");
                return new VehicleStatsResponse();
            }
        }

        public async Task<bool> FeatureVehicleAsync(Guid vehicleId, bool featured, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"/api/vehicles/{vehicleId}/feature",
                    new { featured },
                    ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to feature vehicle {VehicleId}: {StatusCode}",
                        vehicleId, response.StatusCode);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error featuring vehicle {VehicleId}", vehicleId);
                return false;
            }
        }

        public async Task<bool> DeleteVehicleAsync(Guid vehicleId, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/vehicles/{vehicleId}", ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to delete vehicle {VehicleId}: {StatusCode}",
                        vehicleId, response.StatusCode);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {VehicleId}", vehicleId);
                return false;
            }
        }

        public async Task<bool> PublishVehicleAsync(Guid vehicleId, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/api/vehicles/{vehicleId}/publish", null, ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing vehicle {VehicleId}", vehicleId);
                return false;
            }
        }

        public async Task<bool> UnpublishVehicleAsync(Guid vehicleId, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/api/vehicles/{vehicleId}/unpublish", null, ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing vehicle {VehicleId}", vehicleId);
                return false;
            }
        }

        // ── Private helpers ─────────────────────────────────────────

        private async Task<int> FetchVehiclesCount(int? status, CancellationToken ct)
        {
            try
            {
                var url = "/api/vehicles?Page=1&PageSize=1";
                var response = await _httpClient.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode) return 0;

                var content = await response.Content.ReadAsStringAsync(ct);
                var result = JsonSerializer.Deserialize<RawVehicleSearchResult>(content, JsonOptions);
                return result?.TotalCount ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> FetchVehiclesByStatus(int status, CancellationToken ct)
        {
            try
            {
                // Query VehiclesSaleService with status filter to get count
                var url = $"/api/vehicles?Page=1&PageSize=1&Status={status}";
                var response = await _httpClient.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch vehicles by status {Status}: {StatusCode}",
                        status, response.StatusCode);
                    return 0;
                }

                var content = await response.Content.ReadAsStringAsync(ct);
                var result = JsonSerializer.Deserialize<RawVehicleSearchResult>(content, JsonOptions);
                return result?.TotalCount ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle count for status {Status}", status);
                return 0;
            }
        }

        private async Task<int> FetchFeaturedCount(CancellationToken ct)
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/vehicles/featured", ct);
                if (!response.IsSuccessStatusCode) return 0;

                var content = await response.Content.ReadAsStringAsync(ct);
                var vehicles = JsonSerializer.Deserialize<List<RawVehicle>>(content, JsonOptions);
                return vehicles?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private static VehicleDto MapToVehicleDto(RawVehicle raw)
        {
            var statusInt = raw.Status;
            var statusName = StatusMap.GetValueOrDefault(statusInt, "unknown");

            // Determine primary image
            var primaryImage = raw.Images?
                .FirstOrDefault(i => i.IsPrimary)?.Url
                ?? raw.Images?.FirstOrDefault()?.Url
                ?? string.Empty;

            return new VehicleDto
            {
                Id = raw.Id,
                Title = raw.Title ?? $"{raw.Year} {raw.Make} {raw.Model}",
                Make = raw.Make ?? string.Empty,
                Model = raw.Model ?? string.Empty,
                Year = raw.Year,
                Price = raw.Price,
                Currency = raw.Currency ?? "DOP",
                Status = statusInt,
                StatusName = statusName,
                SellerId = raw.SellerId,
                SellerName = raw.SellerName ?? raw.SellerEmail ?? "Unknown",
                SellerType = MapSellerType(raw.SellerType),
                ViewCount = raw.ViewCount,
                LeadCount = raw.LeadCount,
                IsFeatured = raw.IsFeatured,
                CreatedAt = raw.CreatedAt,
                PublishedAt = raw.PublishedAt,
                RejectionReason = raw.RejectionReason,
                Images = raw.Images?.Select(i => new VehicleImageDto
                {
                    Url = i.Url ?? string.Empty,
                    IsPrimary = i.IsPrimary
                }).ToList()
            };
        }

        private static string MapSellerType(int sellerType)
        {
            return sellerType switch
            {
                0 => "individual",
                1 => "dealer",
                2 => "franchise",
                3 => "wholesale",
                _ => "individual"
            };
        }

        // ── Raw DTOs for VehiclesSaleService deserialization ─────────

        private class RawVehicleSearchResult
        {
            public List<RawVehicle> Vehicles { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }

        private class RawVehicle
        {
            public Guid Id { get; set; }
            public string? Title { get; set; }
            public string? Make { get; set; }
            public string? Model { get; set; }
            public int Year { get; set; }
            public decimal Price { get; set; }
            public string? Currency { get; set; }
            public int Status { get; set; }
            public Guid? SellerId { get; set; }
            public string? SellerName { get; set; }
            public string? SellerEmail { get; set; }
            public int SellerType { get; set; }
            public int ViewCount { get; set; }
            public int LeadCount { get; set; }
            public bool IsFeatured { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? PublishedAt { get; set; }
            public string? RejectionReason { get; set; }
            public List<RawVehicleImage>? Images { get; set; }
        }

        private class RawVehicleImage
        {
            public string? Url { get; set; }
            public bool IsPrimary { get; set; }
        }
    }
}
