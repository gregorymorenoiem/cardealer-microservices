using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StaffService.Domain.Entities;

namespace StaffService.Domain.Interfaces;

public interface IStaffInvitationRepository
{
    Task<StaffInvitation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<StaffInvitation?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<StaffInvitation?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<StaffInvitation>> GetPendingAsync(CancellationToken ct = default);
    Task<IEnumerable<StaffInvitation>> GetByInviterAsync(Guid inviterId, CancellationToken ct = default);
    Task<IEnumerable<StaffInvitation>> SearchAsync(InvitationStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(InvitationStatus? status, CancellationToken ct = default);
    Task<StaffInvitation> AddAsync(StaffInvitation invitation, CancellationToken ct = default);
    Task UpdateAsync(StaffInvitation invitation, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> TokenExistsAsync(string token, CancellationToken ct = default);
    Task ExpireOldInvitationsAsync(CancellationToken ct = default);
}
