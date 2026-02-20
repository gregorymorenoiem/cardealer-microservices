using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence;

/// <summary>
/// DbContext para Payment Service (Multi-Provider)
/// Soporta AZUL, CardNET, PixelPay, Fygaro, PayPal + Tasas de Cambio BCRD
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

    /// <summary>
    /// Tasas de cambio del Banco Central RD
    /// Requerido por DGII para transacciones en moneda extranjera
    /// </summary>
    public DbSet<ExchangeRate> ExchangeRates { get; set; } = null!;

    /// <summary>
    /// Registros de conversión de moneda (auditoría DGII)
    /// </summary>
    public DbSet<CurrencyConversion> CurrencyConversions { get; set; } = null!;

    /// <summary>
    /// Métodos de pago guardados (tarjetas tokenizadas)
    /// </summary>
    public DbSet<SavedPaymentMethod> SavedPaymentMethods { get; set; } = null!;

    /// <summary>
    /// Transacciones multi-gateway (genérico)
    /// </summary>
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;

    /// <summary>
    /// Facturas generadas (DGII-compliant)
    /// </summary>
    public DbSet<Invoice> Invoices { get; set; } = null!;

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

        // ==================== EXCHANGE RATES (BCRD - DGII Compliance) ====================
        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.SourceCurrency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(e => e.TargetCurrency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("DOP");

            entity.Property(e => e.BuyRate)
                .HasPrecision(18, 6);

            entity.Property(e => e.SellRate)
                .HasPrecision(18, 6);

            entity.Property(e => e.Source)
                .IsRequired();

            entity.Property(e => e.BcrdReferenceId)
                .HasMaxLength(100);

            entity.Property(e => e.FetchedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Índice único: solo una tasa activa por moneda/fecha
            entity.HasIndex(e => new { e.SourceCurrency, e.RateDate, e.IsActive })
                .HasFilter("\"IsActive\" = true")
                .IsUnique();

            entity.HasIndex(e => e.RateDate);
            entity.HasIndex(e => e.SourceCurrency);
        });

        // ==================== CURRENCY CONVERSIONS (Auditoría DGII) ====================
        modelBuilder.Entity<CurrencyConversion>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.OriginalCurrency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(e => e.OriginalAmount)
                .HasPrecision(18, 2);

            entity.Property(e => e.ConvertedAmountDop)
                .HasPrecision(18, 2);

            entity.Property(e => e.AppliedRate)
                .HasPrecision(18, 6);

            entity.Property(e => e.RateSource)
                .IsRequired();

            entity.Property(e => e.ConversionType)
                .IsRequired();

            entity.Property(e => e.ItbisDop)
                .HasPrecision(18, 2);

            entity.Property(e => e.TotalWithItbisDop)
                .HasPrecision(18, 2);

            entity.Property(e => e.Ncf)
                .HasMaxLength(19); // Formato: B0100000001

            entity.Property(e => e.Notes)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relación con ExchangeRate
            entity.HasOne(e => e.ExchangeRate)
                .WithMany()
                .HasForeignKey(e => e.ExchangeRateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            entity.HasIndex(e => e.PaymentTransactionId);
            entity.HasIndex(e => e.ExchangeRateId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Ncf);
        });

        // ==================== SAVED PAYMENT METHODS (Tarjetas Tokenizadas) ====================
        modelBuilder.Entity<SavedPaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.PaymentGateway)
                .IsRequired();

            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Type)
                .IsRequired();

            entity.Property(e => e.NickName)
                .HasMaxLength(100);

            entity.Property(e => e.CardBrand)
                .HasMaxLength(50);

            entity.Property(e => e.CardLast4)
                .HasMaxLength(4);

            entity.Property(e => e.CardHolderName)
                .HasMaxLength(200);

            entity.Property(e => e.BankCountry)
                .HasMaxLength(2);

            entity.Property(e => e.BankName)
                .HasMaxLength(200);

            entity.Property(e => e.AccountLast4)
                .HasMaxLength(4);

            entity.Property(e => e.AccountType)
                .HasMaxLength(20);

            entity.Property(e => e.AccountBankName)
                .HasMaxLength(200);

            entity.Property(e => e.BillingAddressJson)
                .HasColumnType("jsonb");

            entity.Property(e => e.ExternalReference)
                .HasMaxLength(255);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Índices
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.IsDefault })
                .HasFilter("\"IsDefault\" = true");
            entity.HasIndex(e => new { e.Token, e.PaymentGateway }).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // ==================== PAYMENT TRANSACTIONS (Multi-Gateway) ====================
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.GatewayTransactionId).HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("DOP");
            entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ResponseCode).HasMaxLength(10);
            entity.Property(e => e.ResponseMessage).HasMaxLength(500);
            entity.Property(e => e.AuthorizationCode).HasMaxLength(50);
            entity.Property(e => e.CommissionAmount).HasPrecision(18, 4);
            entity.Property(e => e.CommissionRate).HasPrecision(8, 4);
            entity.Property(e => e.NetAmount).HasPrecision(18, 2);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.GatewayTransactionId);
            entity.HasIndex(e => e.Gateway);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // ==================== INVOICES (DGII-Compliant) ====================
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Ncf).HasMaxLength(19);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 4);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("DOP");
            entity.Property(e => e.ExchangeRate).HasPrecision(18, 6);
            entity.Property(e => e.AmountInDop).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.LineItemsJson).HasColumnType("jsonb");
            entity.Property(e => e.BuyerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BuyerEmail).IsRequired().HasMaxLength(254);
            entity.Property(e => e.BuyerRnc).HasMaxLength(11);
            entity.Property(e => e.BuyerAddress).HasMaxLength(500);
            entity.Property(e => e.BuyerPhone).HasMaxLength(20);
            entity.Property(e => e.SellerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SellerRnc).HasMaxLength(11);
            entity.Property(e => e.SellerAddress).HasMaxLength(500);
            entity.Property(e => e.PdfUrl).HasMaxLength(500);
            entity.Property(e => e.PdfStorageKey).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CorrelationId).HasMaxLength(50);
            entity.Property(e => e.IssuedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => e.Ncf).IsUnique().HasFilter("\"Ncf\" IS NOT NULL");
            entity.HasIndex(e => e.PaymentTransactionId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.IssuedAt);
            entity.HasIndex(e => e.Status);
        });
    }
}
