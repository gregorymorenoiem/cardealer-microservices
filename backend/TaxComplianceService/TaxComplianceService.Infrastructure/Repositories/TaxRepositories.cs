// =====================================================
// TaxComplianceService - Repositories
// Ley 11-92 Código Tributario de República Dominicana
// =====================================================

using Microsoft.EntityFrameworkCore;
using TaxComplianceService.Domain.Entities;
using TaxComplianceService.Domain.Interfaces;
using TaxComplianceService.Domain.Enums;
using TaxComplianceService.Infrastructure.Persistence;

namespace TaxComplianceService.Infrastructure.Repositories;

public class TaxpayerRepository : ITaxpayerRepository
{
    private readonly TaxDbContext _context;

    public TaxpayerRepository(TaxDbContext context) => _context = context;

    public async Task<Taxpayer?> GetByIdAsync(Guid id)
        => await _context.Taxpayers.FindAsync(id);

    public async Task<Taxpayer?> GetByRncAsync(string rnc)
        => await _context.Taxpayers.FirstOrDefaultAsync(t => t.Rnc == rnc);

    public async Task<IEnumerable<Taxpayer>> GetAllAsync(int page, int pageSize)
        => await _context.Taxpayers
            .OrderBy(t => t.BusinessName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<Taxpayer> AddAsync(Taxpayer taxpayer)
    {
        _context.Taxpayers.Add(taxpayer);
        await _context.SaveChangesAsync();
        return taxpayer;
    }

    public async Task UpdateAsync(Taxpayer taxpayer)
    {
        _context.Entry(taxpayer).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var taxpayer = await _context.Taxpayers.FindAsync(id);
        if (taxpayer != null)
        {
            _context.Taxpayers.Remove(taxpayer);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetCountAsync()
        => await _context.Taxpayers.CountAsync();
}

public class TaxDeclarationRepository : ITaxDeclarationRepository
{
    private readonly TaxDbContext _context;

    public TaxDeclarationRepository(TaxDbContext context) => _context = context;

    public async Task<TaxDeclaration?> GetByIdAsync(Guid id)
        => await _context.TaxDeclarations.FindAsync(id);

    public async Task<IEnumerable<TaxDeclaration>> GetByTaxpayerIdAsync(Guid taxpayerId)
        => await _context.TaxDeclarations
            .Where(d => d.TaxpayerId == taxpayerId)
            .OrderByDescending(d => d.Period)
            .ToListAsync();

    public async Task<IEnumerable<TaxDeclaration>> GetByPeriodAsync(string period)
        => await _context.TaxDeclarations
            .Where(d => d.Period == period)
            .ToListAsync();

    public async Task<IEnumerable<TaxDeclaration>> GetPendingDeclarationsAsync()
        => await _context.TaxDeclarations
            .Where(d => d.Status == DeclarationStatus.Draft || d.Status == DeclarationStatus.Pending)
            .ToListAsync();

    public async Task<IEnumerable<TaxDeclaration>> GetOverdueDeclarationsAsync()
        => await _context.TaxDeclarations
            .Where(d => d.DueDate < DateTime.UtcNow && d.Status != DeclarationStatus.Accepted)
            .ToListAsync();

    public async Task<TaxDeclaration> AddAsync(TaxDeclaration declaration)
    {
        _context.TaxDeclarations.Add(declaration);
        await _context.SaveChangesAsync();
        return declaration;
    }

    public async Task UpdateAsync(TaxDeclaration declaration)
    {
        _context.Entry(declaration).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
        => await _context.TaxDeclarations.CountAsync();
}

public class TaxPaymentRepository : ITaxPaymentRepository
{
    private readonly TaxDbContext _context;

    public TaxPaymentRepository(TaxDbContext context) => _context = context;

    public async Task<TaxPayment?> GetByIdAsync(Guid id)
        => await _context.TaxPayments.FindAsync(id);

    public async Task<IEnumerable<TaxPayment>> GetByDeclarationIdAsync(Guid declarationId)
        => await _context.TaxPayments
            .Where(p => p.TaxDeclarationId == declarationId)
            .ToListAsync();

    public async Task<IEnumerable<TaxPayment>> GetPendingPaymentsAsync()
        => await _context.TaxPayments
            .Where(p => p.Status == PaymentStatus.Pending)
            .ToListAsync();

    public async Task<TaxPayment> AddAsync(TaxPayment payment)
    {
        _context.TaxPayments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task UpdateAsync(TaxPayment payment)
    {
        _context.Entry(payment).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}

public class NcfSequenceRepository : INcfSequenceRepository
{
    private readonly TaxDbContext _context;

    public NcfSequenceRepository(TaxDbContext context) => _context = context;

    public async Task<NcfSequence?> GetByIdAsync(Guid id)
        => await _context.NcfSequences.FindAsync(id);

    public async Task<IEnumerable<NcfSequence>> GetByTaxpayerIdAsync(Guid taxpayerId)
        => await _context.NcfSequences
            .Where(s => s.TaxpayerId == taxpayerId)
            .ToListAsync();

    public async Task<NcfSequence?> GetActiveByTypeAsync(Guid taxpayerId, string ncfType)
    {
        var type = Enum.Parse<NcfType>(ncfType);
        return await _context.NcfSequences
            .FirstOrDefaultAsync(s => s.TaxpayerId == taxpayerId && s.NcfType == type && s.IsActive);
    }

    public async Task<NcfSequence> AddAsync(NcfSequence sequence)
    {
        _context.NcfSequences.Add(sequence);
        await _context.SaveChangesAsync();
        return sequence;
    }

    public async Task UpdateAsync(NcfSequence sequence)
    {
        _context.Entry(sequence).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<string> GetNextNcfAsync(Guid taxpayerId, string ncfType)
    {
        var sequence = await GetActiveByTypeAsync(taxpayerId, ncfType);
        if (sequence == null)
            throw new InvalidOperationException($"No hay secuencia NCF activa para tipo {ncfType}");

        if (sequence.CurrentNumber >= sequence.EndNumber)
            throw new InvalidOperationException("Secuencia NCF agotada");

        if (sequence.ExpirationDate < DateTime.UtcNow)
            throw new InvalidOperationException("Secuencia NCF expirada");

        sequence.CurrentNumber++;
        await UpdateAsync(sequence);

        // Formato: B01XXXXXXXXXX (11 dígitos)
        return $"B{sequence.Serie}{sequence.CurrentNumber:D10}";
    }
}

public class Reporte606Repository : IReporte606Repository
{
    private readonly TaxDbContext _context;

    public Reporte606Repository(TaxDbContext context) => _context = context;

    public async Task<IEnumerable<Reporte606Item>> GetByDeclarationIdAsync(Guid declarationId)
        => await _context.Reporte606Items
            .Where(i => i.TaxDeclarationId == declarationId)
            .ToListAsync();

    public async Task<Reporte606Item> AddAsync(Reporte606Item item)
    {
        _context.Reporte606Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task AddRangeAsync(IEnumerable<Reporte606Item> items)
    {
        _context.Reporte606Items.AddRange(items);
        await _context.SaveChangesAsync();
    }
}

public class Reporte607Repository : IReporte607Repository
{
    private readonly TaxDbContext _context;

    public Reporte607Repository(TaxDbContext context) => _context = context;

    public async Task<IEnumerable<Reporte607Item>> GetByDeclarationIdAsync(Guid declarationId)
        => await _context.Reporte607Items
            .Where(i => i.TaxDeclarationId == declarationId)
            .ToListAsync();

    public async Task<Reporte607Item> AddAsync(Reporte607Item item)
    {
        _context.Reporte607Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task AddRangeAsync(IEnumerable<Reporte607Item> items)
    {
        _context.Reporte607Items.AddRange(items);
        await _context.SaveChangesAsync();
    }
}
