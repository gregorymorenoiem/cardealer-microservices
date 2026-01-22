// =====================================================
// ConsumerProtectionService - Repositories
// Ley 358-05 Derechos del Consumidor de RD
// =====================================================

using Microsoft.EntityFrameworkCore;
using ConsumerProtectionService.Domain.Entities;
using ConsumerProtectionService.Domain.Interfaces;
using ConsumerProtectionService.Domain.Enums;
using ConsumerProtectionService.Infrastructure.Persistence;

namespace ConsumerProtectionService.Infrastructure.Repositories;

public class WarrantyRepository : IWarrantyRepository
{
    private readonly ConsumerDbContext _context;

    public WarrantyRepository(ConsumerDbContext context) => _context = context;

    public async Task<Warranty?> GetByIdAsync(Guid id)
        => await _context.Warranties.FindAsync(id);

    public async Task<IEnumerable<Warranty>> GetByProductIdAsync(Guid productId)
        => await _context.Warranties.Where(w => w.ProductId == productId).ToListAsync();

    public async Task<IEnumerable<Warranty>> GetBySellerIdAsync(Guid sellerId)
        => await _context.Warranties.Where(w => w.SellerId == sellerId).ToListAsync();

    public async Task<IEnumerable<Warranty>> GetByConsumerIdAsync(Guid consumerId)
        => await _context.Warranties.Where(w => w.ConsumerId == consumerId).ToListAsync();

    public async Task<IEnumerable<Warranty>> GetActiveWarrantiesAsync()
        => await _context.Warranties.Where(w => w.Status == WarrantyStatus.Active && w.EndDate > DateTime.UtcNow).ToListAsync();

    public async Task<IEnumerable<Warranty>> GetExpiringWarrantiesAsync(int daysAhead)
    {
        var expirationDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.Warranties
            .Where(w => w.Status == WarrantyStatus.Active && w.EndDate <= expirationDate && w.EndDate > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<Warranty> AddAsync(Warranty warranty)
    {
        _context.Warranties.Add(warranty);
        await _context.SaveChangesAsync();
        return warranty;
    }

    public async Task UpdateAsync(Warranty warranty)
    {
        _context.Entry(warranty).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
        => await _context.Warranties.CountAsync();
}

public class WarrantyClaimRepository : IWarrantyClaimRepository
{
    private readonly ConsumerDbContext _context;

    public WarrantyClaimRepository(ConsumerDbContext context) => _context = context;

    public async Task<WarrantyClaim?> GetByIdAsync(Guid id)
        => await _context.WarrantyClaims.FindAsync(id);

    public async Task<IEnumerable<WarrantyClaim>> GetByWarrantyIdAsync(Guid warrantyId)
        => await _context.WarrantyClaims.Where(c => c.WarrantyId == warrantyId).ToListAsync();

    public async Task<IEnumerable<WarrantyClaim>> GetByConsumerIdAsync(Guid consumerId)
        => await _context.WarrantyClaims.Where(c => c.ConsumerId == consumerId).ToListAsync();

    public async Task<IEnumerable<WarrantyClaim>> GetPendingClaimsAsync()
        => await _context.WarrantyClaims.Where(c => c.Status == ComplaintStatus.Pending).ToListAsync();

    public async Task<WarrantyClaim> AddAsync(WarrantyClaim claim)
    {
        _context.WarrantyClaims.Add(claim);
        await _context.SaveChangesAsync();
        return claim;
    }

    public async Task UpdateAsync(WarrantyClaim claim)
    {
        _context.Entry(claim).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}

public class ComplaintRepository : IComplaintRepository
{
    private readonly ConsumerDbContext _context;

    public ComplaintRepository(ConsumerDbContext context) => _context = context;

    public async Task<Complaint?> GetByIdAsync(Guid id)
        => await _context.Complaints.FindAsync(id);

    public async Task<IEnumerable<Complaint>> GetByConsumerIdAsync(Guid consumerId)
        => await _context.Complaints.Where(c => c.ConsumerId == consumerId).ToListAsync();

    public async Task<IEnumerable<Complaint>> GetBySellerIdAsync(Guid sellerId)
        => await _context.Complaints.Where(c => c.SellerId == sellerId).ToListAsync();

    public async Task<IEnumerable<Complaint>> GetPendingComplaintsAsync()
        => await _context.Complaints.Where(c => c.Status == ComplaintStatus.Pending || c.Status == ComplaintStatus.InProgress).ToListAsync();

    public async Task<IEnumerable<Complaint>> GetEscalatedToProConsumidorAsync()
        => await _context.Complaints.Where(c => c.IsEscalatedToProConsumidor).ToListAsync();

    public async Task<Complaint> AddAsync(Complaint complaint)
    {
        _context.Complaints.Add(complaint);
        await _context.SaveChangesAsync();
        return complaint;
    }

    public async Task UpdateAsync(Complaint complaint)
    {
        _context.Entry(complaint).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
        => await _context.Complaints.CountAsync();
}

public class ComplaintEvidenceRepository : IComplaintEvidenceRepository
{
    private readonly ConsumerDbContext _context;

    public ComplaintEvidenceRepository(ConsumerDbContext context) => _context = context;

    public async Task<ComplaintEvidence?> GetByIdAsync(Guid id)
        => await _context.ComplaintEvidences.FindAsync(id);

    public async Task<IEnumerable<ComplaintEvidence>> GetByComplaintIdAsync(Guid complaintId)
        => await _context.ComplaintEvidences.Where(e => e.ComplaintId == complaintId).ToListAsync();

    public async Task<ComplaintEvidence> AddAsync(ComplaintEvidence evidence)
    {
        _context.ComplaintEvidences.Add(evidence);
        await _context.SaveChangesAsync();
        return evidence;
    }

    public async Task DeleteAsync(Guid id)
    {
        var evidence = await _context.ComplaintEvidences.FindAsync(id);
        if (evidence != null)
        {
            _context.ComplaintEvidences.Remove(evidence);
            await _context.SaveChangesAsync();
        }
    }
}

public class MediationRepository : IMediationRepository
{
    private readonly ConsumerDbContext _context;

    public MediationRepository(ConsumerDbContext context) => _context = context;

    public async Task<Mediation?> GetByIdAsync(Guid id)
        => await _context.Mediations.FindAsync(id);

    public async Task<IEnumerable<Mediation>> GetByComplaintIdAsync(Guid complaintId)
        => await _context.Mediations.Where(m => m.ComplaintId == complaintId).ToListAsync();

    public async Task<IEnumerable<Mediation>> GetScheduledMediationsAsync()
        => await _context.Mediations
            .Where(m => m.Status == MediationStatus.Scheduled && m.ScheduledDate > DateTime.UtcNow)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync();

    public async Task<Mediation> AddAsync(Mediation mediation)
    {
        _context.Mediations.Add(mediation);
        await _context.SaveChangesAsync();
        return mediation;
    }

    public async Task UpdateAsync(Mediation mediation)
    {
        _context.Entry(mediation).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
