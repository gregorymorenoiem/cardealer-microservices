namespace DealerManagementService.Domain.Entities;

public class Dealer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // FK to UserService
    public string BusinessName { get; set; } = string.Empty;
    public string RNC { get; set; } = string.Empty; // Registro Nacional de Contribuyentes (RD Tax ID)
    public string? LegalName { get; set; }
    public string? TradeName { get; set; }
    public DealerType Type { get; set; } = DealerType.Independent;
    public DealerStatus Status { get; set; } = DealerStatus.Pending;
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.NotVerified;
    
    // Contact Info
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? MobilePhone { get; set; }
    public string? Website { get; set; }
    
    // Address
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public string Country { get; set; } = "República Dominicana";
    
    // Business Details
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public int? EmployeeCount { get; set; }
    
    // Verification Documents
    public List<DealerDocument> Documents { get; set; } = new();
    
    // Locations/Branches
    public List<DealerLocation> Locations { get; set; } = new();
    
    // Subscription
    public Guid? SubscriptionId { get; set; } // FK to BillingService
    public DealerPlan CurrentPlan { get; set; } = DealerPlan.None;
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public bool IsSubscriptionActive { get; set; }
    
    // Limits (based on plan)
    public int MaxActiveListings { get; set; }
    public int CurrentActiveListings { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; } // Admin user ID
    
    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Helper Methods
    public bool CanAddListing()
    {
        return IsSubscriptionActive && CurrentActiveListings < MaxActiveListings;
    }
    
    public int GetRemainingListings()
    {
        return Math.Max(0, MaxActiveListings - CurrentActiveListings);
    }
    
    public bool IsVerified()
    {
        return VerificationStatus == VerificationStatus.Verified && Status == DealerStatus.Active;
    }
}

public enum DealerType
{
    Independent = 0,    // Single dealership
    Chain = 1,          // Multiple locations, same brand
    MultipleStore = 2,  // Multiple locations, different brands
    Franchise = 3       // Franchised dealer
}

public enum DealerStatus
{
    Pending = 0,        // Registration submitted, awaiting review
    UnderReview = 1,    // Admin is reviewing
    Active = 2,         // Approved and operational
    Suspended = 3,      // Temporarily suspended (payment issues, violations)
    Rejected = 4,       // Registration rejected
    Inactive = 5        // Dealer chose to deactivate
}

public enum VerificationStatus
{
    NotVerified = 0,
    DocumentsUploaded = 1,
    UnderReview = 2,
    Verified = 3,
    Rejected = 4,
    RequiresMoreInfo = 5
}

public enum DealerPlan
{
    None = 0,           // No active subscription
    Starter = 1,        // $49/month - 15 listings
    Pro = 2,            // $129/month - 50 listings
    Enterprise = 3      // $299/month - Unlimited listings
}
