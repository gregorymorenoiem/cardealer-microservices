namespace ReviewService.Application.DTOs;

/// <summary>
/// DTO para mostrar una review en el frontend
/// </summary>
public record ReviewDto
{
    public Guid Id { get; init; }
    public Guid BuyerId { get; init; }
    public Guid SellerId { get; init; }
    public Guid? VehicleId { get; init; }
    public Guid? OrderId { get; init; }
    public int Rating { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool IsVerifiedPurchase { get; init; }
    public string BuyerName { get; init; } = string.Empty;
    public string? BuyerPhotoUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    
    // Respuesta del vendedor (Sprint 15 - Nueva funcionalidad)
    public string? SellerResponse { get; init; }
    public DateTime? SellerRespondedAt { get; init; }
    
    // Votos útiles (Sprint 15 - Nueva funcionalidad)
    public ReviewVoteStatsDto? VoteStats { get; init; }
    public bool? UserVotedHelpful { get; init; }  // Para mostrar si el usuario actual votó
    
    // Anti-fraude (Sprint 15 - Nueva funcionalidad)
    public decimal TrustScore { get; init; }
    public bool WasAutoRequested { get; init; }
    
    // Respuesta del vendedor (si existe) - DEPRECATED, use SellerResponse
    public ReviewResponseDto? Response { get; init; }
}

/// <summary>
/// DTO para crear una nueva review
/// </summary>
public record CreateReviewDto
{
    public Guid SellerId { get; init; }
    public Guid? VehicleId { get; init; }
    public Guid? OrderId { get; init; }
    public int Rating { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}

/// <summary>
/// DTO para actualizar una review
/// </summary>
public record UpdateReviewDto
{
    public int Rating { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}

/// <summary>
/// DTO para respuesta de vendedor a review
/// </summary>
public record ReviewResponseDto
{
    public Guid Id { get; init; }
    public Guid ReviewId { get; init; }
    public Guid SellerId { get; init; }
    public string Content { get; init; } = string.Empty;
    public string SellerName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO para crear respuesta de vendedor
/// </summary>
public record CreateReviewResponseDto
{
    public Guid ReviewId { get; init; }
    public string Content { get; init; } = string.Empty;
}

/// <summary>
/// DTO para estadísticas de reviews de un vendedor
/// </summary>
public record ReviewSummaryDto
{
    public Guid SellerId { get; init; }
    public int TotalReviews { get; init; }
    public decimal AverageRating { get; init; }
    public Dictionary<int, int> RatingDistribution { get; init; } = new();
    public decimal PositivePercentage { get; init; }
    public int VerifiedPurchaseReviews { get; init; }
    public DateTime? LastReviewDate { get; init; }
}

/// <summary>
/// DTO para resultados paginados de reviews
/// </summary>
public record PagedReviewsDto
{
    public IEnumerable<ReviewDto> Reviews { get; init; } = new List<ReviewDto>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}

/// <summary>
/// DTO para filtros de reviews
/// </summary>
public record ReviewFiltersDto
{
    public int? Rating { get; init; }
    public bool? OnlyVerified { get; init; }
    public string? SearchTerm { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}