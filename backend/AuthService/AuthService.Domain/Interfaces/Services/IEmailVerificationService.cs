using AuthService.Domain.Entities;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces.Services;

public interface IEmailVerificationService
{
    Task SendVerificationEmailAsync(ApplicationUser user);
    Task<bool> VerifyAsync(string token);
}