using Microsoft.EntityFrameworkCore;
using StaffService.Domain.Entities;
using StaffService.Domain.Interfaces;

namespace StaffService.Infrastructure.Persistence.Repositories;

public class StaffInvitationRepository : IStaffInvitationRepository
{
    private readonly StaffDbContext _context;

    public StaffInvitationRepository(StaffDbContext context)
    {
        _context = context;
    }

    public async Task<StaffInvitation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.StaffInvitations
            .Include(i => i.Department)
            .Include(i => i.Position)
            .Include(i => i.InvitedByStaff)
            .Include(i => i.Staff)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<StaffInvitation?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await _context.StaffInvitations
            .Include(i => i.Department)
            .Include(i => i.Position)
            .Include(i => i.InvitedByStaff)
            .FirstOrDefaultAsync(i => i.Token == token, ct);
    }

    public async Task<StaffInvitation?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.StaffInvitations
            .Include(i => i.Department)
            .Include(i => i.Position)
            .Where(i => i.Email.ToLower() == email.ToLower() && i.Status == InvitationStatus.Pending)
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<StaffInvitation>> GetPendingAsync(CancellationToken ct = default)
    {
        return await _context.StaffInvitations
            .Include(i => i.InvitedByStaff)
            .Where(i => i.Status == InvitationStatus.Pending && i.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<StaffInvitation>> GetByInviterAsync(Guid inviterId, CancellationToken ct = default)
    {
        return await _context.StaffInvitations
            .Include(i => i.Staff)
            .Where(i => i.InvitedBy == inviterId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<StaffInvitation>> SearchAsync(
        InvitationStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.StaffInvitations
            .Include(i => i.Department)
            .Include(i => i.Position)
            .Include(i => i.InvitedByStaff)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(InvitationStatus? status, CancellationToken ct = default)
    {
        var query = _context.StaffInvitations.AsQueryable();

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        return await query.CountAsync(ct);
    }

    public async Task<StaffInvitation> AddAsync(StaffInvitation invitation, CancellationToken ct = default)
    {
        _context.StaffInvitations.Add(invitation);
        await _context.SaveChangesAsync(ct);
        return invitation;
    }

    public async Task UpdateAsync(StaffInvitation invitation, CancellationToken ct = default)
    {
        _context.StaffInvitations.Update(invitation);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var invitation = await _context.StaffInvitations.FindAsync(new object[] { id }, ct);
        if (invitation != null)
        {
            _context.StaffInvitations.Remove(invitation);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> TokenExistsAsync(string token, CancellationToken ct = default)
    {
        return await _context.StaffInvitations.AnyAsync(i => i.Token == token, ct);
    }

    public async Task ExpireOldInvitationsAsync(CancellationToken ct = default)
    {
        var expiredInvitations = await _context.StaffInvitations
            .Where(i => i.Status == InvitationStatus.Pending && i.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var invitation in expiredInvitations)
        {
            invitation.Status = InvitationStatus.Expired;
        }

        await _context.SaveChangesAsync(ct);
    }
}
