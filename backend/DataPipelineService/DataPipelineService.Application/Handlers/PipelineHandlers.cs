// =====================================================
// DataPipelineService - Handlers
// Procesamiento de Datos y ETL
// =====================================================

using MediatR;
using DataPipelineService.Application.Commands;
using DataPipelineService.Application.Queries;
using DataPipelineService.Application.DTOs;
using DataPipelineService.Domain.Entities;
using DataPipelineService.Domain.Interfaces;
using DataPipelineService.Domain.Enums;

namespace DataPipelineService.Application.Handlers;

// ==================== Pipeline Handlers ====================

public class CreatePipelineHandler : IRequestHandler<CreatePipelineCommand, DataPipelineDto>
{
    private readonly IDataPipelineRepository _repository;

    public CreatePipelineHandler(IDataPipelineRepository repository) => _repository = repository;

    public async Task<DataPipelineDto> Handle(CreatePipelineCommand request, CancellationToken ct)
    {
        var pipeline = new DataPipeline
        {
            Id = Guid.NewGuid(),
            Name = request.Data.Name,
            Description = request.Data.Description,
            Type = request.Data.Type,
            Status = PipelineStatus.Draft,
            SourceType = request.Data.SourceType,
            SourceConfig = request.Data.SourceConfig,
            DestinationType = request.Data.DestinationType,
            DestinationConfig = request.Data.DestinationConfig,
            TransformationScript = request.Data.TransformationScript,
            CronSchedule = request.Data.CronSchedule,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        await _repository.AddAsync(pipeline);

        return new DataPipelineDto(
            pipeline.Id, pipeline.Name, pipeline.Description, pipeline.Type,
            pipeline.Status, pipeline.SourceType, pipeline.DestinationType,
            pipeline.IsActive, pipeline.LastRunAt, pipeline.NextRunAt, 0
        );
    }
}

public class GetPipelineByIdHandler : IRequestHandler<GetPipelineByIdQuery, DataPipelineDetailDto?>
{
    private readonly IDataPipelineRepository _pipelineRepo;
    private readonly IPipelineStepRepository _stepRepo;
    private readonly IPipelineRunRepository _runRepo;

    public GetPipelineByIdHandler(
        IDataPipelineRepository pipelineRepo,
        IPipelineStepRepository stepRepo,
        IPipelineRunRepository runRepo)
    {
        _pipelineRepo = pipelineRepo;
        _stepRepo = stepRepo;
        _runRepo = runRepo;
    }

    public async Task<DataPipelineDetailDto?> Handle(GetPipelineByIdQuery request, CancellationToken ct)
    {
        var pipeline = await _pipelineRepo.GetByIdAsync(request.Id);
        if (pipeline == null) return null;

        var steps = await _stepRepo.GetByPipelineIdAsync(pipeline.Id);
        var runs = await _runRepo.GetByPipelineIdAsync(pipeline.Id, 10);

        return new DataPipelineDetailDto(
            pipeline.Id, pipeline.Name, pipeline.Description, pipeline.Type,
            pipeline.Status, pipeline.SourceType, pipeline.SourceConfig,
            pipeline.DestinationType, pipeline.DestinationConfig,
            pipeline.TransformationScript, pipeline.CronSchedule,
            pipeline.IsActive, pipeline.LastRunAt, pipeline.NextRunAt,
            steps.Select(s => new PipelineStepDto(s.Id, s.StepOrder, s.Name, s.StepType, s.Configuration, s.IsActive)),
            runs.Select(r => new PipelineRunSummaryDto(r.Id, r.Status, r.StartedAt, r.CompletedAt, r.RecordsProcessed, r.TriggeredBy))
        );
    }
}

public class GetAllPipelinesHandler : IRequestHandler<GetAllPipelinesQuery, IEnumerable<DataPipelineDto>>
{
    private readonly IDataPipelineRepository _repository;

    public GetAllPipelinesHandler(IDataPipelineRepository repository) => _repository = repository;

    public async Task<IEnumerable<DataPipelineDto>> Handle(GetAllPipelinesQuery request, CancellationToken ct)
    {
        var pipelines = await _repository.GetAllAsync();
        return pipelines.Select(p => new DataPipelineDto(
            p.Id, p.Name, p.Description, p.Type, p.Status,
            p.SourceType, p.DestinationType, p.IsActive,
            p.LastRunAt, p.NextRunAt, p.Steps?.Count ?? 0
        ));
    }
}

public class StartPipelineRunHandler : IRequestHandler<StartPipelineRunCommand, PipelineRunDto>
{
    private readonly IDataPipelineRepository _pipelineRepo;
    private readonly IPipelineRunRepository _runRepo;

    public StartPipelineRunHandler(IDataPipelineRepository pipelineRepo, IPipelineRunRepository runRepo)
    {
        _pipelineRepo = pipelineRepo;
        _runRepo = runRepo;
    }

    public async Task<PipelineRunDto> Handle(StartPipelineRunCommand request, CancellationToken ct)
    {
        var pipeline = await _pipelineRepo.GetByIdAsync(request.Data.PipelineId);
        if (pipeline == null) throw new KeyNotFoundException("Pipeline no encontrado");

        var run = new PipelineRun
        {
            Id = Guid.NewGuid(),
            DataPipelineId = pipeline.Id,
            Status = RunStatus.Running,
            StartedAt = DateTime.UtcNow,
            RecordsProcessed = 0,
            RecordsSuccess = 0,
            RecordsFailed = 0,
            TriggeredBy = request.Data.TriggeredBy ?? "Manual",
            CreatedAt = DateTime.UtcNow
        };

        await _runRepo.AddAsync(run);

        pipeline.LastRunAt = DateTime.UtcNow;
        pipeline.UpdatedAt = DateTime.UtcNow;
        await _pipelineRepo.UpdateAsync(pipeline);

        return new PipelineRunDto(
            run.Id, run.DataPipelineId, run.Status, run.StartedAt, run.CompletedAt,
            run.RecordsProcessed, run.RecordsSuccess, run.RecordsFailed,
            run.ErrorMessage, run.TriggeredBy, Enumerable.Empty<RunLogDto>()
        );
    }
}

// ==================== Connector Handlers ====================

public class CreateConnectorHandler : IRequestHandler<CreateConnectorCommand, DataConnectorDto>
{
    private readonly IDataConnectorRepository _repository;

    public CreateConnectorHandler(IDataConnectorRepository repository) => _repository = repository;

    public async Task<DataConnectorDto> Handle(CreateConnectorCommand request, CancellationToken ct)
    {
        var connector = new DataConnector
        {
            Id = Guid.NewGuid(),
            Name = request.Data.Name,
            ConnectorType = request.Data.ConnectorType,
            ConnectionString = request.Data.ConnectionString, // En prod: encriptar
            AdditionalConfig = request.Data.AdditionalConfig,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(connector);

        return new DataConnectorDto(
            connector.Id, connector.Name, connector.ConnectorType,
            connector.IsActive, connector.LastTestedAt, connector.LastTestSuccess
        );
    }
}

public class GetAllConnectorsHandler : IRequestHandler<GetAllConnectorsQuery, IEnumerable<DataConnectorDto>>
{
    private readonly IDataConnectorRepository _repository;

    public GetAllConnectorsHandler(IDataConnectorRepository repository) => _repository = repository;

    public async Task<IEnumerable<DataConnectorDto>> Handle(GetAllConnectorsQuery request, CancellationToken ct)
    {
        var connectors = await _repository.GetAllAsync();
        return connectors.Select(c => new DataConnectorDto(
            c.Id, c.Name, c.ConnectorType, c.IsActive, c.LastTestedAt, c.LastTestSuccess
        ));
    }
}

// ==================== Statistics Handler ====================

public class GetPipelineStatisticsHandler : IRequestHandler<GetPipelineStatisticsQuery, PipelineStatisticsDto>
{
    private readonly IDataPipelineRepository _pipelineRepo;
    private readonly IPipelineRunRepository _runRepo;

    public GetPipelineStatisticsHandler(IDataPipelineRepository pipelineRepo, IPipelineRunRepository runRepo)
    {
        _pipelineRepo = pipelineRepo;
        _runRepo = runRepo;
    }

    public async Task<PipelineStatisticsDto> Handle(GetPipelineStatisticsQuery request, CancellationToken ct)
    {
        var total = await _pipelineRepo.GetCountAsync();
        var active = await _pipelineRepo.GetActiveAsync();
        var running = await _runRepo.GetRunningAsync();

        return new PipelineStatisticsDto(
            TotalPipelines: total,
            ActivePipelines: active.Count(),
            RunningPipelines: running.Count(),
            TotalRecordsProcessed: 0,
            FailedRunsToday: 0,
            SuccessfulRunsToday: 0,
            PipelinesByType: new Dictionary<string, int>()
        );
    }
}
