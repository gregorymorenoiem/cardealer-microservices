using Microsoft.EntityFrameworkCore;
using StripePaymentService.Domain.Entities;

namespace StripePaymentService.Infrastructure.Persistence;

/// <summary>
/// DbContext para Stripe Payment Service
/// </summary>
public class StripeDbContext : DbContext
{
    public DbSet<StripePaymentIntent> PaymentIntents { get; set; }
    public DbSet<StripeCustomer> Customers { get; set; }
    public DbSet<StripeSubscription> Subscriptions { get; set; }
    public DbSet<StripePaymentMethod> PaymentMethods { get; set; }
    public DbSet<StripeInvoice> Invoices { get; set; }

    public StripeDbContext(DbContextOptions<StripeDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Payment Intent
        modelBuilder.Entity<StripePaymentIntent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StripePaymentIntentId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ClientSecret).HasMaxLength(500);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CustomerEmail).HasMaxLength(255);
            entity.Property(e => e.CustomerName).HasMaxLength(255);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            
            entity.HasIndex(e => e.StripePaymentIntentId).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Customer
        modelBuilder.Entity<StripeCustomer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StripeCustomerId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(2);

            entity.HasIndex(e => e.StripeCustomerId).IsUnique();
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.IsActive);

            // Relaciones
            entity.HasMany(e => e.PaymentMethods)
                .WithOne(pm => pm.Customer)
                .HasForeignKey(pm => pm.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Subscriptions)
                .WithOne(s => s.Customer)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Subscription
        modelBuilder.Entity<StripeSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StripeSubscriptionId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BillingInterval).HasMaxLength(50);

            entity.HasIndex(e => e.StripeSubscriptionId).IsUnique();
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.BillingCycleAnchor);

            // Relación con Customer
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Subscriptions)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con Invoices
            entity.HasMany(e => e.Invoices)
                .WithOne(i => i.Subscription)
                .HasForeignKey(i => i.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Payment Method
        modelBuilder.Entity<StripePaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StripePaymentMethodId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Brand).HasMaxLength(50);
            entity.Property(e => e.Last4).HasMaxLength(4);

            entity.HasIndex(e => e.StripePaymentMethodId).IsUnique();
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.IsDefault);

            // Relación con Customer
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.PaymentMethods)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Invoice
        modelBuilder.Entity<StripeInvoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StripeInvoiceId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PdfUrl).HasMaxLength(1000);
            entity.Property(e => e.HostedInvoiceUrl).HasMaxLength(1000);

            entity.HasIndex(e => e.StripeInvoiceId).IsUnique();
            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            // Relación con Subscription
            entity.HasOne(e => e.Subscription)
                .WithMany(s => s.Invoices)
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
