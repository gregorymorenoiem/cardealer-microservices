using AuthService.Domain.Interfaces.Services;
using QRCoder;

namespace AuthService.Infrastructure.Services.Identity;

public class QRCodeService : IQRCodeService
{
    public string GenerateQRCode(string text, int size = 300)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);
        // Return as Data URL so it can be used directly in <img src="...">
        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
    }
}
