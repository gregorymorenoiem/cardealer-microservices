namespace MarketingService.Application.DTOs;

public record CampaignDto(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string Status,
    Guid? AudienceId,
    Guid? TemplateId,
    DateTime? ScheduledDate,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int TotalRecipients,
    int SentCount,
    int DeliveredCount,
    int OpenedCount,
    int ClickedCount,
    int BouncedCount,
    int UnsubscribedCount,
    decimal Budget,
    decimal SpentAmount,
    double OpenRate,
    double ClickRate,
    double BounceRate,
    DateTime CreatedAt
);

public record CreateCampaignRequest(
    string Name,
    string Type,
    string? Description = null,
    Guid? AudienceId = null,
    Guid? TemplateId = null,
    decimal Budget = 0
);

public record UpdateCampaignRequest(
    string Name,
    string? Description
);

public record ScheduleCampaignRequest(
    DateTime ScheduledDate
);
