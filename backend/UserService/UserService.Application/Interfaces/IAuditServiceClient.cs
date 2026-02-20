using System;
using System.Threading.Tasks;

namespace UserService.Application.Interfaces
{
    public interface IAuditServiceClient
    {
        Task LogUserCreatedAsync(Guid userId, string email, string performedBy);
        Task LogUserUpdatedAsync(Guid userId, string changes, string performedBy);
        Task LogUserDeletedAsync(Guid userId, string email, string performedBy);
        Task LogRoleAssignedAsync(Guid userId, Guid roleId, string performedBy);
        Task LogRoleRevokedAsync(Guid userId, Guid roleId, string performedBy);
        Task LogSellerConversionAsync(Guid userId, Guid sellerProfileId, string previousAccountType, string performedBy);
        Task LogDealerRegistrationAsync(Guid dealerId, Guid ownerUserId, string businessName, string performedBy);
        Task LogDealerVerificationAsync(Guid dealerId, bool isApproved, string performedBy);
    }
}
