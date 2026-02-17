using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BankReconciliationService.Infrastructure.Persistence;

public class BankReconciliationDbContext : DbContext
{
    public BankReconciliationDbContext(DbContextOptions<BankReconciliationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<BankStatement> BankStatements { get; set; } = null!;
    public DbSet<BankStatementLine> BankStatementLines { get; set; } = null!;
    public DbSet<InternalTransaction> InternalTransactions { get; set; } = null!;
    public DbSet<Reconciliation> Reconciliations { get; set; } = null!;
    public DbSet<ReconciliationMatch> ReconciliationMatches { get; set; } = null!;
    public DbSet<ReconciliationDiscrepancy> ReconciliationDiscrepancies { get; set; } = null!;
    public DbSet<BankAccountConfig> BankAccountConfigs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankReconciliationDbContext).Assembly);

        // Set default schema
        modelBuilder.HasDefaultSchema("bank_reconciliation");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Add audit fields automatically
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is BankStatement statement)
            {
                if (entry.State == EntityState.Added)
                {
                    statement.ImportedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is Reconciliation reconciliation)
            {
                if (entry.State == EntityState.Added)
                {
                    reconciliation.ReconciliationDate = DateTime.UtcNow;
                }
                if (entry.State == EntityState.Modified && reconciliation.Status == ReconciliationStatus.Completed)
                {
                    reconciliation.CompletedAt = DateTime.UtcNow;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
