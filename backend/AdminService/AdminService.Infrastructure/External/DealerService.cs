using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.Extensions.Logging;
using AdminService.Application.Interfaces;
using AdminService.Application.UseCases.Dealers;

namespace AdminService.Infrastructure.External;

/// <summary>
/// Client for managing dealers via DealerManagementService.
/// This is the authoritative source of dealer data (own DB, rich domain model).
/// Returns empty results when DealerManagementService is unavailable.
/// </summary>
public class DealerService : IDealerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DealerService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public DealerService(HttpClient httpClient, ILogger<DealerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginatedDealerResult> GetDealersAsync(
        string? search = null,
        string? status = null,
        string? plan = null,
        bool? verified = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["page"] = page.ToString();
            queryParams["pageSize"] = pageSize.ToString();
            if (!string.IsNullOrEmpty(search)) queryParams["searchTerm"] = search;
            if (!string.IsNullOrEmpty(status)) queryParams["status"] = status;
            if (verified.HasValue)
            {
                queryParams["verificationStatus"] = verified.Value ? "Verified" : "Pending";
            }

            var url = $"api/dealers?{queryParams}";
            _logger.LogInformation("Fetching dealers from DealerManagementService: {Url}", url);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DealerMgmtListResponse>(JsonOptions, cancellationToken);
                if (result?.Dealers != null)
                {
                    var dealers = result.Dealers.Select(MapToDealerDto).ToList();

                    // Apply plan filter client-side (DealerManagementService GET list doesn't filter by plan)
                    if (!string.IsNullOrEmpty(plan))
                    {
                        dealers = dealers.Where(d =>
                            string.Equals(d.Plan, plan, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    return new PaginatedDealerResult
                    {
                        Items = dealers,
                        Total = result.TotalCount,
                        Page = result.Page,
                        PageSize = result.PageSize
                    };
                }
            }
            else
            {
                _logger.LogWarning("DealerManagementService returned {StatusCode} for dealer list", response.StatusCode);
            }

            return new PaginatedDealerResult { Items = new(), Total = 0, Page = page, PageSize = pageSize };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dealers from DealerManagementService");
            return new PaginatedDealerResult { Items = new(), Total = 0, Page = page, PageSize = pageSize };
        }
    }

    public async Task<DealerStatsDto> GetDealerStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // DealerManagementService exposes GET /api/dealers/statistics
            var url = "api/dealers/statistics";
            _logger.LogInformation("Fetching dealer stats from DealerManagementService: {Url}", url);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var stats = await response.Content.ReadFromJsonAsync<DealerMgmtStatsResponse>(JsonOptions, cancellationToken);
                if (stats != null)
                {
                    return new DealerStatsDto
                    {
                        Total = stats.TotalDealers,
                        Active = stats.ActiveDealers,
                        Pending = stats.PendingVerification,
                        Suspended = Math.Max(0, stats.TotalDealers - stats.ActiveDealers - stats.PendingVerification),
                        TotalMrr = (stats.ByPlan?.Enterprise ?? 0) * 299
                                 + (stats.ByPlan?.Pro ?? 0) * 149
                                 + (stats.ByPlan?.Starter ?? 0) * 49,
                        ByPlan = new DealerPlanBreakdown
                        {
                            Starter = stats.ByPlan?.Starter ?? 0,
                            Pro = stats.ByPlan?.Pro ?? 0,
                            Enterprise = stats.ByPlan?.Enterprise ?? 0
                        }
                    };
                }
            }
            else
            {
                _logger.LogWarning("DealerManagementService stats returned {StatusCode}", response.StatusCode);
            }

            // If stats endpoint returns minimal data, fetch all dealers and compute
            return await ComputeStatsFromListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dealer stats from DealerManagementService");
            return new DealerStatsDto();
        }
    }

    public async Task<AdminDealerDto?> GetDealerByIdAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"api/dealers/{dealerId}";
            _logger.LogInformation("Fetching dealer {DealerId} from DealerManagementService", dealerId);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var dealer = await response.Content.ReadFromJsonAsync<DealerMgmtDto>(JsonOptions, cancellationToken);
                if (dealer != null)
                {
                    return MapToDealerDto(dealer);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("Dealer {DealerId} not found in DealerManagementService", dealerId);
                return null;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dealer {DealerId}", dealerId);
            return null;
        }
    }

    public async Task VerifyDealerAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        try
        {
            // DealerManagementService: POST /api/dealers/{id}/verify with body { approved, rejectionReason }
            var url = $"api/dealers/{dealerId}/verify";
            _logger.LogInformation("Verifying dealer {DealerId} via DealerManagementService", dealerId);

            var payload = new { DealerId = dealerId, Approved = true, RejectionReason = (string?)null };
            var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to verify dealer {DealerId}: {StatusCode} - {Body}", dealerId, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying dealer {DealerId}", dealerId);
        }
    }

    public async Task SuspendDealerAsync(Guid dealerId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use update endpoint to set status to Suspended
            var url = $"api/dealers/{dealerId}";
            _logger.LogInformation("Suspending dealer {DealerId}: {Reason}", dealerId, reason);

            var payload = new { Status = "Suspended", Description = $"Suspended by admin: {reason}" };
            var response = await _httpClient.PutAsJsonAsync(url, payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to suspend dealer {DealerId}: {StatusCode} - {Body}", dealerId, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending dealer {DealerId}", dealerId);
        }
    }

    public async Task ReactivateDealerAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"api/dealers/{dealerId}";
            _logger.LogInformation("Reactivating dealer {DealerId}", dealerId);

            var payload = new { Status = "Active", Description = "Reactivated by admin" };
            var response = await _httpClient.PutAsJsonAsync(url, payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to reactivate dealer {DealerId}: {StatusCode} - {Body}", dealerId, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating dealer {DealerId}", dealerId);
        }
    }

    public async Task DeleteDealerAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"api/dealers/{dealerId}";
            _logger.LogInformation("Deleting dealer {DealerId}", dealerId);

            var response = await _httpClient.DeleteAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to delete dealer {DealerId}: {StatusCode} - {Body}", dealerId, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting dealer {DealerId}", dealerId);
        }
    }

    // =========================================================================
    // COMPUTE STATS FROM LIST (fallback when /statistics returns partial data)
    // =========================================================================

    private async Task<DealerStatsDto> ComputeStatsFromListAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Fetch a large page to compute stats
            var result = await GetDealersAsync(page: 1, pageSize: 500, cancellationToken: cancellationToken);
            var dealers = result.Items;

            return new DealerStatsDto
            {
                Total = result.Total,
                Active = dealers.Count(d => string.Equals(d.Status, "active", StringComparison.OrdinalIgnoreCase)),
                Pending = dealers.Count(d => string.Equals(d.Status, "pending", StringComparison.OrdinalIgnoreCase)),
                Suspended = dealers.Count(d => string.Equals(d.Status, "suspended", StringComparison.OrdinalIgnoreCase)),
                TotalMrr = dealers.Sum(d => d.Mrr),
                ByPlan = new DealerPlanBreakdown
                {
                    Starter = dealers.Count(d => string.Equals(d.Plan, "starter", StringComparison.OrdinalIgnoreCase)),
                    Pro = dealers.Count(d => string.Equals(d.Plan, "pro", StringComparison.OrdinalIgnoreCase)),
                    Enterprise = dealers.Count(d => string.Equals(d.Plan, "enterprise", StringComparison.OrdinalIgnoreCase))
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing stats from dealer list");
            return new DealerStatsDto();
        }
    }

    // =========================================================================
    // MAPPING: DealerManagementService DealerDto → AdminDealerDto
    // =========================================================================

    private static AdminDealerDto MapToDealerDto(DealerMgmtDto dealer)
    {
        var plan = (dealer.CurrentPlan ?? "Free").ToLowerInvariant() switch
        {
            "enterprise" or "premium" => "enterprise",
            "professional" or "pro" => "pro",
            "starter" or "basic" => "starter",
            _ => "none"
        };

        // Map DealerManagementService status → frontend-compatible status
        var status = (dealer.Status ?? "Pending").ToLowerInvariant() switch
        {
            "active" => "active",
            "suspended" or "deactivated" => "suspended",
            "rejected" => "rejected",
            _ => "pending"
        };

        var verified = (dealer.VerificationStatus ?? "Pending").ToLowerInvariant() switch
        {
            "verified" or "approved" => true,
            _ => false
        };

        var mrr = plan switch
        {
            "enterprise" => 299m,
            "pro" => 149m,
            "starter" => 49m,
            _ => 0m
        };

        return new AdminDealerDto
        {
            Id = dealer.Id.ToString(),
            Name = dealer.BusinessName ?? "Sin nombre",
            Email = dealer.Email ?? "",
            Phone = dealer.Phone ?? "",
            Status = status,
            Verified = verified,
            Plan = plan,
            VehiclesCount = dealer.CurrentActiveListings,
            SalesCount = 0, // TODO: Get from analytics
            Rating = 0, // TODO: Get from ReviewService
            ReviewsCount = 0, // TODO: Get from ReviewService
            Location = !string.IsNullOrEmpty(dealer.City) ? $"{dealer.City}, {dealer.Province}" : "N/A",
            CreatedAt = dealer.CreatedAt.ToString("O"),
            Mrr = mrr,
            DocumentsCount = dealer.Documents?.Count ?? 0,
            PendingDocuments = dealer.Documents?
                .Count(d => string.Equals(d.VerificationStatus, "Pending", StringComparison.OrdinalIgnoreCase)) ?? 0
        };
    }

}

// =========================================================================
// DTOs FOR DEALERMANAGEMENTSERVICE RESPONSES
// =========================================================================

internal class DealerMgmtListResponse
{
    public List<DealerMgmtDto> Dealers { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

internal class DealerMgmtDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string? RNC { get; set; }
    public string? LegalName { get; set; }
    public string? TradeName { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? VerificationStatus { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public int? EmployeeCount { get; set; }
    public string? CurrentPlan { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public bool IsSubscriptionActive { get; set; }
    public int MaxActiveListings { get; set; }
    public int CurrentActiveListings { get; set; }
    public int RemainingListings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public List<DealerMgmtDocumentDto>? Documents { get; set; }
    public List<DealerMgmtLocationDto>? Locations { get; set; }
}

internal class DealerMgmtDocumentDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? VerificationStatus { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }
}

internal class DealerMgmtLocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Province { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
}

internal class DealerMgmtStatsResponse
{
    public int TotalDealers { get; set; }
    public int ActiveDealers { get; set; }
    public int PendingVerification { get; set; }
    public DealerMgmtPlanBreakdown? ByPlan { get; set; }
}

internal class DealerMgmtPlanBreakdown
{
    public int Starter { get; set; }
    public int Pro { get; set; }
    public int Enterprise { get; set; }
}
