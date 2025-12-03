using Microsoft.EntityFrameworkCore;
using MessageBusService.Domain.Entities;
using System.Text.Json;

namespace MessageBusService.Infrastructure.Data;

public class MessageBusDbContext : DbContext
{
    public MessageBusDbContext(DbContextOptions<MessageBusDbContext> options) : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageBatch> MessageBatches { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<DeadLetterMessage> DeadLetterMessages { get; set; }
    public DbSet<Saga> Sagas { get; set; }
    public DbSet<SagaStep> SagaSteps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Priority).IsRequired();
            entity.HasIndex(e => e.Topic);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            // Configure Headers as JSON instead of Hstore
            entity.Property(e => e.Headers)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null!));
        });

        modelBuilder.Entity<MessageBatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BatchName).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ConsumerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.QueueName).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Topic);
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<DeadLetterMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.FailureReason).IsRequired();
            entity.HasIndex(e => e.FailedAt);
            entity.HasIndex(e => e.IsDiscarded);
        });

        modelBuilder.Entity<Saga>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CorrelationId).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CorrelationId);
            entity.HasIndex(e => e.CreatedAt);

            // Configure Context as JSON
            entity.Property(e => e.Context)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null!));

            // Navigation
            entity.HasMany(e => e.Steps)
                .WithOne(e => e.Saga)
                .HasForeignKey(e => e.SagaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SagaStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ActionType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ActionPayload).IsRequired();
            entity.Property(e => e.CompensationActionType).HasMaxLength(200);
            entity.Property(e => e.Status).IsRequired();
            entity.HasIndex(e => e.SagaId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.SagaId, e.Order });

            // Configure Metadata as JSON
            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null!));
        });
    }
}
