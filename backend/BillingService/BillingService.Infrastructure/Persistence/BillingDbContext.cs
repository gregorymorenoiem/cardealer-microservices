using Microsoft.EntityFrameworkCore;
using BillingService.Domain.Entities;

namespace BillingService.Infrastructure.Persistence;

public class BillingDbContext : DbContext
{
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<StripeCustomer> StripeCustomers => Set<StripeCustomer>();
    public DbSet<EarlyBirdMember> EarlyBirdMembers => Set<EarlyBirdMember>();

    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Plan);
            entity.HasIndex(e => e.StripeCustomerId);
            entity.HasIndex(e => e.StripeSubscriptionId);

            entity.Property(e => e.PricePerCycle).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.StripeCustomerId).HasMaxLength(100);
            entity.Property(e => e.StripeSubscriptionId).HasMaxLength(100);
            entity.Property(e => e.StripePaymentMethodId).HasMaxLength(100);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.InvoiceId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StripePaymentIntentId);

            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.RefundedAmount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.StripePaymentIntentId).HasMaxLength(100);
            entity.Property(e => e.StripeChargeId).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ReceiptUrl).HasMaxLength(500);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.RefundReason).HasMaxLength(500);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StripeInvoiceId);

            entity.Property(e => e.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.StripeInvoiceId).HasMaxLength(100);
            entity.Property(e => e.PdfUrl).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);
        });

        modelBuilder.Entity<StripeCustomer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId).IsUnique();
            entity.HasIndex(e => e.StripeCustomerId).IsUnique();

            entity.Property(e => e.StripeCustomerId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Metadata).HasMaxLength(4000); // JSON
            entity.Property(e => e.DefaultPaymentMethodId).HasMaxLength(100);
        });

        modelBuilder.Entity<EarlyBirdMember>(entity =>
        {
            entity.ToTable("early_bird_members");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.HasUsedBenefit);
            entity.HasIndex(e => e.FreeUntil);

            entity.Property(e => e.Id)
                .HasColumnName("Id");

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnName("UserId");

            entity.Property(e => e.EnrolledAt)
                .IsRequired()
                .HasColumnName("EnrolledAt");

            entity.Property(e => e.FreeUntil)
                .IsRequired()
                .HasColumnName("FreeUntil");

            entity.Property(e => e.HasUsedBenefit)
                .IsRequired()
                .HasColumnName("HasUsedBenefit")
                .HasDefaultValue(false);

            entity.Property(e => e.BenefitUsedAt)
                .HasColumnName("BenefitUsedAt");

            entity.Property(e => e.SubscriptionIdWhenUsed)
                .HasMaxLength(100)
                .HasColumnName("SubscriptionIdWhenUsed");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnName("CreatedAt")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
