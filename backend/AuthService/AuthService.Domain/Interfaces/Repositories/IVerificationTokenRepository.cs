using AuthService.Domain.Entities;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Interfaces.Repositories;

public interface IVerificationTokenRepository
{
    Task<VerificationToken?> GetByTokenAsync(string token);
    Task<VerificationToken?> GetByTokenAndTypeAsync(string token, VerificationTokenType type);
    Task<IEnumerable<VerificationToken>> GetByEmailAsync(string email);
    Task<VerificationToken?> GetValidByEmailAndTypeAsync(string email, VerificationTokenType type);
    Task AddAsync(VerificationToken token);
    Task UpdateAsync(VerificationToken token);
    Task DeleteAsync(Guid id);
    Task DeleteExpiredTokensAsync();
    Task<bool> ExistsValidTokenAsync(string email, VerificationTokenType type);
}
