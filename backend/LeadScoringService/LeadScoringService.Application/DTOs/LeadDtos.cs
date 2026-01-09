using LeadScoringService.Domain.Entities;

namespace LeadScoringService.Application.DTOs;

/// <summary>
/// DTO principal de Lead
/// </summary>
public record LeadDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string UserFullName,
    string? UserPhone,
    Guid VehicleId,
    string VehicleTitle,
    decimal VehiclePrice,
    Guid DealerId,
    string DealerName,
    int Score,
    string Temperature, // HOT, WARM, COLD
    decimal ConversionProbability,
    int EngagementScore,
    int RecencyScore,
    int IntentScore,
    int ViewCount,
    int ContactCount,
    int FavoriteCount,
    int ShareCount,
    int ComparisonCount,
    bool HasScheduledTestDrive,
    bool HasRequestedFinancing,
    int TotalTimeSpentSeconds,
    string Status,
    string Source,
    DateTime FirstInteractionAt,
    DateTime LastInteractionAt,
    DateTime? LastContactedAt,
    DateTime? ConvertedAt,
    string? DealerNotes,
    List<LeadActionDto> RecentActions
);

/// <summary>
/// DTO de acción de lead
/// </summary>
public record LeadActionDto(
    Guid Id,
    string ActionType,
    string Description,
    int ScoreImpact,
    DateTime OccurredAt
);

/// <summary>
/// DTO para crear/actualizar un lead
/// </summary>
public record CreateLeadDto(
    Guid UserId,
    string UserEmail,
    string UserFullName,
    string? UserPhone,
    Guid VehicleId,
    string VehicleTitle,
    decimal VehiclePrice,
    Guid DealerId,
    string DealerName,
    string Source
);

/// <summary>
/// DTO para registrar una acción
/// </summary>
public record RecordLeadActionDto(
    Guid LeadId,
    string ActionType,
    string? Metadata,
    string? IpAddress,
    string? UserAgent
);

/// <summary>
/// DTO para actualizar estado del lead
/// </summary>
public record UpdateLeadStatusDto(
    string Status,
    string? DealerNotes
);

/// <summary>
/// DTO de estadísticas de leads por dealer
/// </summary>
public record LeadStatisticsDto(
    Guid DealerId,
    int TotalLeads,
    int HotLeads,
    int WarmLeads,
    int ColdLeads,
    int NewLeads,
    int ContactedLeads,
    int ConvertedLeads,
    decimal AverageScore,
    decimal ConversionRate,
    List<LeadTrendDto> ScoreTrends
);

/// <summary>
/// DTO de tendencia de scores
/// </summary>
public record LeadTrendDto(
    DateTime Date,
    decimal AverageScore,
    int LeadCount
);

/// <summary>
/// DTO de respuesta paginada
/// </summary>
public record PagedLeadsResponse(
    List<LeadDto> Leads,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

/// <summary>
/// DTO de notificación de lead HOT
/// </summary>
public record HotLeadNotificationDto(
    Guid LeadId,
    Guid DealerId,
    string DealerEmail,
    string UserFullName,
    string UserEmail,
    string? UserPhone,
    string VehicleTitle,
    int Score,
    string Reason
);
