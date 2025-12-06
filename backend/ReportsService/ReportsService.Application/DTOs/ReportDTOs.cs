namespace ReportsService.Application.DTOs;

public record ReportDto(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string Format,
    string Status,
    DateTime? StartDate,
    DateTime? EndDate,
    string? FilePath,
    long? FileSize,
    string? ErrorMessage,
    DateTime? GeneratedAt,
    DateTime? ExpiresAt,
    DateTime CreatedAt
);

public record CreateReportRequest(
    string Name,
    string Type,
    string Format,
    string? Description = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? QueryDefinition = null,
    string? Parameters = null,
    string? FilterCriteria = null
);

public record UpdateReportRequest(
    string Name,
    string? Description = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? QueryDefinition = null,
    string? Parameters = null,
    string? FilterCriteria = null
);

public record GenerateReportRequest(
    string? Parameters = null
);

public record ReportGeneratedRequest(
    string FilePath,
    long FileSize,
    DateTime? ExpiresAt = null
);
