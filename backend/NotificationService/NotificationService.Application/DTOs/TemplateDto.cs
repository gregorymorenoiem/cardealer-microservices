using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs;

public record CreateTemplateRequest(
    string Name,
    string Subject,
    string Body,
    NotificationType Type,
    string? Description = null,
    string? Category = null,
    Dictionary<string, string>? Variables = null,
    string? Tags = null,
    string? PreviewData = null
);

public record UpdateTemplateRequest(
    string Subject,
    string Body,
    string? Description = null,
    Dictionary<string, string>? Variables = null,
    string? Tags = null,
    string? PreviewData = null
);

public record TemplateResponse(
    Guid Id,
    string Name,
    string Subject,
    string Body,
    NotificationType Type,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    Dictionary<string, string>? Variables,
    string? Description,
    string? Category,
    int Version,
    Guid? PreviousVersionId,
    string? Tags,
    List<string> TagsList,
    string CreatedBy,
    string? UpdatedBy
);

public record PreviewTemplateRequest(
    string? TemplateId = null, // Use existing template
    string? TemplateContent = null, // Or provide content directly
    Dictionary<string, object>? Parameters = null
);

public record PreviewTemplateResponse(
    string RenderedContent,
    bool IsValid,
    List<string> Errors,
    List<string> MissingParameters,
    List<string> AvailableParameters
);

public record GetTemplatesRequest(
    NotificationType? Type = null,
    string? Category = null,
    string? Tag = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20
);

public record GetTemplatesResponse(
    List<TemplateResponse> Templates,
    int TotalCount,
    int PageNumber,
    int PageSize
);

public record TemplateValidationResponse(
    bool IsValid,
    List<string> Errors,
    List<string> Placeholders
);
