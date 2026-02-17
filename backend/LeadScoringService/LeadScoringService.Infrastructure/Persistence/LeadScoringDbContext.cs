using LeadScoringService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeadScoringService.Infrastructure.Persistence;

public class LeadScoringDbContext : DbContext
{
    public LeadScoringDbContext(DbContextOptions<LeadScoringDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadAction> LeadActions => Set<LeadAction>();
    public DbSet<LeadScoreHistory> LeadScoreHistory => Set<LeadScoreHistory>();
    public DbSet<ScoringRule> ScoringRules => Set<ScoringRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Lead
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("leads");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.UserEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.UserFullName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.UserPhone).HasMaxLength(50);
            entity.Property(e => e.VehicleTitle).HasMaxLength(500).IsRequired();
            entity.Property(e => e.VehiclePrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DealerName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ConversionProbability).HasColumnType("decimal(5,2)");
            entity.Property(e => e.DealerNotes).HasMaxLength(2000);
            
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.VehicleId);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.Score);
            entity.HasIndex(e => e.Temperature);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.UserId, e.VehicleId });
            entity.HasIndex(e => new { e.DealerId, e.Temperature });
            entity.HasIndex(e => e.LastInteractionAt);
        });

        // LeadAction
        modelBuilder.Entity<LeadAction>(entity =>
        {
            entity.ToTable("lead_actions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Metadata).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            
            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Actions)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.ActionType);
            entity.HasIndex(e => e.OccurredAt);
        });

        // LeadScoreHistory
        modelBuilder.Entity<LeadScoreHistory>(entity =>
        {
            entity.ToTable("lead_score_history");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Reason).HasMaxLength(500).IsRequired();
            
            entity.HasOne(e => e.Lead)
                .WithMany(l => l.ScoreHistory)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.TriggeringAction)
                .WithMany()
                .HasForeignKey(e => e.TriggeringActionId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.ChangedAt);
        });

        // ScoringRule
        modelBuilder.Entity<ScoringRule>(entity =>
        {
            entity.ToTable("scoring_rules");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Configuration).HasMaxLength(4000).IsRequired();
            
            entity.HasIndex(e => e.RuleType);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
        });
    }
}
