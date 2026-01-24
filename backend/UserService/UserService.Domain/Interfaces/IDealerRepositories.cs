using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces
{
    public interface IDealerRepository
    {
        Task<Dealer?> GetByIdAsync(Guid id);
        Task<Dealer?> GetByOwnerIdAsync(Guid ownerId);
        Task<IEnumerable<Dealer>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<Dealer>> SearchAsync(string? searchTerm, string? city, string? state, DealerType? dealerType, bool? isVerified, int page = 1, int pageSize = 10);
        Task<Dealer> AddAsync(Dealer dealer);
        Task UpdateAsync(Dealer dealer);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountAsync();
        Task<int> CountByTypeAsync(DealerType dealerType);
    }

    public interface ISellerProfileRepository
    {
        Task<SellerProfile?> GetByIdAsync(Guid id);
        Task<SellerProfile?> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<SellerProfile>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<SellerProfile>> SearchAsync(string? searchTerm, string? city, string? state, bool? isVerified, int page = 1, int pageSize = 10);
        Task<SellerProfile> AddAsync(SellerProfile profile);
        Task UpdateAsync(SellerProfile profile);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountAsync();
    }

    public interface IIdentityDocumentRepository
    {
        Task<IdentityDocument?> GetByIdAsync(Guid id);
        Task<IEnumerable<IdentityDocument>> GetBySellerProfileIdAsync(Guid sellerProfileId);
        Task<IEnumerable<IdentityDocument>> GetPendingVerificationAsync(int page = 1, int pageSize = 10);
        Task<IdentityDocument> AddAsync(IdentityDocument document);
        Task UpdateAsync(IdentityDocument document);
        Task DeleteAsync(Guid id);
        Task<int> CountPendingAsync();
    }

    public interface IDealerEmployeeRepository
    {
        // Employee operations
        Task<DealerEmployee?> GetByIdAsync(Guid id);
        Task<IEnumerable<DealerEmployee>> GetByDealerIdAsync(Guid dealerId);
        Task<DealerEmployee?> GetByUserIdAndDealerIdAsync(Guid userId, Guid dealerId);
        Task<DealerEmployee> AddAsync(DealerEmployee employee);
        Task UpdateAsync(DealerEmployee employee);
        Task DeleteAsync(Guid id);
        Task<int> CountByDealerIdAsync(Guid dealerId);

        // Invitation operations
        Task<DealerEmployeeInvitation?> GetInvitationByIdAsync(Guid dealerId, Guid invitationId);
        Task<DealerEmployeeInvitation?> GetPendingInvitationByEmailAsync(Guid dealerId, string email);
        Task<IEnumerable<DealerEmployeeInvitation>> GetPendingInvitationsAsync(Guid dealerId);
        Task<DealerEmployeeInvitation> AddInvitationAsync(DealerEmployeeInvitation invitation);
        Task UpdateInvitationAsync(DealerEmployeeInvitation invitation);
    }
}
