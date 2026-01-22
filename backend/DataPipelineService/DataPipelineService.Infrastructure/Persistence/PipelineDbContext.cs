// =====================================================
// DataPipelineService - DbContext
// Procesamiento de Datos y ETL
// =====================================================

using Microsoft.EntityFrameworkCore;
using DataPipelineService.Domain.Entities;

namespace DataPipelineService.Infrastructure.Persistence;

public class PipelineDbContext : DbContext
{
    public PipelineDbContext(DbContextOptions<PipelineDbContext> options) : base(options) { }

    public DbSet<DataPipeline> DataPipelines => Set<DataPipeline>();
    public DbSet<PipelineStep> PipelineSteps => Set<PipelineStep>();
    public DbSet<PipelineRun> PipelineRuns => Set<PipelineRun>();
    public DbSet<RunLog> RunLogs => Set<RunLog>();
    public DbSet<DataConnector> DataConnectors => Set<DataConnector>();
    public DbSet<TransformationJob> TransformationJobs => Set<TransformationJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DataPipeline
        modelBuilder.Entity<DataPipeline>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Status);

            entity.HasMany(e => e.Steps)
                  .WithOne(s => s.DataPipeline)
                  .HasForeignKey(s => s.DataPipelineId);

            entity.HasMany(e => e.Runs)
                  .WithOne(r => r.DataPipeline)
                  .HasForeignKey(r => r.DataPipelineId);
        });

        // PipelineStep
        modelBuilder.Entity<PipelineStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DataPipelineId, e.StepOrder });
        });

        // PipelineRun
        modelBuilder.Entity<PipelineRun>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DataPipelineId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartedAt);

            entity.HasMany(e => e.Logs)
                  .WithOne(l => l.PipelineRun)
                  .HasForeignKey(l => l.PipelineRunId);
        });

        // RunLog
        modelBuilder.Entity<RunLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PipelineRunId);
            entity.HasIndex(e => e.Timestamp);
        });

        // DataConnector
        modelBuilder.Entity<DataConnector>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // TransformationJob
        modelBuilder.Entity<TransformationJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TargetTable).HasMaxLength(100).IsRequired();
        });
    }
}
