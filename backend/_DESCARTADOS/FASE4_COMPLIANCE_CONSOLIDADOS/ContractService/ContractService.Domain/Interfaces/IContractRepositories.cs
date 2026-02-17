// ContractService - Domain Interfaces (Consistent with Repositories)

namespace ContractService.Domain.Interfaces;

using ContractService.Domain.Entities;

/// <summary>
/// Repositorio de plantillas de contrato
/// </summary>
public interface IContractTemplateRepository
{
    Task<ContractTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ContractTemplate?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<List<ContractTemplate>> GetByTypeAsync(ContractType type, CancellationToken ct = default);
    Task<List<ContractTemplate>> GetActiveTemplatesAsync(CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
    Task AddAsync(ContractTemplate template, CancellationToken ct = default);
    Task UpdateAsync(ContractTemplate template, CancellationToken ct = default);
}

/// <summary>
/// Repositorio de contratos
/// </summary>
public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Contract?> GetByContractNumberAsync(string contractNumber, CancellationToken ct = default);
    Task<List<Contract>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<List<Contract>> GetBySubjectAsync(string subjectType, Guid subjectId, CancellationToken ct = default);
    Task<(List<Contract> Items, int Total)> GetByStatusAsync(ContractStatus status, int page, int pageSize, CancellationToken ct = default);
    Task<List<Contract>> GetExpiringContractsAsync(int daysUntilExpiration, CancellationToken ct = default);
    Task<string> GenerateContractNumberAsync(CancellationToken ct = default);
    Task AddAsync(Contract contract, CancellationToken ct = default);
    Task UpdateAsync(Contract contract, CancellationToken ct = default);
}

/// <summary>
/// Repositorio de partes del contrato
/// </summary>
public interface IContractPartyRepository
{
    Task<ContractParty?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ContractParty>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default);
    Task<ContractParty?> GetByUserAndContractAsync(Guid userId, Guid contractId, CancellationToken ct = default);
    Task AddAsync(ContractParty party, CancellationToken ct = default);
    Task UpdateAsync(ContractParty party, CancellationToken ct = default);
}

/// <summary>
/// Repositorio de firmas digitales - Ley 126-02
/// </summary>
public interface IContractSignatureRepository
{
    Task<ContractSignature?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ContractSignature>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default);
    Task<List<ContractSignature>> GetPendingSignaturesAsync(Guid userId, CancellationToken ct = default);
    Task<bool> AllPartiesSignedAsync(Guid contractId, CancellationToken ct = default);
    Task AddAsync(ContractSignature signature, CancellationToken ct = default);
    Task UpdateAsync(ContractSignature signature, CancellationToken ct = default);
}

/// <summary>
/// Repositorio de cláusulas de contrato
/// </summary>
public interface IContractClauseRepository
{
    Task<ContractClause?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ContractClause>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default);
    Task AddAsync(ContractClause clause, CancellationToken ct = default);
    Task AddRangeAsync(List<ContractClause> clauses, CancellationToken ct = default);
    Task UpdateAsync(ContractClause clause, CancellationToken ct = default);
}

/// <summary>
/// Repositorio de versiones de contrato
/// </summary>
public interface IContractVersionRepository
{
    Task<List<ContractVersion>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default);
    Task<ContractVersion?> GetLatestVersionAsync(Guid contractId, CancellationToken ct = default);
    Task AddAsync(ContractVersion version, CancellationToken ct = default);
}

/// <summary>
/// Repositorio de documentos adjuntos
/// </summary>
public interface IContractDocumentRepository
{
    Task<ContractDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ContractDocument>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default);
    Task AddAsync(ContractDocument document, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Repositorio de log de auditoría
/// </summary>
public interface IContractAuditLogRepository
{
    Task<List<ContractAuditLog>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default);
    Task<List<ContractAuditLog>> GetByUserAsync(string userId, DateTime from, DateTime to, CancellationToken ct = default);
    Task AddAsync(ContractAuditLog auditLog, CancellationToken ct = default);
}

/// <summary>
/// Repositorio de autoridades de certificación
/// </summary>
public interface ICertificationAuthorityRepository
{
    Task<CertificationAuthority?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CertificationAuthority?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<List<CertificationAuthority>> GetAllAsync(CancellationToken ct = default);
    Task<List<CertificationAuthority>> GetActiveAsync(CancellationToken ct = default);
}
