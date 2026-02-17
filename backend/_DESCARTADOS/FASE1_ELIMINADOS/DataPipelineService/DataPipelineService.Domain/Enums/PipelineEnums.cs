// =====================================================
// DataPipelineService - Enums
// Procesamiento de Datos y ETL
// =====================================================

namespace DataPipelineService.Domain.Enums;

/// <summary>
/// Tipo de pipeline
/// </summary>
public enum PipelineType
{
    ETL,            // Extract, Transform, Load
    ELT,            // Extract, Load, Transform
    DataSync,       // Sincronización de datos
    DataMigration,  // Migración de datos
    DataExport,     // Exportación
    DataImport,     // Importación
    Aggregation     // Agregación de métricas
}

/// <summary>
/// Estado del pipeline
/// </summary>
public enum PipelineStatus
{
    Draft,          // Borrador
    Active,         // Activo
    Paused,         // Pausado
    Disabled,       // Deshabilitado
    Archived        // Archivado
}

/// <summary>
/// Tipo de paso
/// </summary>
public enum StepType
{
    Extract,        // Extracción
    Transform,      // Transformación
    Load,           // Carga
    Validate,       // Validación
    Filter,         // Filtrado
    Aggregate,      // Agregación
    Join,           // Unión de datos
    Custom          // Personalizado
}

/// <summary>
/// Estado de ejecución
/// </summary>
public enum RunStatus
{
    Queued,         // En cola
    Running,        // Ejecutando
    Completed,      // Completado
    Failed,         // Fallido
    Cancelled,      // Cancelado
    PartialSuccess  // Éxito parcial
}

/// <summary>
/// Nivel de log
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Tipo de conector
/// </summary>
public enum ConnectorType
{
    PostgreSQL,
    MySQL,
    SqlServer,
    MongoDB,
    Redis,
    RabbitMQ,
    RestApi,
    GraphQL,
    S3,
    LocalFile,
    SFTP
}

/// <summary>
/// Tipo de transformación
/// </summary>
public enum TransformationType
{
    Map,            // Mapeo de campos
    Filter,         // Filtrado
    Aggregate,      // Agregación
    Deduplicate,    // Deduplicación
    Enrich,         // Enriquecimiento
    Normalize,      // Normalización
    Custom          // Personalizado
}
