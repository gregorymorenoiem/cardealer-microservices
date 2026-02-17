// =====================================================
// AntiMoneyLaunderingService - Repository Interfaces
// Ley 155-17 Prevención de Lavado de Activos (PLD)
// =====================================================

using AntiMoneyLaunderingService.Domain.Entities;

namespace AntiMoneyLaunderingService.Domain.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task<Customer?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Customer>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<Customer>> GetByRiskLevelAsync(string riskLevel);
    Task<IEnumerable<Customer>> GetPepsAsync();
    Task<Customer> AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Guid id);
    Task<int> GetCountAsync();
}

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<Transaction>> GetSuspiciousTransactionsAsync();
    Task<IEnumerable<Transaction>> GetAboveThresholdAsync(decimal threshold);
    Task<Transaction> AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task<int> GetCountAsync();
}

public interface ISuspiciousActivityReportRepository
{
    Task<SuspiciousActivityReport?> GetByIdAsync(Guid id);
    Task<IEnumerable<SuspiciousActivityReport>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<SuspiciousActivityReport>> GetByStatusAsync(string status);
    Task<IEnumerable<SuspiciousActivityReport>> GetPendingSubmissionAsync();
    Task<SuspiciousActivityReport> AddAsync(SuspiciousActivityReport report);
    Task UpdateAsync(SuspiciousActivityReport report);
    Task<int> GetCountAsync();
}

public interface IAmlAlertRepository
{
    Task<AmlAlert?> GetByIdAsync(Guid id);
    Task<IEnumerable<AmlAlert>> GetActiveAlertsAsync();
    Task<IEnumerable<AmlAlert>> GetByCustomerIdAsync(Guid customerId);
    Task<AmlAlert> AddAsync(AmlAlert alert);
    Task UpdateAsync(AmlAlert alert);
    Task<int> GetActiveCountAsync();
}

public interface IKycDocumentRepository
{
    Task<KycDocument?> GetByIdAsync(Guid id);
    Task<IEnumerable<KycDocument>> GetByCustomerIdAsync(Guid customerId);
    Task<KycDocument> AddAsync(KycDocument document);
    Task UpdateAsync(KycDocument document);
    Task DeleteAsync(Guid id);
}
