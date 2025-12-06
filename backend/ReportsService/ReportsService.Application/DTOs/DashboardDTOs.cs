namespace ReportsService.Application.DTOs;

public record DashboardDto(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string? Layout,
    bool IsDefault,
    bool IsPublic,
    DateTime CreatedAt,
    IEnumerable<DashboardWidgetDto>? Widgets = null
);

public record DashboardWidgetDto(
    Guid Id,
    string Title,
    string WidgetType,
    string? DataSource,
    string? Configuration,
    int PositionX,
    int PositionY,
    int Width,
    int Height
);

public record CreateDashboardRequest(
    string Name,
    string Type,
    string? Description = null,
    string? Layout = null,
    bool IsPublic = false
);

public record UpdateDashboardRequest(
    string Name,
    string? Description = null,
    string? Layout = null,
    bool? IsPublic = null
);

public record CreateWidgetRequest(
    string Title,
    string WidgetType,
    int PositionX = 0,
    int PositionY = 0,
    int Width = 4,
    int Height = 2,
    string? DataSource = null,
    string? Configuration = null
);

public record UpdateWidgetRequest(
    string? Title = null,
    int? PositionX = null,
    int? PositionY = null,
    int? Width = null,
    int? Height = null,
    string? DataSource = null,
    string? Configuration = null
);
