namespace AdminService.Domain.Entities;

/// <summary>
/// Represents an item in the moderation queue (vehicles, listings, etc.)
/// </summary>
public class ModerationItem
{
    public Guid Id { get; set; }
    public string Type { get; set; } = "vehicle"; // vehicle, dealer, user, content
    public string TargetId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public List<string> Images { get; set; } = new();
    public string SellerName { get; set; } = string.Empty;
    public string SellerType { get; set; } = "individual"; // individual, dealer
    public Guid SellerId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string? FlagReason { get; set; }
    public string Priority { get; set; } = "normal"; // normal, high, urgent
    public string Status { get; set; } = "pending"; // pending, approved, rejected, escalated
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedById { get; set; }
    public string? ReviewerNotes { get; set; }
    public string? RejectionReason { get; set; }
    public int ViewCount { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Statistics for the moderation queue
/// </summary>
public class ModerationStats
{
    public int InQueue { get; set; }
    public int HighPriority { get; set; }
    public int ReviewedToday { get; set; }
    public int RejectedToday { get; set; }
    public int ApprovedToday { get; set; }
    public int EscalatedCount { get; set; }
    public double AvgReviewTimeMinutes { get; set; }
}

/// <summary>
/// Request to submit a moderation action
/// </summary>
public class ModerationActionRequest
{
    public string Action { get; set; } = string.Empty; // approve, reject, escalate, skip
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}
