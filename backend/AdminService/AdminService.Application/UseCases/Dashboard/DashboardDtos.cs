namespace AdminService.Application.UseCases.Dashboard;

/// <summary>
/// Dashboard statistics response matching frontend DashboardStats interface
/// </summary>
public class DashboardStatsResponse
{
    public int TotalUsers { get; set; }
    public int TotalVehicles { get; set; }
    public int ActiveVehicles { get; set; }
    public int TotalDealers { get; set; }
    public int ActiveDealers { get; set; }
    public int PendingApprovals { get; set; }
    public int PendingVerifications { get; set; }
    public int TotalReports { get; set; }
    public int OpenSupportTickets { get; set; }
    public decimal Mrr { get; set; }
    public decimal MrrChange { get; set; }
    public decimal UsersChange { get; set; }
    public decimal VehiclesChange { get; set; }
    public decimal DealersChange { get; set; }
}

/// <summary>
/// Dashboard activity item matching frontend RecentActivity interface
/// </summary>
public class DashboardActivityResponse
{
    public string Id { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string SubjectType { get; set; } = string.Empty; // user, dealer, vehicle, payment, report
    public string SubjectId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Dashboard pending action matching frontend PendingAction interface
/// </summary>
public class DashboardPendingActionResponse
{
    public string Type { get; set; } = string.Empty; // moderation, verification, report, support
    public string Title { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Priority { get; set; } = "medium"; // high, medium, low
    public string Href { get; set; } = string.Empty;
}
