namespace ReviewService.Application.DTOs;

/// &lt;summary&gt;
/// DTO para mostrar una review en el frontend
/// &lt;/summary&gt;
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
    public int HelpfulVotes { get; init; }
    public int TotalVotes { get; init; }
    public DateTime CreatedAt { get; init; }
    
    // Respuesta del vendedor (si existe)
    public ReviewResponseDto? Response { get; init; }
}

/// &lt;summary&gt;
/// DTO para crear una nueva review
/// &lt;/summary&gt;
public record CreateReviewDto
{
    public Guid SellerId { get; init; }
    public Guid? VehicleId { get; init; }
    public Guid? OrderId { get; init; }
    public int Rating { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}

/// &lt;summary&gt;
/// DTO para actualizar una review
/// &lt;/summary&gt;
public record UpdateReviewDto
{
    public int Rating { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}

/// &lt;summary&gt;
/// DTO para respuesta de vendedor a review
/// &lt;/summary&gt;
public record ReviewResponseDto
{
    public Guid Id { get; init; }
    public Guid ReviewId { get; init; }
    public Guid SellerId { get; init; }
    public string Content { get; init; } = string.Empty;
    public string SellerName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

/// &lt;summary&gt;
/// DTO para crear respuesta de vendedor
/// &lt;/summary&gt;
public record CreateReviewResponseDto
{
    public Guid ReviewId { get; init; }
    public string Content { get; init; } = string.Empty;
}

/// &lt;summary&gt;
/// DTO para estadísticas de reviews de un vendedor
/// &lt;/summary&gt;
public record ReviewSummaryDto
{
    public Guid SellerId { get; init; }
    public int TotalReviews { get; init; }
    public decimal AverageRating { get; init; }
    public Dictionary&lt;int, int&gt; RatingDistribution { get; init; } = new();
    public decimal PositivePercentage { get; init; }
    public int VerifiedPurchaseReviews { get; init; }
    public DateTime? LastReviewDate { get; init; }
}

/// &lt;summary&gt;
/// DTO para resultados paginados de reviews
/// &lt;/summary&gt;
public record PagedReviewsDto
{
    public IEnumerable&lt;ReviewDto&gt; Reviews { get; init; } = new List&lt;ReviewDto&gt;();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}

/// &lt;summary&gt;
/// DTO para filtros de reviews
/// &lt;/summary&gt;
public record ReviewFiltersDto
{
    public int? Rating { get; init; }
    public bool? OnlyVerified { get; init; }
    public string? SearchTerm { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}