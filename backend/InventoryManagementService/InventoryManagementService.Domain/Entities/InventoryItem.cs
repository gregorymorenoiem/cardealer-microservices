using System;

namespace InventoryManagementService.Domain.Entities;

/// <summary>
/// Inventory Item - Represents a vehicle in a dealer's inventory
/// Related to VehiclesSaleService.Vehicle but focused on dealer management
/// </summary>
public class InventoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Dealer Information
    public Guid DealerId { get; set; }
    
    // Vehicle Reference
    public Guid VehicleId { get; set; } // FK to VehiclesSaleService.Vehicle
    
    // Inventory Status
    public InventoryStatus Status { get; set; } = InventoryStatus.Active;
    public InventoryVisibility Visibility { get; set; } = InventoryVisibility.Public;
    
    // Dealer Notes & Management
    public string? InternalNotes { get; set; } // Private notes for dealer
    public string? Location { get; set; } // Physical location (lot, showroom, etc.)
    public int? StockNumber { get; set; } // Dealer's internal stock number
    public string? VIN { get; set; } // Cached from Vehicle
    
    // Pricing Management
    public decimal? CostPrice { get; set; } // Dealer's cost (private)
    public decimal ListPrice { get; set; } // Public listing price
    public decimal? TargetPrice { get; set; } // Target selling price
    public decimal? MinAcceptablePrice { get; set; } // Minimum acceptable offer
    public bool IsNegotiable { get; set; } = true;
    
    // Acquisition Information
    public DateTime? AcquiredDate { get; set; }
    public AcquisitionSource? AcquisitionSource { get; set; }
    public string? AcquisitionDetails { get; set; } // Trade-in, auction, etc.
    
    // Days on Market
    public int DaysOnMarket
    {
        get
        {
            if (!CreatedAt.HasValue) return 0;
            return (DateTime.UtcNow - CreatedAt.Value).Days;
        }
    }
    
    // Performance Metrics
    public int ViewCount { get; set; } = 0;
    public int InquiryCount { get; set; } = 0;
    public int TestDriveCount { get; set; } = 0;
    public int OfferCount { get; set; } = 0;
    public decimal? HighestOffer { get; set; }
    public DateTime? LastViewedAt { get; set; }
    public DateTime? LastInquiryAt { get; set; }
    
    // Featured & Priority
    public bool IsFeatured { get; set; } = false;
    public DateTime? FeaturedUntil { get; set; }
    public int Priority { get; set; } = 0; // 0 = normal, higher = more priority
    
    // Tags for Organization
    public List<string> Tags { get; set; } = new();
    
    // Timestamps
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? SoldAt { get; set; }
    
    // Sold Information
    public decimal? SoldPrice { get; set; }
    public string? SoldTo { get; set; } // Buyer name (optional)
    
    // Computed Properties
    public decimal? PotentialProfit => SoldPrice.HasValue && CostPrice.HasValue 
        ? SoldPrice - CostPrice 
        : null;
        
    public decimal? ExpectedProfit => ListPrice - (CostPrice ?? 0);
    
    public bool IsOverdue => DaysOnMarket > 90; // More than 90 days without selling
    
    public bool IsHot => ViewCount > 50 && InquiryCount > 5 && DaysOnMarket < 30;
    
    // Methods
    public void MarkAsFeatured(int durationDays)
    {
        IsFeatured = true;
        FeaturedUntil = DateTime.UtcNow.AddDays(durationDays);
    }
    
    public void RemoveFeatured()
    {
        IsFeatured = false;
        FeaturedUntil = null;
    }
    
    public void RecordView()
    {
        ViewCount++;
        LastViewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RecordInquiry()
    {
        InquiryCount++;
        LastInquiryAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RecordOffer(decimal offerAmount)
    {
        OfferCount++;
        if (!HighestOffer.HasValue || offerAmount > HighestOffer.Value)
        {
            HighestOffer = offerAmount;
        }
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsSold(decimal soldPrice, string? buyer = null)
    {
        Status = InventoryStatus.Sold;
        SoldPrice = soldPrice;
        SoldAt = DateTime.UtcNow;
        SoldTo = buyer;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        Status = InventoryStatus.Active;
        if (!PublishedAt.HasValue)
        {
            PublishedAt = DateTime.UtcNow;
        }
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Pause()
    {
        Status = InventoryStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Archive()
    {
        Status = InventoryStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Inventory Status
/// </summary>
public enum InventoryStatus
{
    Draft,      // Not yet published
    Active,     // Live on marketplace
    Paused,     // Temporarily hidden
    Pending,    // Pending sale completion
    Sold,       // Sold and archived
    Archived    // Removed from active inventory
}

/// <summary>
/// Inventory Visibility
/// </summary>
public enum InventoryVisibility
{
    Public,     // Visible to all users
    Unlisted,   // Only visible via direct link
    Private     // Only visible to dealer
}

/// <summary>
/// Acquisition Source
/// </summary>
public enum AcquisitionSource
{
    TradeIn,
    Auction,
    Wholesale,
    DirectPurchase,
    Consignment,
    Other
}
