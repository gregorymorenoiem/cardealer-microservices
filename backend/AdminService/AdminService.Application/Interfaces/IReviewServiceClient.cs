namespace AdminService.Application.Interfaces;

/// <summary>
/// Client para interactuar con ReviewService y gestionar reviews desde el panel admin
/// </summary>
public interface IReviewServiceClient
{
    /// <summary>
    /// Obtener todas las reviews con paginación y filtros
    /// </summary>
    Task<AdminReviewListResult> GetAdminReviewsAsync(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtener reviews reportadas/flagged
    /// </summary>
    Task<List<AdminReviewItemDto>> GetFlaggedReviewsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtener estadísticas globales de reviews
    /// </summary>
    Task<AdminReviewStatsResult> GetAdminReviewStatsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aprobar una review
    /// </summary>
    Task ApproveReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rechazar una review con razón opcional
    /// </summary>
    Task RejectReviewAsync(Guid reviewId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Eliminar una review (admin)
    /// </summary>
    Task DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
}

// ============================================================
// Result/DTO types (local to AdminService)
// ============================================================

public class AdminReviewItemDto
{
    public string Id { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    public string TargetName { get; set; } = string.Empty;
    public string TargetType { get; set; } = "seller";
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string[] Reports { get; set; } = Array.Empty<string>();
}

public class ReportedAdminReviewItemDto : AdminReviewItemDto
{
    public int ReportCount { get; set; }
    public string[] ReportReasons { get; set; } = Array.Empty<string>();
    public string LastReportedAt { get; set; } = string.Empty;
}

public class AdminReviewListResult
{
    public List<AdminReviewItemDto> Items { get; set; } = new();
    public int Total { get; set; }
}

public class AdminReviewStatsResult
{
    public int TotalReviews { get; set; }
    public int PendingReviews { get; set; }
    public int ApprovedReviews { get; set; }
    public decimal AverageRating { get; set; }
    public int ReportedReviews { get; set; }
}
