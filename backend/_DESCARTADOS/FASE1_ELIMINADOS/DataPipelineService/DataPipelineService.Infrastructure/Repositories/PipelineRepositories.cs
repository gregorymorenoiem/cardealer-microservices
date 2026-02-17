// =====================================================
// DataPipelineService - Repositories
// Procesamiento de Datos y ETL
// =====================================================

using Microsoft.EntityFrameworkCore;
using DataPipelineService.Domain.Entities;
using DataPipelineService.Domain.Interfaces;
using DataPipelineService.Domain.Enums;
using DataPipelineService.Infrastructure.Persistence;

namespace DataPipelineService.Infrastructure.Repositories;

public class DataPipelineRepository : IDataPipelineRepository
{
    private readonly PipelineDbContext _context;

    public DataPipelineRepository(PipelineDbContext context) => _context = context;

    public async Task<DataPipeline?> GetByIdAsync(Guid id) =>
        await _context.DataPipelines
            .Include(p => p.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<DataPipeline?> GetByNameAsync(string name) =>
        await _context.DataPipelines.FirstOrDefaultAsync(p => p.Name == name);

    public async Task<IEnumerable<DataPipeline>> GetAllAsync() =>
        await _context.DataPipelines.Include(p => p.Steps).OrderBy(p => p.Name).ToListAsync();

    public async Task<IEnumerable<DataPipeline>> GetActiveAsync() =>
        await _context.DataPipelines.Where(p => p.IsActive).ToListAsync();

    public async Task<IEnumerable<DataPipeline>> GetScheduledAsync() =>
        await _context.DataPipelines
            .Where(p => p.IsActive && !string.IsNullOrEmpty(p.CronSchedule))
            .ToListAsync();

    public async Task<DataPipeline> AddAsync(DataPipeline pipeline)
    {
        await _context.DataPipelines.AddAsync(pipeline);
        await _context.SaveChangesAsync();
        return pipeline;
    }

    public async Task UpdateAsync(DataPipeline pipeline)
    {
        _context.DataPipelines.Update(pipeline);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var pipeline = await _context.DataPipelines.FindAsync(id);
        if (pipeline != null)
        {
            _context.DataPipelines.Remove(pipeline);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetCountAsync() => await _context.DataPipelines.CountAsync();
}

public class PipelineStepRepository : IPipelineStepRepository
{
    private readonly PipelineDbContext _context;

    public PipelineStepRepository(PipelineDbContext context) => _context = context;

    public async Task<PipelineStep?> GetByIdAsync(Guid id) =>
        await _context.PipelineSteps.FindAsync(id);

    public async Task<IEnumerable<PipelineStep>> GetByPipelineIdAsync(Guid pipelineId) =>
        await _context.PipelineSteps
            .Where(s => s.DataPipelineId == pipelineId)
            .OrderBy(s => s.StepOrder)
            .ToListAsync();

    public async Task<PipelineStep> AddAsync(PipelineStep step)
    {
        var maxOrder = await _context.PipelineSteps
            .Where(s => s.DataPipelineId == step.DataPipelineId)
            .MaxAsync(s => (int?)s.StepOrder) ?? 0;
        step.StepOrder = maxOrder + 1;
        
        await _context.PipelineSteps.AddAsync(step);
        await _context.SaveChangesAsync();
        return step;
    }

    public async Task UpdateAsync(PipelineStep step)
    {
        _context.PipelineSteps.Update(step);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var step = await _context.PipelineSteps.FindAsync(id);
        if (step != null)
        {
            _context.PipelineSteps.Remove(step);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ReorderStepsAsync(Guid pipelineId, Dictionary<Guid, int> newOrder)
    {
        var steps = await _context.PipelineSteps.Where(s => s.DataPipelineId == pipelineId).ToListAsync();
        foreach (var step in steps)
        {
            if (newOrder.TryGetValue(step.Id, out var order))
                step.StepOrder = order;
        }
        await _context.SaveChangesAsync();
    }
}

public class PipelineRunRepository : IPipelineRunRepository
{
    private readonly PipelineDbContext _context;

    public PipelineRunRepository(PipelineDbContext context) => _context = context;

    public async Task<PipelineRun?> GetByIdAsync(Guid id) =>
        await _context.PipelineRuns.Include(r => r.Logs).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<PipelineRun>> GetByPipelineIdAsync(Guid pipelineId, int limit = 50) =>
        await _context.PipelineRuns
            .Where(r => r.DataPipelineId == pipelineId)
            .OrderByDescending(r => r.StartedAt)
            .Take(limit)
            .ToListAsync();

    public async Task<PipelineRun?> GetLatestByPipelineIdAsync(Guid pipelineId) =>
        await _context.PipelineRuns
            .Where(r => r.DataPipelineId == pipelineId)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<PipelineRun>> GetRunningAsync() =>
        await _context.PipelineRuns.Where(r => r.Status == RunStatus.Running).ToListAsync();

    public async Task<IEnumerable<PipelineRun>> GetByStatusAsync(RunStatus status) =>
        await _context.PipelineRuns.Where(r => r.Status == status).ToListAsync();

    public async Task<PipelineRun> AddAsync(PipelineRun run)
    {
        await _context.PipelineRuns.AddAsync(run);
        await _context.SaveChangesAsync();
        return run;
    }

    public async Task UpdateAsync(PipelineRun run)
    {
        _context.PipelineRuns.Update(run);
        await _context.SaveChangesAsync();
    }
}

public class RunLogRepository : IRunLogRepository
{
    private readonly PipelineDbContext _context;

    public RunLogRepository(PipelineDbContext context) => _context = context;

    public async Task<IEnumerable<RunLog>> GetByRunIdAsync(Guid runId) =>
        await _context.RunLogs
            .Where(l => l.PipelineRunId == runId)
            .OrderBy(l => l.Timestamp)
            .ToListAsync();

    public async Task<RunLog> AddAsync(RunLog log)
    {
        await _context.RunLogs.AddAsync(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task AddRangeAsync(IEnumerable<RunLog> logs)
    {
        await _context.RunLogs.AddRangeAsync(logs);
        await _context.SaveChangesAsync();
    }
}

public class DataConnectorRepository : IDataConnectorRepository
{
    private readonly PipelineDbContext _context;

    public DataConnectorRepository(PipelineDbContext context) => _context = context;

    public async Task<DataConnector?> GetByIdAsync(Guid id) =>
        await _context.DataConnectors.FindAsync(id);

    public async Task<DataConnector?> GetByNameAsync(string name) =>
        await _context.DataConnectors.FirstOrDefaultAsync(c => c.Name == name);

    public async Task<IEnumerable<DataConnector>> GetAllAsync() =>
        await _context.DataConnectors.OrderBy(c => c.Name).ToListAsync();

    public async Task<IEnumerable<DataConnector>> GetByTypeAsync(ConnectorType type) =>
        await _context.DataConnectors.Where(c => c.ConnectorType == type).ToListAsync();

    public async Task<DataConnector> AddAsync(DataConnector connector)
    {
        await _context.DataConnectors.AddAsync(connector);
        await _context.SaveChangesAsync();
        return connector;
    }

    public async Task UpdateAsync(DataConnector connector)
    {
        _context.DataConnectors.Update(connector);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var connector = await _context.DataConnectors.FindAsync(id);
        if (connector != null)
        {
            _context.DataConnectors.Remove(connector);
            await _context.SaveChangesAsync();
        }
    }
}

public class TransformationJobRepository : ITransformationJobRepository
{
    private readonly PipelineDbContext _context;

    public TransformationJobRepository(PipelineDbContext context) => _context = context;

    public async Task<TransformationJob?> GetByIdAsync(Guid id) =>
        await _context.TransformationJobs.FindAsync(id);

    public async Task<IEnumerable<TransformationJob>> GetAllAsync() =>
        await _context.TransformationJobs.OrderBy(t => t.Name).ToListAsync();

    public async Task<IEnumerable<TransformationJob>> GetActiveAsync() =>
        await _context.TransformationJobs.Where(t => t.IsActive).ToListAsync();

    public async Task<TransformationJob> AddAsync(TransformationJob job)
    {
        await _context.TransformationJobs.AddAsync(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task UpdateAsync(TransformationJob job)
    {
        _context.TransformationJobs.Update(job);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var job = await _context.TransformationJobs.FindAsync(id);
        if (job != null)
        {
            _context.TransformationJobs.Remove(job);
            await _context.SaveChangesAsync();
        }
    }
}
