using AuthService.Domain.Entities;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces.Services;

public interface IPasswordResetService
{
    Task GenerateAndSendTokenAsync(ApplicationUser user);
    Task<bool> ResetAsync(string token, string newPassword);
}
