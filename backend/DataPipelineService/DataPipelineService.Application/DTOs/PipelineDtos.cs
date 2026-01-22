// =====================================================
// DataPipelineService - DTOs
// Procesamiento de Datos y ETL
// =====================================================

using DataPipelineService.Domain.Enums;

namespace DataPipelineService.Application.DTOs;

// ==================== Pipelines ====================
public record DataPipelineDto(
    Guid Id,
    string Name,
    string? Description,
    PipelineType Type,
    PipelineStatus Status,
    string SourceType,
    string DestinationType,
    bool IsActive,
    DateTime? LastRunAt,
    DateTime? NextRunAt,
    int StepCount
);

public record DataPipelineDetailDto(
    Guid Id,
    string Name,
    string? Description,
    PipelineType Type,
    PipelineStatus Status,
    string SourceType,
    string SourceConfig,
    string DestinationType,
    string DestinationConfig,
    string? TransformationScript,
    string? CronSchedule,
    bool IsActive,
    DateTime? LastRunAt,
    DateTime? NextRunAt,
    IEnumerable<PipelineStepDto> Steps,
    IEnumerable<PipelineRunSummaryDto> RecentRuns
);

public record CreatePipelineDto(
    string Name,
    string? Description,
    PipelineType Type,
    string SourceType,
    string SourceConfig,
    string DestinationType,
    string DestinationConfig,
    string? TransformationScript,
    string? CronSchedule
);

public record UpdatePipelineDto(
    string Name,
    string? Description,
    string? TransformationScript,
    string? CronSchedule,
    bool IsActive
);

// ==================== Steps ====================
public record PipelineStepDto(
    Guid Id,
    int StepOrder,
    string Name,
    StepType StepType,
    string Configuration,
    bool IsActive
);

public record CreateStepDto(
    Guid PipelineId,
    string Name,
    StepType StepType,
    string Configuration
);

// ==================== Runs ====================
public record PipelineRunDto(
    Guid Id,
    Guid DataPipelineId,
    RunStatus Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    long RecordsProcessed,
    long RecordsSuccess,
    long RecordsFailed,
    string? ErrorMessage,
    string TriggeredBy,
    IEnumerable<RunLogDto> Logs
);

public record PipelineRunSummaryDto(
    Guid Id,
    RunStatus Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    long RecordsProcessed,
    string TriggeredBy
);

public record StartRunDto(
    Guid PipelineId,
    string? TriggeredBy
);

// ==================== Logs ====================
public record RunLogDto(
    Guid Id,
    LogLevel Level,
    string Message,
    string? Details,
    int? StepOrder,
    DateTime Timestamp
);

// ==================== Connectors ====================
public record DataConnectorDto(
    Guid Id,
    string Name,
    ConnectorType ConnectorType,
    bool IsActive,
    DateTime? LastTestedAt,
    bool? LastTestSuccess
);

public record CreateConnectorDto(
    string Name,
    ConnectorType ConnectorType,
    string ConnectionString,
    string? AdditionalConfig
);

public record TestConnectorResult(
    bool Success,
    string? ErrorMessage,
    DateTime TestedAt
);

// ==================== Transformations ====================
public record TransformationJobDto(
    Guid Id,
    string Name,
    TransformationType TransformationType,
    string TargetTable,
    bool IsActive
);

public record CreateTransformationDto(
    string Name,
    TransformationType TransformationType,
    string SourceQuery,
    string TransformationLogic,
    string TargetTable
);

// ==================== Statistics ====================
public record PipelineStatisticsDto(
    int TotalPipelines,
    int ActivePipelines,
    int RunningPipelines,
    long TotalRecordsProcessed,
    int FailedRunsToday,
    int SuccessfulRunsToday,
    Dictionary<string, int> PipelinesByType
);
