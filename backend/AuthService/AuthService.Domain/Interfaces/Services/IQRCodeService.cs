namespace AuthService.Domain.Interfaces.Services;

public interface IQRCodeService
{
    string GenerateQRCode(string text, int size = 300);
}