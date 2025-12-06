namespace IntegrationService.Application.DTOs;

public record SyncJobDto(
    Guid Id,
    Guid IntegrationId,
    string Name,
    string EntityType,
    string Direction,
    string Status,
    string? FilterCriteria,
    DateTime? ScheduledAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int TotalRecords,
    int ProcessedRecords,
    int SuccessCount,
    int ErrorCount,
    DateTime CreatedAt
);

public record CreateSyncJobRequest(
    Guid IntegrationId,
    string Name,
    string EntityType,
    string Direction,
    string? FilterCriteria = null,
    DateTime? ScheduledAt = null
);

public record UpdateSyncJobProgressRequest(
    int ProcessedRecords,
    int SuccessCount,
    int ErrorCount
);
