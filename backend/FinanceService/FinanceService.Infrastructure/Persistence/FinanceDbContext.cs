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

        ConfigureAccount(modelBuilder);
        ConfigureTransaction(modelBuilder);
        ConfigureExpense(modelBuilder);
        ConfigureBudget(modelBuilder);
        ConfigureBudgetCategory(modelBuilder);
    }

    private void ConfigureAccount(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DealerId, e.Code }).IsUnique();
            
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.Balance).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.InitialBalance).HasPrecision(18, 2).IsRequired();

            // Self-referencing relationship for account hierarchy
            entity.HasOne(e => e.ParentAccount)
                .WithMany(p => p.ChildAccounts)
                .HasForeignKey(e => e.ParentAccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Primary relationship: Account → Transactions (one-to-many)
            // This maps Account.Transactions to Transaction.Account navigation
            entity.HasMany(e => e.Transactions)
                .WithOne(t => t.Account) // WITH navigation property Account in Transaction
                .HasForeignKey(t => t.AccountId) // Use property-based FK
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureTransaction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DealerId, e.TransactionNumber }).IsUnique();
            entity.HasIndex(e => e.AccountId); // FK index for performance
            entity.HasIndex(e => e.TargetAccountId); // FK index for performance
            
            entity.Property(e => e.TransactionNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.ExchangeRate).HasPrecision(18, 6);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);

            // AccountId FK - Main account relationship configured in ConfigureAccount
            entity.Property(e => e.AccountId).IsRequired();

            // TargetAccountId FK - For transfers ONLY
            // Uses TargetAccount navigation property
            entity.Property(e => e.TargetAccountId).IsRequired(false);
            
            // FK constraint to Account (TargetAccount for transfers)
            // This is a secondary one-to-many without inverse collection
            entity.HasOne(t => t.TargetAccount) // WITH navigation property TargetAccount
                .WithMany() // No inverse collection in Account
                .HasForeignKey(t => t.TargetAccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureExpense(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("Expenses");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DealerId, e.ExpenseNumber }).IsUnique();
            entity.HasIndex(e => e.AccountId); // FK index for performance
            
            entity.Property(e => e.ExpenseNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.Vendor).HasMaxLength(200);
            entity.Property(e => e.VendorTaxId).HasMaxLength(50);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(100);
            entity.Property(e => e.ReceiptUrl).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            // AccountId FK - Optional for Expense
            // Maps to Expense.Account navigation property
            entity.Property(e => e.AccountId).IsRequired(false);
            
            // FK constraint to Account with navigation
            entity.HasOne(e => e.Account) // WITH navigation property Account
                .WithMany() // No inverse collection in Account
                .HasForeignKey(e => e.AccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureBudget(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.ToTable("Budgets");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DealerId, e.Name, e.Year }).IsUnique();
            
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TotalBudget).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.TotalSpent).HasPrecision(18, 2).IsRequired();

            entity.HasMany(e => e.Categories)
                .WithOne(c => c.Budget) // WITH navigation property Budget in BudgetCategory
                .HasForeignKey(c => c.BudgetId) // Use property-based FK
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureBudgetCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BudgetCategory>(entity =>
        {
            entity.ToTable("BudgetCategories");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AllocatedAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.SpentAmount).HasPrecision(18, 2).IsRequired();
        });
    }
}
