using Microsoft.EntityFrameworkCore;
using SchedulerService.Domain.Entities;
using SchedulerService.Domain.Enums;
using System.Text.Json;

namespace SchedulerService.Infrastructure.Data;

public class SchedulerDbContext : DbContext
{
    public SchedulerDbContext(DbContextOptions<SchedulerDbContext> options) : base(options)
    {
    }

    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobExecution> JobExecutions => Set<JobExecution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Job configuration
        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("jobs");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();
            
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();
            
            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);
            
            entity.Property(e => e.JobType)
                .HasColumnName("job_type")
                .HasMaxLength(500)
                .IsRequired();
            
            entity.Property(e => e.CronExpression)
                .HasColumnName("cron_expression")
                .HasMaxLength(100)
                .IsRequired();
            
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<int>()
                .IsRequired();
            
            entity.Property(e => e.RetryCount)
                .HasColumnName("retry_count")
                .HasDefaultValue(3);
            
            entity.Property(e => e.TimeoutSeconds)
                .HasColumnName("timeout_seconds")
                .HasDefaultValue(300);
            
            entity.Property(e => e.Parameters)
                .HasColumnName("parameters")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                );
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
            
            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(100);
            
            entity.Property(e => e.LastExecutionAt)
                .HasColumnName("last_execution_at");
            
            entity.Property(e => e.NextExecutionAt)
                .HasColumnName("next_execution_at");

            entity.HasMany(e => e.Executions)
                .WithOne(e => e.Job)
                .HasForeignKey(e => e.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.NextExecutionAt);
        });

        // JobExecution configuration
        modelBuilder.Entity<JobExecution>(entity =>
        {
            entity.ToTable("job_executions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();
            
            entity.Property(e => e.JobId)
                .HasColumnName("job_id")
                .IsRequired();
            
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<int>()
                .IsRequired();
            
            entity.Property(e => e.ScheduledAt)
                .HasColumnName("scheduled_at")
                .IsRequired();
            
            entity.Property(e => e.StartedAt)
                .HasColumnName("started_at");
            
            entity.Property(e => e.CompletedAt)
                .HasColumnName("completed_at");
            
            entity.Property(e => e.AttemptNumber)
                .HasColumnName("attempt_number")
                .HasDefaultValue(1);
            
            entity.Property(e => e.ErrorMessage)
                .HasColumnName("error_message")
                .HasMaxLength(2000);
            
            entity.Property(e => e.StackTrace)
                .HasColumnName("stack_trace")
                .HasColumnType("text");
            
            entity.Property(e => e.Result)
                .HasColumnName("result")
                .HasColumnType("text");
            
            entity.Property(e => e.DurationMs)
                .HasColumnName("duration_ms");
            
            entity.Property(e => e.ExecutedBy)
                .HasColumnName("executed_by")
                .HasMaxLength(100);

            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledAt);
            entity.HasIndex(e => new { e.JobId, e.ScheduledAt });
        });
    }
}
