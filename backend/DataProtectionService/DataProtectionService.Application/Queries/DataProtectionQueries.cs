using MediatR;
using DataProtectionService.Application.DTOs;

namespace DataProtectionService.Application.Queries;

public record GetDataChangeLogsQuery(
    Guid UserId,
    string? DataCategory = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 50
) : IRequest<PaginatedResult<DataChangeLogDto>>;

public record GetDataChangeLogByIdQuery(Guid LogId) : IRequest<DataChangeLogDto?>;

public record GetDataChangesByFieldQuery(
    Guid UserId,
    string DataField,
    int Limit = 10
) : IRequest<List<DataChangeLogDto>>;

public record GetDataProtectionStatsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<DataProtectionStatsDto>;

public record GetAnonymizationRecordsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<List<AnonymizationRecordDto>>;

public record CheckAnonymizationStatusQuery(Guid UserId) : IRequest<bool>;

public record GetDataExportQuery(Guid ExportId) : IRequest<DataExportDto?>;

public record GetDataExportsByUserQuery(Guid UserId) : IRequest<List<DataExportDto>>;

public record GetRetentionScheduleQuery() : IRequest<List<RetentionScheduleDto>>;

public record RetentionScheduleDto
{
    public string DataCategory { get; init; } = string.Empty;
    public int RetentionDays { get; init; }
    public string LegalBasis { get; init; } = string.Empty;
    public DateTime? NextPurgeDate { get; init; }
    public int PendingPurgeCount { get; init; }
}
