using CarDealer.Shared.MultiTenancy;
using CRMService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRMService.Infrastructure.Persistence;

public class CRMDbContext : MultiTenantDbContext
{
    private readonly ILogger<CRMDbContext>? _logger;

    public CRMDbContext(DbContextOptions<CRMDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public CRMDbContext(DbContextOptions<CRMDbContext> options, ITenantContext tenantContext, ILogger<CRMDbContext> logger)
        : base(options, tenantContext)
    {
        _logger = logger;
    }

    public DbSet<Lead> Leads { get; set; } = null!;
    public DbSet<Deal> Deals { get; set; } = null!;
    public DbSet<Pipeline> Pipelines { get; set; } = null!;
    public DbSet<Stage> Stages { get; set; } = null!;
    public DbSet<Activity> Activities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _logger?.LogInformation("Configurando DbContext CRMDbContext con proveedor PostgreSQL");

        // Lead Configuration
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("leads");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Company).HasMaxLength(200);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.Source).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.EstimatedValue).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(4000);
            entity.Property(e => e.InterestedProductNotes).HasMaxLength(1000);

            // Tags stored as JSON
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>());

            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.AssignedToUserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Deal Configuration
        modelBuilder.Entity<Deal>(entity =>
        {
            entity.ToTable("deals");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Value).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("MXN");
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.VIN).HasMaxLength(50);
            entity.Property(e => e.LostReason).HasMaxLength(500);
            entity.Property(e => e.WonNotes).HasMaxLength(1000);

            // Tags and CustomFields stored as JSON
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.CustomFields)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            entity.HasOne(e => e.Pipeline)
                .WithMany(p => p.Deals)
                .HasForeignKey(e => e.PipelineId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Stage)
                .WithMany(s => s.Deals)
                .HasForeignKey(e => e.StageId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Lead)
                .WithMany()
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PipelineId);
            entity.HasIndex(e => e.StageId);
            entity.HasIndex(e => e.AssignedToUserId);
            entity.HasIndex(e => e.ExpectedCloseDate);
        });

        // Pipeline Configuration
        modelBuilder.Entity<Pipeline>(entity =>
        {
            entity.ToTable("pipelines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => e.IsDefault);
        });

        // Stage Configuration
        modelBuilder.Entity<Stage>(entity =>
        {
            entity.ToTable("stages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(20);

            entity.HasOne(e => e.Pipeline)
                .WithMany(p => p.Stages)
                .HasForeignKey(e => e.PipelineId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PipelineId, e.Order });
        });

        // Activity Configuration
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.ToTable("activities");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Subject).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Outcome).HasMaxLength(500);
            entity.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Activities)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Deal)
                .WithMany(d => d.Activities)
                .HasForeignKey(e => e.DealId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => e.AssignedToUserId);
            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.DealId);
        });
    }
}
