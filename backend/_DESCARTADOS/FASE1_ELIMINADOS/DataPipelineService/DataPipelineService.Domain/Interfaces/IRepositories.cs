// =====================================================
// DataPipelineService - Interfaces
// Procesamiento de Datos y ETL
// =====================================================

using DataPipelineService.Domain.Entities;
using DataPipelineService.Domain.Enums;

namespace DataPipelineService.Domain.Interfaces;

public interface IDataPipelineRepository
{
    Task<DataPipeline?> GetByIdAsync(Guid id);
    Task<DataPipeline?> GetByNameAsync(string name);
    Task<IEnumerable<DataPipeline>> GetAllAsync();
    Task<IEnumerable<DataPipeline>> GetActiveAsync();
    Task<IEnumerable<DataPipeline>> GetScheduledAsync();
    Task<DataPipeline> AddAsync(DataPipeline pipeline);
    Task UpdateAsync(DataPipeline pipeline);
    Task DeleteAsync(Guid id);
    Task<int> GetCountAsync();
}

public interface IPipelineStepRepository
{
    Task<PipelineStep?> GetByIdAsync(Guid id);
    Task<IEnumerable<PipelineStep>> GetByPipelineIdAsync(Guid pipelineId);
    Task<PipelineStep> AddAsync(PipelineStep step);
    Task UpdateAsync(PipelineStep step);
    Task DeleteAsync(Guid id);
    Task ReorderStepsAsync(Guid pipelineId, Dictionary<Guid, int> newOrder);
}

public interface IPipelineRunRepository
{
    Task<PipelineRun?> GetByIdAsync(Guid id);
    Task<IEnumerable<PipelineRun>> GetByPipelineIdAsync(Guid pipelineId, int limit = 50);
    Task<PipelineRun?> GetLatestByPipelineIdAsync(Guid pipelineId);
    Task<IEnumerable<PipelineRun>> GetRunningAsync();
    Task<IEnumerable<PipelineRun>> GetByStatusAsync(RunStatus status);
    Task<PipelineRun> AddAsync(PipelineRun run);
    Task UpdateAsync(PipelineRun run);
}

public interface IRunLogRepository
{
    Task<IEnumerable<RunLog>> GetByRunIdAsync(Guid runId);
    Task<RunLog> AddAsync(RunLog log);
    Task AddRangeAsync(IEnumerable<RunLog> logs);
}

public interface IDataConnectorRepository
{
    Task<DataConnector?> GetByIdAsync(Guid id);
    Task<DataConnector?> GetByNameAsync(string name);
    Task<IEnumerable<DataConnector>> GetAllAsync();
    Task<IEnumerable<DataConnector>> GetByTypeAsync(ConnectorType type);
    Task<DataConnector> AddAsync(DataConnector connector);
    Task UpdateAsync(DataConnector connector);
    Task DeleteAsync(Guid id);
}

public interface ITransformationJobRepository
{
    Task<TransformationJob?> GetByIdAsync(Guid id);
    Task<IEnumerable<TransformationJob>> GetAllAsync();
    Task<IEnumerable<TransformationJob>> GetActiveAsync();
    Task<TransformationJob> AddAsync(TransformationJob job);
    Task UpdateAsync(TransformationJob job);
    Task DeleteAsync(Guid id);
}
