using ContactService.Domain.Entities;

namespace ContactService.Domain.Interfaces;

public interface IContactRequestRepository
{
    Task<ContactRequest?> GetByIdAsync(Guid id);
    Task<List<ContactRequest>> GetByBuyerIdAsync(Guid buyerId);
    Task<List<ContactRequest>> GetBySellerIdAsync(Guid sellerId);
    Task<List<ContactRequest>> GetByVehicleIdAsync(Guid vehicleId);
    Task<ContactRequest> CreateAsync(ContactRequest contactRequest);
    Task<ContactRequest> UpdateAsync(ContactRequest contactRequest);
    Task DeleteAsync(Guid id);
    Task<int> GetUnreadCountForSellerAsync(Guid sellerId);
}

public interface IContactMessageRepository
{
    Task<ContactMessage?> GetByIdAsync(Guid id);
    Task<List<ContactMessage>> GetByContactRequestIdAsync(Guid contactRequestId);
    Task<ContactMessage> CreateAsync(ContactMessage message);
    Task MarkAsReadAsync(Guid messageId);
    Task<int> GetUnreadCountForUserAsync(Guid userId);
}