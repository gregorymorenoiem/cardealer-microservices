using Microsoft.EntityFrameworkCore;
using AzulPaymentService.Domain.Entities;

namespace AzulPaymentService.Infrastructure.Persistence;

/// <summary>
/// DbContext para AZUL Payment Service
/// </summary>
public class AzulDbContext : DbContext
{
    public AzulDbContext(DbContextOptions<AzulDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Transacciones de pago
    /// </summary>
    public DbSet<AzulTransaction> AzulTransactions { get; set; } = null!;

    /// <summary>
    /// Suscripciones recurrentes
    /// </summary>
    public DbSet<AzulSubscription> AzulSubscriptions { get; set; } = null!;

    /// <summary>
    /// Eventos de webhook de AZUL
    /// </summary>
    public DbSet<AzulWebhookEvent> AzulWebhookEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de AzulTransaction
        modelBuilder.Entity<AzulTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.AzulTransactionId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);

            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("DOP");

            entity.Property(e => e.Status)
                .IsRequired();

            entity.Property(e => e.PaymentMethod)
                .IsRequired();

            entity.Property(e => e.TransactionType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ResponseCode)
                .HasMaxLength(10);

            entity.Property(e => e.ResponseMessage)
                .HasMaxLength(500);

            entity.Property(e => e.AuthorizationCode)
                .HasMaxLength(50);

            entity.Property(e => e.CardToken)
                .HasMaxLength(255);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Índices para búsquedas comunes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.AzulTransactionId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configuración de AzulSubscription
        modelBuilder.Entity<AzulSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.AzulSubscriptionId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);

            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("DOP");

            entity.Property(e => e.Frequency)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CardToken)
                .HasMaxLength(255);

            entity.Property(e => e.TotalAmountCharged)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            entity.Property(e => e.PlanName)
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Índices
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.AzulSubscriptionId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.NextChargeDate);
        });

        // Configuración de AzulWebhookEvent
        modelBuilder.Entity<AzulWebhookEvent>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.AzulEventId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PayloadJson)
                .IsRequired();

            entity.Property(e => e.Signature)
                .HasMaxLength(255);

            entity.Property(e => e.ProcessingResult)
                .HasMaxLength(500);

            entity.Property(e => e.ProcessingError)
                .HasMaxLength(500);

            entity.Property(e => e.ReceivedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.SenderIpAddress)
                .HasMaxLength(45);

            // Índices
            entity.HasIndex(e => e.AzulEventId).IsUnique();
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.IsProcessed);
            entity.HasIndex(e => e.ReceivedAt);
        });
    }
}
