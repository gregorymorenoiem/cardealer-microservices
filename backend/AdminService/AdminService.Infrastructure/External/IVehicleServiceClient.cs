using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AdminService.Infrastructure.External
{
    /// <summary>
    /// HTTP client for communicating with VehiclesSaleService
    /// </summary>
    public interface IVehicleServiceClient
    {
        Task<VehicleSearchResponse?> SearchVehiclesAsync(VehicleSearchFilters filters, CancellationToken ct = default);
        Task<VehicleDetailResponse?> GetVehicleByIdAsync(Guid vehicleId, CancellationToken ct = default);
        Task<VehicleStatsResponse> GetVehicleStatsAsync(CancellationToken ct = default);
        Task<bool> FeatureVehicleAsync(Guid vehicleId, bool featured, CancellationToken ct = default);
        Task<bool> DeleteVehicleAsync(Guid vehicleId, CancellationToken ct = default);
        Task<bool> PublishVehicleAsync(Guid vehicleId, CancellationToken ct = default);
        Task<bool> UnpublishVehicleAsync(Guid vehicleId, CancellationToken ct = default);
    }

    // ── Request / Response DTOs ──────────────────────────────────────

    public class VehicleSearchFilters
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? SellerType { get; set; }
        public bool? Featured { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class VehicleSearchResponse
    {
        public List<VehicleDto> Vehicles { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class VehicleDetailResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "DOP";
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public Guid? SellerId { get; set; }
        public string? SellerName { get; set; }
        public string? SellerType { get; set; }
        public int ViewCount { get; set; }
        public int LeadCount { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? RejectionReason { get; set; }
        public List<VehicleImageDto>? Images { get; set; }
    }

    public class VehicleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "DOP";
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public Guid? SellerId { get; set; }
        public string? SellerName { get; set; }
        public string? SellerType { get; set; }
        public int ViewCount { get; set; }
        public int LeadCount { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? RejectionReason { get; set; }
        public List<VehicleImageDto>? Images { get; set; }
    }

    public class VehicleImageDto
    {
        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public class VehicleStatsResponse
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
        public int Featured { get; set; }
        public int WithReports { get; set; }
    }
}
