using Microsoft.EntityFrameworkCore;
using MessageBusService.Domain.Entities;

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
    }
}
