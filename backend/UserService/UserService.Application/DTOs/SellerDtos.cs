using System;
using System.Collections.Generic;
using UserService.Domain.Entities;

namespace UserService.Application.DTOs;

#region Seller Profile DTOs

/// <summary>
/// Full seller profile information
/// </summary>
public class SellerProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Personal Info
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }

    // Contact
    public string Phone { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public string? WhatsApp { get; set; }
    public string Email { get; set; } = string.Empty;

    // Location
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public string Country { get; set; } = "DO";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Verification
    public SellerVerificationStatus VerificationStatus { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Metrics
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int TotalSales { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int ResponseTimeMinutes { get; set; }

    // Configuration
    public bool IsActive { get; set; }
    public bool AcceptsOffers { get; set; }
    public bool ShowPhone { get; set; }
    public bool ShowLocation { get; set; }
    public string? PreferredContactMethod { get; set; }
    public int MaxActiveListings { get; set; }
    public bool CanSellHighValue { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // From User entity (optional join)
    public string? UserEmail { get; set; }
    public string? UserFullName { get; set; }

    // Identity documents (for verified sellers)
    public List<IdentityDocumentDto>? IdentityDocuments { get; set; }
}

/// <summary>
/// Summary view of seller profile for listings
/// </summary>
public class SellerProfileSummaryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public SellerVerificationStatus VerificationStatus { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int ActiveListings { get; set; }
    public int TotalSales { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ResponseTimeMinutes { get; set; }
}

#endregion

#region Create/Update Requests

/// <summary>
/// Request to create a new seller profile
/// </summary>
public class CreateSellerProfileRequest
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }

    // Contact
    public string Phone { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public string? WhatsApp { get; set; }
    public string Email { get; set; } = string.Empty;

    // Location
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public string Country { get; set; } = "DO";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Preferences
    public bool AcceptsOffers { get; set; } = true;
    public bool ShowPhone { get; set; } = true;
    public bool ShowLocation { get; set; } = true;
    public string? PreferredContactMethod { get; set; } = "whatsapp";
}

/// <summary>
/// Request to update a seller profile
/// </summary>
public class UpdateSellerProfileRequest
{
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? AvatarUrl { get; set; }

    // Contact
    public string? Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? WhatsApp { get; set; }

    // Location
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Preferences
    public bool? AcceptsOffers { get; set; }
    public bool? ShowPhone { get; set; }
    public bool? ShowLocation { get; set; }
    public string? PreferredContactMethod { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Request to verify/reject a seller profile
/// </summary>
public class VerifySellerProfileRequest
{
    public bool IsVerified { get; set; }
    public Guid VerifiedByUserId { get; set; }
    public string? Notes { get; set; }
}

public class PaginatedSellersResponse
{
    public List<SellerProfileSummaryDto> Sellers { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

#endregion

#region Identity Document DTOs

public class IdentityDocumentDto
{
    public Guid Id { get; set; }
    public Guid SellerProfileId { get; set; }
    public IdentityDocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string? IssuingCountry { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DocumentVerificationStatus Status { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime UploadedAt { get; set; }
    // Note: Image URLs are NOT included for security
}

public class UploadIdentityDocumentRequest
{
    public Guid SellerProfileId { get; set; }
    public IdentityDocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string? IssuingCountry { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string FrontImageUrl { get; set; } = string.Empty;
    public string? BackImageUrl { get; set; }
    public string? SelfieWithDocumentUrl { get; set; }
}

public class VerifyIdentityDocumentRequest
{
    public DocumentVerificationStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public Guid VerifiedBy { get; set; }
}

public class IdentityVerificationStatusDto
{
    public Guid SellerProfileId { get; set; }
    public SellerVerificationStatus VerificationStatus { get; set; }
    public List<IdentityDocumentDto> Documents { get; set; } = new();
    public List<string> PendingRequirements { get; set; } = new();
    public bool CanUpgrade { get; set; }
}

#endregion

#region Seller Stats DTOs

/// <summary>
/// Basic stats available from SellerProfile entity
/// </summary>
public class SellerStatsDto
{
    public Guid SellerId { get; set; }
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int TotalSales { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int MaxActiveListings { get; set; }
    public bool CanSellHighValue { get; set; }
    public int MemberSinceDays { get; set; }
}

/// <summary>
/// Extended stats (requires aggregation from VehiclesSaleService)
/// </summary>
public class SellerExtendedStatsDto
{
    public Guid SellerId { get; set; }
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int SoldListings { get; set; }
    public int TotalViews { get; set; }
    public int TotalInquiries { get; set; }
    public int TotalFavorites { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal AverageListingPrice { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int ResponseTimeMinutes { get; set; }
}

public class SellerPerformanceDto
{
    public Guid SellerId { get; set; }
    public string Period { get; set; } = "monthly";
    public List<PerformanceDataPoint> ViewsOverTime { get; set; } = new();
    public List<PerformanceDataPoint> InquiriesOverTime { get; set; } = new();
    public List<PerformanceDataPoint> SalesOverTime { get; set; } = new();
    public List<TopListingDto> TopListings { get; set; } = new();
}

public class PerformanceDataPoint
{
    public DateTime Date { get; set; }
    public int Value { get; set; }
}

public class TopListingDto
{
    public Guid ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Views { get; set; }
    public int Inquiries { get; set; }
    public int Favorites { get; set; }
}

#endregion
