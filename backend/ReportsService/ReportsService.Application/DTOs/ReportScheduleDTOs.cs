namespace ReportsService.Application.DTOs;

public record ReportScheduleDto(
    Guid Id,
    Guid ReportId,
    string Name,
    string Frequency,
    string? ExecutionTime,
    string? DayOfWeek,
    int? DayOfMonth,
    bool IsActive,
    string? Recipients,
    bool SendEmail,
    bool SaveToStorage,
    DateTime? LastRunAt,
    DateTime? NextRunAt,
    string? LastRunStatus,
    DateTime CreatedAt
);

public record CreateReportScheduleRequest(
    Guid ReportId,
    string Name,
    string Frequency,
    string? ExecutionTime = null,
    string? DayOfWeek = null,
    int? DayOfMonth = null,
    string? Recipients = null,
    bool SendEmail = true,
    bool SaveToStorage = true
);

public record UpdateReportScheduleRequest(
    string Name,
    string? ExecutionTime = null,
    string? DayOfWeek = null,
    int? DayOfMonth = null,
    string? Recipients = null,
    bool? SendEmail = null,
    bool? SaveToStorage = null
);
