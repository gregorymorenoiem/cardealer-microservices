using Microsoft.EntityFrameworkCore;
using RateLimitingService.Core.Models;

namespace RateLimitingService.Infrastructure.Data;

/// <summary>
/// Database context for rate limiting violations and audit
/// </summary>
public class RateLimitDbContext : DbContext
{
    public RateLimitDbContext(DbContextOptions<RateLimitDbContext> options)
        : base(options)
    {
    }

    public DbSet<RateLimitViolation> Violations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RateLimitViolation>(entity =>
        {
            entity.ToTable("rate_limit_violations");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Identifier)
                .HasColumnName("identifier")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.IdentifierType)
                .HasColumnName("identifier_type")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Endpoint)
                .HasColumnName("endpoint")
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(e => e.RuleId)
                .HasColumnName("rule_id")
                .HasMaxLength(100);

            entity.Property(e => e.RuleName)
                .HasColumnName("rule_name")
                .HasMaxLength(200);

            entity.Property(e => e.AllowedLimit)
                .HasColumnName("allowed_limit")
                .IsRequired();

            entity.Property(e => e.AttemptedRequests)
                .HasColumnName("attempted_requests")
                .IsRequired();

            entity.Property(e => e.ViolatedAt)
                .HasColumnName("violated_at")
                .IsRequired();

            // Ignore computed properties
            entity.Ignore(e => e.WindowSize);
            entity.Ignore(e => e.Timestamp);
            entity.Ignore(e => e.Reason);
            entity.Ignore(e => e.Limit);

            // Indexes for common queries
            entity.HasIndex(e => e.Identifier)
                .HasDatabaseName("idx_violations_identifier");

            entity.HasIndex(e => e.IdentifierType)
                .HasDatabaseName("idx_violations_identifier_type");

            entity.HasIndex(e => e.ViolatedAt)
                .HasDatabaseName("idx_violations_violated_at");

            entity.HasIndex(e => new { e.Identifier, e.ViolatedAt })
                .HasDatabaseName("idx_violations_identifier_date");

            entity.HasIndex(e => new { e.IdentifierType, e.ViolatedAt })
                .HasDatabaseName("idx_violations_type_date");
        });
    }
}
