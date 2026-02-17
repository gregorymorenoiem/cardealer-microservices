// =====================================================
// DataPipelineService - Entities
// Procesamiento de Datos y ETL
// =====================================================

namespace DataPipelineService.Domain.Entities;

/// <summary>
/// Pipeline de datos
/// </summary>
public class DataPipeline
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Enums.PipelineType Type { get; set; }
    public Enums.PipelineStatus Status { get; set; }
    public string SourceType { get; set; } = string.Empty; // Database, API, File, Queue
    public string SourceConfig { get; set; } = string.Empty; // JSON config
    public string DestinationType { get; set; } = string.Empty;
    public string DestinationConfig { get; set; } = string.Empty;
    public string? TransformationScript { get; set; }
    public string? CronSchedule { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    
    public ICollection<PipelineRun> Runs { get; set; } = new List<PipelineRun>();
    public ICollection<PipelineStep> Steps { get; set; } = new List<PipelineStep>();
}

/// <summary>
/// Paso dentro del pipeline
/// </summary>
public class PipelineStep
{
    public Guid Id { get; set; }
    public Guid DataPipelineId { get; set; }
    public int StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public Enums.StepType StepType { get; set; }
    public string Configuration { get; set; } = string.Empty; // JSON
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public DataPipeline? DataPipeline { get; set; }
}

/// <summary>
/// Ejecución de un pipeline
/// </summary>
public class PipelineRun
{
    public Guid Id { get; set; }
    public Guid DataPipelineId { get; set; }
    public Enums.RunStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long RecordsProcessed { get; set; }
    public long RecordsSuccess { get; set; }
    public long RecordsFailed { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RunMetadata { get; set; } // JSON con métricas
    public string TriggeredBy { get; set; } = string.Empty; // Schedule, Manual, API
    public DateTime CreatedAt { get; set; }
    
    public DataPipeline? DataPipeline { get; set; }
    public ICollection<RunLog> Logs { get; set; } = new List<RunLog>();
}

/// <summary>
/// Logs de ejecución
/// </summary>
public class RunLog
{
    public Guid Id { get; set; }
    public Guid PipelineRunId { get; set; }
    public Enums.LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int? StepOrder { get; set; }
    public DateTime Timestamp { get; set; }
    
    public PipelineRun? PipelineRun { get; set; }
}

/// <summary>
/// Conector de datos
/// </summary>
public class DataConnector
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Enums.ConnectorType ConnectorType { get; set; }
    public string ConnectionString { get; set; } = string.Empty; // Encriptado
    public string? AdditionalConfig { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public bool? LastTestSuccess { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Job de transformación
/// </summary>
public class TransformationJob
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Enums.TransformationType TransformationType { get; set; }
    public string SourceQuery { get; set; } = string.Empty;
    public string TransformationLogic { get; set; } = string.Empty;
    public string TargetTable { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
