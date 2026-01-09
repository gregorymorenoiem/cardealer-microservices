namespace ReviewService.Application.DTOs;

/// <summary>
/// DTO para voto de utilidad de review
/// </summary>
public record ReviewHelpfulVoteDto
{
    public Guid Id { get; init; }
    public Guid ReviewId { get; init; }
    public Guid UserId { get; init; }
    public bool IsHelpful { get; init; }
    public DateTime VotedAt { get; init; }
    public string? UserIpAddress { get; init; }
}

/// <summary>
/// DTO para estadísticas de votos de una review
/// </summary>
public record ReviewVoteStatsDto
{
    public Guid ReviewId { get; init; }
    public int TotalVotes { get; init; }
    public int HelpfulVotes { get; init; }
    public int NotHelpfulVotes { get; init; }
    public decimal HelpfulPercentage { get; init; }
}

/// <summary>
/// DTO para badge de vendedor
/// </summary>
public record SellerBadgeDto
{
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public string BadgeType { get; init; } = string.Empty;
    public string BadgeDisplayName { get; init; } = string.Empty;
    public string BadgeDescription { get; init; } = string.Empty;
    public string BadgeColor { get; init; } = string.Empty;
    public DateTime GrantedAt { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO para resultado de cálculo de badges
/// </summary>
public record BadgeCalculationResultDto
{
    public int ProcessedSellers { get; init; }
    public int TotalBadgesGranted { get; init; }
    public Dictionary<Guid, List<string>> Results { get; init; } = new();
}

/// <summary>
/// DTO para solicitud de review
/// </summary>
public record ReviewRequestDto
{
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public Guid BuyerId { get; init; }
    public Guid VehicleId { get; init; }
    public Guid OrderId { get; init; }
    public string BuyerEmail { get; init; } = string.Empty;
    public string BuyerName { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public DateTime RequestSentAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? ReviewCreatedAt { get; init; }
    public int RemindersSent { get; init; }
    public DateTime? LastReminderAt { get; init; }
}

/// <summary>
/// DTO para crear solicitud de review
/// </summary>
public record CreateReviewRequestDto
{
    public Guid SellerId { get; init; }
    public Guid BuyerId { get; init; }
    public Guid? VehicleId { get; init; }
    public Guid? OrderId { get; init; }
    public string BuyerEmail { get; init; } = string.Empty;
    public string? VehicleMake { get; init; }
    public string? VehicleModel { get; init; }
}

/// <summary>
/// DTO para log de detección de fraude
/// </summary>
public record FraudDetectionLogDto
{
    public Guid Id { get; init; }
    public Guid ReviewId { get; init; }
    public string CheckType { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
    public int ConfidenceScore { get; init; }
    public string Details { get; init; } = string.Empty;
    public DateTime CheckedAt { get; init; }
    public string? Metadata { get; init; }
}