// =====================================================
// AntiMoneyLaunderingService - Repositories
// Ley 155-17 Prevención de Lavado de Activos (PLD)
// =====================================================

using Microsoft.EntityFrameworkCore;
using AntiMoneyLaunderingService.Domain.Entities;
using AntiMoneyLaunderingService.Domain.Interfaces;
using AntiMoneyLaunderingService.Domain.Enums;
using AntiMoneyLaunderingService.Infrastructure.Persistence;

namespace AntiMoneyLaunderingService.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AmlDbContext _context;

    public CustomerRepository(AmlDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
        => await _context.Customers.FindAsync(id);

    public async Task<Customer?> GetByUserIdAsync(Guid userId)
        => await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<IEnumerable<Customer>> GetAllAsync(int page, int pageSize)
        => await _context.Customers
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<IEnumerable<Customer>> GetByRiskLevelAsync(string riskLevel)
    {
        var level = Enum.Parse<RiskLevel>(riskLevel);
        return await _context.Customers.Where(c => c.RiskLevel == level).ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetPepsAsync()
        => await _context.Customers.Where(c => c.IsPep).ToListAsync();

    public async Task<Customer> AddAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        _context.Entry(customer).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetCountAsync()
        => await _context.Customers.CountAsync();
}

public class TransactionRepository : ITransactionRepository
{
    private readonly AmlDbContext _context;

    public TransactionRepository(AmlDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
        => await _context.Transactions.FindAsync(id);

    public async Task<IEnumerable<Transaction>> GetByCustomerIdAsync(Guid customerId)
        => await _context.Transactions.Where(t => t.CustomerId == customerId).ToListAsync();

    public async Task<IEnumerable<Transaction>> GetSuspiciousTransactionsAsync()
        => await _context.Transactions.Where(t => t.IsSuspicious).ToListAsync();

    public async Task<IEnumerable<Transaction>> GetAboveThresholdAsync(decimal threshold)
        => await _context.Transactions.Where(t => t.Amount >= threshold).ToListAsync();

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _context.Entry(transaction).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
        => await _context.Transactions.CountAsync();
}

public class SuspiciousActivityReportRepository : ISuspiciousActivityReportRepository
{
    private readonly AmlDbContext _context;

    public SuspiciousActivityReportRepository(AmlDbContext context)
    {
        _context = context;
    }

    public async Task<SuspiciousActivityReport?> GetByIdAsync(Guid id)
        => await _context.SuspiciousActivityReports.FindAsync(id);

    public async Task<IEnumerable<SuspiciousActivityReport>> GetAllAsync(int page, int pageSize)
        => await _context.SuspiciousActivityReports
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<IEnumerable<SuspiciousActivityReport>> GetByStatusAsync(string status)
    {
        var rosStatus = Enum.Parse<RosStatus>(status);
        return await _context.SuspiciousActivityReports.Where(r => r.Status == rosStatus).ToListAsync();
    }

    public async Task<IEnumerable<SuspiciousActivityReport>> GetPendingSubmissionAsync()
        => await _context.SuspiciousActivityReports
            .Where(r => r.Status == RosStatus.Pending || r.Status == RosStatus.UnderReview)
            .ToListAsync();

    public async Task<SuspiciousActivityReport> AddAsync(SuspiciousActivityReport report)
    {
        _context.SuspiciousActivityReports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task UpdateAsync(SuspiciousActivityReport report)
    {
        _context.Entry(report).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
        => await _context.SuspiciousActivityReports.CountAsync();
}

public class AmlAlertRepository : IAmlAlertRepository
{
    private readonly AmlDbContext _context;

    public AmlAlertRepository(AmlDbContext context)
    {
        _context = context;
    }

    public async Task<AmlAlert?> GetByIdAsync(Guid id)
        => await _context.AmlAlerts.FindAsync(id);

    public async Task<IEnumerable<AmlAlert>> GetActiveAlertsAsync()
        => await _context.AmlAlerts.Where(a => a.Status == AlertStatus.Active).ToListAsync();

    public async Task<IEnumerable<AmlAlert>> GetByCustomerIdAsync(Guid customerId)
        => await _context.AmlAlerts.Where(a => a.CustomerId == customerId).ToListAsync();

    public async Task<AmlAlert> AddAsync(AmlAlert alert)
    {
        _context.AmlAlerts.Add(alert);
        await _context.SaveChangesAsync();
        return alert;
    }

    public async Task UpdateAsync(AmlAlert alert)
    {
        _context.Entry(alert).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetActiveCountAsync()
        => await _context.AmlAlerts.CountAsync(a => a.Status == AlertStatus.Active);
}

public class KycDocumentRepository : IKycDocumentRepository
{
    private readonly AmlDbContext _context;

    public KycDocumentRepository(AmlDbContext context)
    {
        _context = context;
    }

    public async Task<KycDocument?> GetByIdAsync(Guid id)
        => await _context.KycDocuments.FindAsync(id);

    public async Task<IEnumerable<KycDocument>> GetByCustomerIdAsync(Guid customerId)
        => await _context.KycDocuments.Where(d => d.CustomerId == customerId).ToListAsync();

    public async Task<KycDocument> AddAsync(KycDocument document)
    {
        _context.KycDocuments.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task UpdateAsync(KycDocument document)
    {
        _context.Entry(document).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var document = await _context.KycDocuments.FindAsync(id);
        if (document != null)
        {
            _context.KycDocuments.Remove(document);
            await _context.SaveChangesAsync();
        }
    }
}
