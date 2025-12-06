using CarDealer.Shared.MultiTenancy;
using InvoicingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoicingService.Infrastructure.Persistence;

public class InvoicingDbContext : MultiTenantDbContext
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CfdiConfiguration> CfdiConfigurations => Set<CfdiConfiguration>();

    public InvoicingDbContext(DbContextOptions<InvoicingDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Invoice
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DueDate);

            entity.Property(e => e.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CustomerEmail).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CustomerTaxId).HasMaxLength(50);
            entity.Property(e => e.CustomerAddress).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.CfdiUuid).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.HasMany(e => e.Items)
                .WithOne(i => i.Invoice)
                .HasForeignKey(i => i.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Payments)
                .WithOne(p => p.Invoice)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // InvoiceItem
        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.ToTable("InvoiceItems");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InvoiceId);

            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.Unit).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);

            entity.Ignore(e => e.DiscountAmount);
            entity.Ignore(e => e.Total);
        });

        // Quote
        modelBuilder.Entity<Quote>(entity =>
        {
            entity.ToTable("Quotes");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QuoteNumber).IsUnique();
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ValidUntil);

            entity.Property(e => e.QuoteNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CustomerEmail).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CustomerPhone).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Currency).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.Terms).HasMaxLength(4000);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);

            entity.HasMany(e => e.Items)
                .WithOne(i => i.Quote)
                .HasForeignKey(i => i.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // QuoteItem
        modelBuilder.Entity<QuoteItem>(entity =>
        {
            entity.ToTable("QuoteItems");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QuoteId);

            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.Unit).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);

            entity.Ignore(e => e.DiscountAmount);
            entity.Ignore(e => e.Total);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PaymentNumber).IsUnique();
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.InvoiceId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PaymentDate);

            entity.Property(e => e.PaymentNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.RefundedAmount).HasPrecision(18, 2);
            entity.Property(e => e.Reference).HasMaxLength(200);
            entity.Property(e => e.TransactionId).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.RefundReason).HasMaxLength(500);
        });

        // CfdiConfiguration
        modelBuilder.Entity<CfdiConfiguration>(entity =>
        {
            entity.ToTable("CfdiConfigurations");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId).IsUnique();
            entity.HasIndex(e => e.Rfc);

            entity.Property(e => e.Rfc).HasMaxLength(20).IsRequired();
            entity.Property(e => e.BusinessName).HasMaxLength(300).IsRequired();
            entity.Property(e => e.TaxRegime).HasMaxLength(10).IsRequired();
            entity.Property(e => e.FiscalAddress).HasMaxLength(500).IsRequired();
            entity.Property(e => e.PostalCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.CertificateNumber).HasMaxLength(50);
            entity.Property(e => e.PacProvider).HasMaxLength(50);
            entity.Property(e => e.PacUsername).HasMaxLength(100);
            entity.Property(e => e.PacPassword).HasMaxLength(200);
            entity.Property(e => e.PrivateKeyPassword).HasMaxLength(200);
            entity.Property(e => e.DefaultSeries).HasMaxLength(10).IsRequired();
        });
    }
}
