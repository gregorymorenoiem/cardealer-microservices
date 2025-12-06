using Microsoft.EntityFrameworkCore;
using CarDealer.Shared.MultiTenancy;
using FinanceService.Domain.Entities;

namespace FinanceService.Infrastructure.Persistence;

public class FinanceDbContext : MultiTenantDbContext
{
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetCategory> BudgetCategories => Set<BudgetCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DealerId, e.Code }).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.InitialBalance).HasPrecision(18, 2);

            entity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DealerId, e.TransactionNumber }).IsUnique();
            entity.Property(e => e.TransactionNumber).HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.ExchangeRate).HasPrecision(18, 6);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);

            entity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.TargetAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DealerId, e.ExpenseNumber }).IsUnique();
            entity.Property(e => e.ExpenseNumber).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Vendor).HasMaxLength(200);
            entity.Property(e => e.VendorTaxId).HasMaxLength(50);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(100);
            entity.Property(e => e.ReceiptUrl).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DealerId, e.Name, e.Year }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TotalBudget).HasPrecision(18, 2);
            entity.Property(e => e.TotalSpent).HasPrecision(18, 2);

            entity.HasMany(e => e.Categories)
                .WithOne()
                .HasForeignKey(c => c.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BudgetCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.AllocatedAmount).HasPrecision(18, 2);
            entity.Property(e => e.SpentAmount).HasPrecision(18, 2);
        });
    }
}
