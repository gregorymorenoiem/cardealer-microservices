// =====================================================
// ConsumerProtectionService - Repository Interfaces
// Ley 358-05 Derechos del Consumidor de RD
// =====================================================

using ConsumerProtectionService.Domain.Entities;

namespace ConsumerProtectionService.Domain.Interfaces;

public interface IWarrantyRepository
{
    Task<Warranty?> GetByIdAsync(Guid id);
    Task<IEnumerable<Warranty>> GetByProductIdAsync(Guid productId);
    Task<IEnumerable<Warranty>> GetBySellerIdAsync(Guid sellerId);
    Task<IEnumerable<Warranty>> GetByConsumerIdAsync(Guid consumerId);
    Task<IEnumerable<Warranty>> GetActiveWarrantiesAsync();
    Task<IEnumerable<Warranty>> GetExpiringWarrantiesAsync(int daysAhead);
    Task<Warranty> AddAsync(Warranty warranty);
    Task UpdateAsync(Warranty warranty);
    Task<int> GetCountAsync();
}

public interface IWarrantyClaimRepository
{
    Task<WarrantyClaim?> GetByIdAsync(Guid id);
    Task<IEnumerable<WarrantyClaim>> GetByWarrantyIdAsync(Guid warrantyId);
    Task<IEnumerable<WarrantyClaim>> GetByConsumerIdAsync(Guid consumerId);
    Task<IEnumerable<WarrantyClaim>> GetPendingClaimsAsync();
    Task<WarrantyClaim> AddAsync(WarrantyClaim claim);
    Task UpdateAsync(WarrantyClaim claim);
}

public interface IComplaintRepository
{
    Task<Complaint?> GetByIdAsync(Guid id);
    Task<IEnumerable<Complaint>> GetByConsumerIdAsync(Guid consumerId);
    Task<IEnumerable<Complaint>> GetBySellerIdAsync(Guid sellerId);
    Task<IEnumerable<Complaint>> GetPendingComplaintsAsync();
    Task<IEnumerable<Complaint>> GetEscalatedToProConsumidorAsync();
    Task<Complaint> AddAsync(Complaint complaint);
    Task UpdateAsync(Complaint complaint);
    Task<int> GetCountAsync();
}

public interface IComplaintEvidenceRepository
{
    Task<ComplaintEvidence?> GetByIdAsync(Guid id);
    Task<IEnumerable<ComplaintEvidence>> GetByComplaintIdAsync(Guid complaintId);
    Task<ComplaintEvidence> AddAsync(ComplaintEvidence evidence);
    Task DeleteAsync(Guid id);
}

public interface IMediationRepository
{
    Task<Mediation?> GetByIdAsync(Guid id);
    Task<IEnumerable<Mediation>> GetByComplaintIdAsync(Guid complaintId);
    Task<IEnumerable<Mediation>> GetScheduledMediationsAsync();
    Task<Mediation> AddAsync(Mediation mediation);
    Task UpdateAsync(Mediation mediation);
}
