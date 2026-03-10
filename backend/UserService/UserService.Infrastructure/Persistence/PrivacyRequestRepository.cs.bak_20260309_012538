using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities.Privacy;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Persistence;

/// <summary>
/// Implementación del repositorio de solicitudes de privacidad
/// </summary>
public class PrivacyRequestRepository : IPrivacyRequestRepository
{
    private readonly ApplicationDbContext _context;

    public PrivacyRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PrivacyRequest?> GetByIdAsync(Guid id)
    {
        return await _context.PrivacyRequests
            .Include(pr => pr.User)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<PrivacyRequest?> GetPendingDeletionRequestAsync(Guid userId)
    {
        return await _context.PrivacyRequests
            .FirstOrDefaultAsync(pr => 
                pr.UserId == userId && 
                pr.Type == PrivacyRequestType.Cancellation &&
                pr.Status == PrivacyRequestStatus.Pending);
    }

    public async Task<IEnumerable<PrivacyRequest>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PrivacyRequests
            .Where(pr => pr.UserId == userId)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();
    }

    public async Task<PrivacyRequest> AddAsync(PrivacyRequest request)
    {
        await _context.PrivacyRequests.AddAsync(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task UpdateAsync(PrivacyRequest request)
    {
        _context.PrivacyRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasPendingRequestAsync(Guid userId, PrivacyRequestType type)
    {
        return await _context.PrivacyRequests
            .AnyAsync(pr => 
                pr.UserId == userId && 
                pr.Type == type &&
                pr.Status == PrivacyRequestStatus.Pending);
    }

    public async Task<IEnumerable<PrivacyRequest>> GetExpiredGracePeriodRequestsAsync()
    {
        return await _context.PrivacyRequests
            .Where(pr => 
                pr.Type == PrivacyRequestType.Cancellation &&
                pr.Status == PrivacyRequestStatus.Pending &&
                pr.IsConfirmed &&
                pr.GracePeriodEndsAt <= DateTime.UtcNow)
            .Include(pr => pr.User)
            .ToListAsync();
    }

    public async Task<PrivacyRequest?> GetByConfirmationCodeAsync(Guid userId, string code)
    {
        return await _context.PrivacyRequests
            .FirstOrDefaultAsync(pr => 
                pr.UserId == userId && 
                pr.ConfirmationCode == code &&
                pr.Type == PrivacyRequestType.Cancellation &&
                pr.Status == PrivacyRequestStatus.Pending &&
                (pr.ExpiresAt == null || pr.ExpiresAt > DateTime.UtcNow));
    }
}
