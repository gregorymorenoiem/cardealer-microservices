using System;
using System.Collections.Generic;
using InventoryManagementService.Domain.Entities;

namespace InventoryManagementService.Application.DTOs;

public record InventoryItemDto
{
    public Guid Id { get; init; }
    public Guid DealerId { get; init; }
    public Guid VehicleId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Visibility { get; init; } = string.Empty;
    public string? InternalNotes { get; init; }
    public string? Location { get; init; }
    public int? StockNumber { get; init; }
    public string? VIN { get; init; }
    public decimal? CostPrice { get; init; }
    public decimal ListPrice { get; init; }
    public decimal? TargetPrice { get; init; }
    public decimal? MinAcceptablePrice { get; init; }
    public bool IsNegotiable { get; init; }
    public DateTime? AcquiredDate { get; init; }
    public string? AcquisitionSource { get; init; }
    public string? AcquisitionDetails { get; init; }
    public int DaysOnMarket { get; init; }
    public int ViewCount { get; init; }
    public int InquiryCount { get; init; }
    public int TestDriveCount { get; init; }
    public int OfferCount { get; init; }
    public decimal? HighestOffer { get; init; }
    public DateTime? LastViewedAt { get; init; }
    public DateTime? LastInquiryAt { get; init; }
    public bool IsFeatured { get; init; }
    public DateTime? FeaturedUntil { get; init; }
    public int Priority { get; init; }
    public List<string> Tags { get; init; } = new();
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? SoldAt { get; init; }
    public decimal? SoldPrice { get; init; }
    public string? SoldTo { get; init; }
    public decimal? ExpectedProfit { get; init; }
    public bool IsOverdue { get; init; }
    public bool IsHot { get; init; }
    
    // Vehicle info (populated from VehiclesSaleService if needed)
    public VehicleBasicInfoDto? VehicleInfo { get; init; }
}

public record VehicleBasicInfoDto
{
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public string? ImageUrl { get; init; }
}

public record CreateInventoryItemRequest
{
    public Guid DealerId { get; init; }
    public Guid VehicleId { get; init; }
    public string? InternalNotes { get; init; }
    public string? Location { get; init; }
    public int? StockNumber { get; init; }
    public string? VIN { get; init; }
    public decimal? CostPrice { get; init; }
    public decimal ListPrice { get; init; }
    public decimal? TargetPrice { get; init; }
    public decimal? MinAcceptablePrice { get; init; }
    public bool IsNegotiable { get; init; } = true;
    public DateTime? AcquiredDate { get; init; }
    public string? AcquisitionSource { get; init; }
    public string? AcquisitionDetails { get; init; }
    public List<string>? Tags { get; init; }
}

public record UpdateInventoryItemRequest
{
    public string? InternalNotes { get; init; }
    public string? Location { get; init; }
    public int? StockNumber { get; init; }
    public decimal? CostPrice { get; init; }
    public decimal? ListPrice { get; init; }
    public decimal? TargetPrice { get; init; }
    public decimal? MinAcceptablePrice { get; init; }
    public bool? IsNegotiable { get; init; }
    public bool? IsFeatured { get; init; }
    public int? Priority { get; init; }
    public List<string>? Tags { get; init; }
}

public record BulkImportJobDto
{
    public Guid Id { get; init; }
    public Guid DealerId { get; init; }
    public Guid UserId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string FileType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int TotalRows { get; init; }
    public int ProcessedRows { get; init; }
    public int SuccessfulRows { get; init; }
    public int FailedRows { get; init; }
    public int SkippedRows { get; init; }
    public decimal ProgressPercentage { get; init; }
    public List<ImportErrorDto> Errors { get; init; } = new();
    public string? FailureReason { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public TimeSpan? Duration { get; init; }
}

public record ImportErrorDto
{
    public int RowNumber { get; init; }
    public string Field { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}

public record InventoryStatsDto
{
    public int TotalItems { get; init; }
    public int ActiveItems { get; init; }
    public int SoldItems { get; init; }
    public int PausedItems { get; init; }
    public int FeaturedItems { get; init; }
    public int HotItems { get; init; }
    public int OverdueItems { get; init; }
    public decimal TotalInventoryValue { get; init; }
    public decimal AverageListPrice { get; init; }
    public int AverageDaysOnMarket { get; init; }
    public decimal TotalProjectedProfit { get; init; }
}

public record PagedResultDto<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
