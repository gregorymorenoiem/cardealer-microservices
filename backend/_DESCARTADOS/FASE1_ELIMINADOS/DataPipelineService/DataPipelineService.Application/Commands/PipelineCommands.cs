// =====================================================
// DataPipelineService - Commands
// Procesamiento de Datos y ETL
// =====================================================

using MediatR;
using DataPipelineService.Application.DTOs;

namespace DataPipelineService.Application.Commands;

// ==================== Pipelines ====================
public record CreatePipelineCommand(CreatePipelineDto Data) : IRequest<DataPipelineDto>;
public record UpdatePipelineCommand(Guid Id, UpdatePipelineDto Data) : IRequest<bool>;
public record DeletePipelineCommand(Guid Id) : IRequest<bool>;
public record ActivatePipelineCommand(Guid Id) : IRequest<bool>;
public record DeactivatePipelineCommand(Guid Id) : IRequest<bool>;

// ==================== Steps ====================
public record AddStepCommand(CreateStepDto Data) : IRequest<PipelineStepDto>;
public record UpdateStepCommand(Guid Id, CreateStepDto Data) : IRequest<bool>;
public record DeleteStepCommand(Guid Id) : IRequest<bool>;
public record ReorderStepsCommand(Guid PipelineId, Dictionary<Guid, int> NewOrder) : IRequest<bool>;

// ==================== Runs ====================
public record StartPipelineRunCommand(StartRunDto Data) : IRequest<PipelineRunDto>;
public record CancelPipelineRunCommand(Guid RunId) : IRequest<bool>;

// ==================== Connectors ====================
public record CreateConnectorCommand(CreateConnectorDto Data) : IRequest<DataConnectorDto>;
public record UpdateConnectorCommand(Guid Id, CreateConnectorDto Data) : IRequest<bool>;
public record DeleteConnectorCommand(Guid Id) : IRequest<bool>;
public record TestConnectorCommand(Guid Id) : IRequest<TestConnectorResult>;

// ==================== Transformations ====================
public record CreateTransformationCommand(CreateTransformationDto Data) : IRequest<TransformationJobDto>;
public record UpdateTransformationCommand(Guid Id, CreateTransformationDto Data) : IRequest<bool>;
public record DeleteTransformationCommand(Guid Id) : IRequest<bool>;
public record ExecuteTransformationCommand(Guid Id) : IRequest<bool>;
