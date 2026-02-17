using DataProtectionService.Domain.Entities;

namespace DataProtectionService.Domain.Interfaces;

public interface IUserConsentRepository
{
    Task<UserConsent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserConsent?> GetActiveConsentAsync(Guid userId, string type, CancellationToken cancellationToken = default);
    Task<List<UserConsent>> GetByUserIdAsync(Guid userId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task AddAsync(UserConsent consent, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserConsent consent, CancellationToken cancellationToken = default);
    Task<bool> HasActiveConsentAsync(Guid userId, string type, CancellationToken cancellationToken = default);
    Task<int> GetConsentCountAsync(Guid userId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetConsentStatsByTypeAsync(CancellationToken cancellationToken = default);
}

public interface IARCORequestRepository
{
    Task<ARCORequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ARCORequest?> GetByRequestNumberAsync(string requestNumber, CancellationToken cancellationToken = default);
    Task<List<ARCORequest>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(List<ARCORequest> Items, int TotalCount)> GetPendingRequestsAsync(int page, int pageSize, bool overdueOnly = false, CancellationToken cancellationToken = default);
    Task<List<ARCORequest>> GetOverdueRequestsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ARCORequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(ARCORequest request, CancellationToken cancellationToken = default);
    Task<ARCOStatisticsResult> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
}

public class ARCOStatisticsResult
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int OverdueRequests { get; set; }
    public double AverageProcessingDays { get; set; }
    public Dictionary<string, int> RequestsByType { get; set; } = new();
    public Dictionary<string, int> RequestsByStatus { get; set; } = new();
    public double ComplianceRate { get; set; }
}

public interface IDataChangeLogRepository
{
    Task<DataChangeLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<DataChangeLog>> GetByUserIdAsync(Guid userId, int limit = 100, CancellationToken cancellationToken = default);
    Task<List<DataChangeLog>> GetByCategoryAsync(Guid userId, string category, CancellationToken cancellationToken = default);
    Task<List<DataChangeLog>> GetByFieldAsync(Guid userId, string field, int limit = 10, CancellationToken cancellationToken = default);
    Task AddAsync(DataChangeLog log, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<DataChangeLog> logs, CancellationToken cancellationToken = default);
}

public interface IPrivacyPolicyRepository
{
    Task<PrivacyPolicy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PrivacyPolicy?> GetCurrentPolicyAsync(string language = "es", CancellationToken cancellationToken = default);
    Task<PrivacyPolicy?> GetByVersionAsync(string version, string language = "es", CancellationToken cancellationToken = default);
    Task<List<PrivacyPolicy>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task AddAsync(PrivacyPolicy policy, CancellationToken cancellationToken = default);
    Task UpdateAsync(PrivacyPolicy policy, CancellationToken cancellationToken = default);
}

public interface IAnonymizationRecordRepository
{
    Task<AnonymizationRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AnonymizationRecord?> GetByOriginalUserIdAsync(Guid originalUserId, CancellationToken cancellationToken = default);
    Task<bool> IsUserAnonymizedAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<AnonymizationRecord>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task CreateAsync(AnonymizationRecord record, CancellationToken cancellationToken = default);
    Task<List<AnonymizationRecord>> GetExpiredRecordsAsync(CancellationToken cancellationToken = default);
}

public interface IDataExportRepository
{
    Task<DataExport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<DataExport>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<DataExport>> GetPendingExportsAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(DataExport export, CancellationToken cancellationToken = default);
    Task UpdateAsync(DataExport export, CancellationToken cancellationToken = default);
}
