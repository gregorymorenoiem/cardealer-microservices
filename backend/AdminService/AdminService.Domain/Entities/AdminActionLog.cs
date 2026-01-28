namespace AdminService.Domain.Entities;

/// <summary>
/// Registro de acciones realizadas por administradores
/// </summary>
public class AdminActionLog
{
    public Guid Id { get; set; }
    public Guid AdminId { get; set; }
    public string? AdminName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TargetType { get; set; }
    public string? TargetId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// Platform stats model
/// </summary>
public class PlatformStats
{
    public int TotalDealers { get; set; }
    public int ActiveDealers { get; set; }
    public int PendingDealers { get; set; }
    public int TotalUsers { get; set; }
    public int TotalListings { get; set; }
    public int PendingListings { get; set; }
    public int TotalTransactions { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int OpenTickets { get; set; }
}

/// <summary>
/// Pending dealer info for dashboard
/// </summary>
public class PendingDealerInfo
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

/// <summary>
/// Pending listing info for moderation
/// </summary>
public class PendingListingInfo
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

/// <summary>
/// Pending report info for moderation
/// </summary>
public class PendingReportInfo
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public DateTime ReportedAt { get; set; }
    public int ReportCount { get; set; }
}

/// <summary>
/// Platform settings model
/// </summary>
public class PlatformSettings
{
    public bool MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public bool RegistrationEnabled { get; set; } = true;
    public bool DealerRegistrationEnabled { get; set; } = true;
    public bool EarlyBirdActive { get; set; }
    public DateTime? EarlyBirdEndDate { get; set; }
    public decimal EarlyBirdDiscount { get; set; }
    public Dictionary<string, object>? FeatureFlags { get; set; }
    public Dictionary<string, object>? Limits { get; set; }
}