namespace MarketingService.Application.DTOs;

public record EmailTemplateDto(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string Subject,
    string Body,
    string? HtmlBody,
    string? PreheaderText,
    string? FromName,
    string? FromEmail,
    string? ReplyToEmail,
    bool IsActive,
    bool IsDefault,
    string Category,
    string? Tags,
    DateTime CreatedAt
);

public record CreateEmailTemplateRequest(
    string Name,
    string Type,
    string Subject,
    string Body,
    string? Description = null,
    string? HtmlBody = null
);

public record UpdateEmailTemplateRequest(
    string Name,
    string Subject,
    string Body,
    string? HtmlBody = null
);
