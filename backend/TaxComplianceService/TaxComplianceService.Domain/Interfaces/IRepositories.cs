// =====================================================
// TaxComplianceService - Repository Interfaces
// Ley 11-92 Código Tributario de República Dominicana
// =====================================================

using TaxComplianceService.Domain.Entities;

namespace TaxComplianceService.Domain.Interfaces;

public interface ITaxDeclarationRepository
{
    Task<TaxDeclaration?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaxDeclaration>> GetByTaxpayerIdAsync(Guid taxpayerId);
    Task<IEnumerable<TaxDeclaration>> GetByPeriodAsync(string period);
    Task<IEnumerable<TaxDeclaration>> GetPendingDeclarationsAsync();
    Task<IEnumerable<TaxDeclaration>> GetOverdueDeclarationsAsync();
    Task<TaxDeclaration> AddAsync(TaxDeclaration declaration);
    Task UpdateAsync(TaxDeclaration declaration);
    Task<int> GetCountAsync();
}

public interface ITaxpayerRepository
{
    Task<Taxpayer?> GetByIdAsync(Guid id);
    Task<Taxpayer?> GetByRncAsync(string rnc);
    Task<IEnumerable<Taxpayer>> GetAllAsync(int page, int pageSize);
    Task<Taxpayer> AddAsync(Taxpayer taxpayer);
    Task UpdateAsync(Taxpayer taxpayer);
    Task DeleteAsync(Guid id);
    Task<int> GetCountAsync();
}

public interface ITaxPaymentRepository
{
    Task<TaxPayment?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaxPayment>> GetByDeclarationIdAsync(Guid declarationId);
    Task<IEnumerable<TaxPayment>> GetPendingPaymentsAsync();
    Task<TaxPayment> AddAsync(TaxPayment payment);
    Task UpdateAsync(TaxPayment payment);
}

public interface INcfSequenceRepository
{
    Task<NcfSequence?> GetByIdAsync(Guid id);
    Task<IEnumerable<NcfSequence>> GetByTaxpayerIdAsync(Guid taxpayerId);
    Task<NcfSequence?> GetActiveByTypeAsync(Guid taxpayerId, string ncfType);
    Task<NcfSequence> AddAsync(NcfSequence sequence);
    Task UpdateAsync(NcfSequence sequence);
    Task<string> GetNextNcfAsync(Guid taxpayerId, string ncfType);
}

public interface IReporte606Repository
{
    Task<IEnumerable<Reporte606Item>> GetByDeclarationIdAsync(Guid declarationId);
    Task<Reporte606Item> AddAsync(Reporte606Item item);
    Task AddRangeAsync(IEnumerable<Reporte606Item> items);
}

public interface IReporte607Repository
{
    Task<IEnumerable<Reporte607Item>> GetByDeclarationIdAsync(Guid declarationId);
    Task<Reporte607Item> AddAsync(Reporte607Item item);
    Task AddRangeAsync(IEnumerable<Reporte607Item> items);
}
