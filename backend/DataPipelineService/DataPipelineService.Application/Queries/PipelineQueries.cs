// =====================================================
// DataPipelineService - Queries
// Procesamiento de Datos y ETL
// =====================================================

using MediatR;
using DataPipelineService.Application.DTOs;
using DataPipelineService.Domain.Enums;

namespace DataPipelineService.Application.Queries;

// ==================== Pipelines ====================
public record GetPipelineByIdQuery(Guid Id) : IRequest<DataPipelineDetailDto?>;
public record GetPipelineByNameQuery(string Name) : IRequest<DataPipelineDetailDto?>;
public record GetAllPipelinesQuery() : IRequest<IEnumerable<DataPipelineDto>>;
public record GetActivePipelinesQuery() : IRequest<IEnumerable<DataPipelineDto>>;
public record GetScheduledPipelinesQuery() : IRequest<IEnumerable<DataPipelineDto>>;

// ==================== Steps ====================
public record GetStepsByPipelineQuery(Guid PipelineId) : IRequest<IEnumerable<PipelineStepDto>>;

// ==================== Runs ====================
public record GetRunByIdQuery(Guid Id) : IRequest<PipelineRunDto?>;
public record GetRunsByPipelineQuery(Guid PipelineId, int Limit = 50) : IRequest<IEnumerable<PipelineRunSummaryDto>>;
public record GetRunningPipelinesQuery() : IRequest<IEnumerable<PipelineRunDto>>;
public record GetRunsByStatusQuery(RunStatus Status) : IRequest<IEnumerable<PipelineRunSummaryDto>>;

// ==================== Connectors ====================
public record GetConnectorByIdQuery(Guid Id) : IRequest<DataConnectorDto?>;
public record GetAllConnectorsQuery() : IRequest<IEnumerable<DataConnectorDto>>;
public record GetConnectorsByTypeQuery(ConnectorType Type) : IRequest<IEnumerable<DataConnectorDto>>;

// ==================== Transformations ====================
public record GetTransformationByIdQuery(Guid Id) : IRequest<TransformationJobDto?>;
public record GetAllTransformationsQuery() : IRequest<IEnumerable<TransformationJobDto>>;
public record GetActiveTransformationsQuery() : IRequest<IEnumerable<TransformationJobDto>>;

// ==================== Statistics ====================
public record GetPipelineStatisticsQuery() : IRequest<PipelineStatisticsDto>;
