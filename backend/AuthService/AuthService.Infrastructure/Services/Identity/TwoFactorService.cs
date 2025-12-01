using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using OtpNet;
using System.Text.Json;

namespace AuthService.Infrastructure.Services.Identity;

public class TwoFactorService : ITwoFactorService
{
    private readonly IQRCodeService _qrCodeService;
    private readonly IDistributedCache _cache;
    private readonly IAuthNotificationService _notificationService;
    private readonly IUserRepository _userRepository; // NECESARIO: Agregar esta dependencia

    public TwoFactorService(
        IQRCodeService qrCodeService,
        IDistributedCache cache,
        IAuthNotificationService notificationService,
        IUserRepository userRepository) // Agregar UserRepository
    {
        _qrCodeService = qrCodeService;
        _cache = cache;
        _notificationService = notificationService;
        _userRepository = userRepository; // Inicializar
    }

    public async Task<(string secret, string qrCodeUri)> GenerateAuthenticatorKeyAsync(string userId, string email)
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(key);

        var issuer = "YourApp";
        var qrCodeUri = $"otpauth://totp/{issuer}:{email}?secret={base32Secret}&issuer={issuer}&digits=6";
        var qrCodeImage = _qrCodeService.GenerateQRCode(qrCodeUri);

        await _cache.SetStringAsync($"2fa_setup_{userId}", base32Secret, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return (base32Secret, qrCodeImage);
    }

    public bool VerifyAuthenticatorCode(string secret, string code)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        return totp.VerifyTotp(code, out _);
    }

    public async Task<string> GenerateSmsCodeAsync(string userId)
    {
        var code = GenerateRandomCode(6);
        await _cache.SetStringAsync($"2fa_sms_{userId}", code, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        return code;
    }

    public async Task<string> GenerateEmailCodeAsync(string userId)
    {
        var code = GenerateRandomCode(6);
        await _cache.SetStringAsync($"2fa_email_{userId}", code, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        return code;
    }

    public async Task<bool> VerifyCodeAsync(string userId, string code, TwoFactorAuthType type)
    {
        var cacheKey = type switch
        {
            TwoFactorAuthType.Authenticator => $"2fa_setup_{userId}",
            TwoFactorAuthType.SMS => $"2fa_sms_{userId}",
            TwoFactorAuthType.Email => $"2fa_email_{userId}",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        var storedCode = await _cache.GetStringAsync(cacheKey);
        if (storedCode == code)
        {
            await _cache.RemoveAsync(cacheKey);
            return true;
        }

        return false;
    }

    public async Task<List<string>> GenerateRecoveryCodesAsync(string userId)
    {
        var recoveryCodes = new List<string>();
        for (int i = 0; i < 10; i++)
            recoveryCodes.Add(GenerateRandomCode(8));

        await _cache.SetStringAsync($"recovery_codes_{userId}",
            JsonSerializer.Serialize(recoveryCodes),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) });

        return recoveryCodes;
    }

    public async Task<bool> VerifyRecoveryCodeAsync(string userId, string code)
    {
        var storedCodesJson = await _cache.GetStringAsync($"recovery_codes_{userId}");
        if (storedCodesJson == null) return false;

        var storedCodes = JsonSerializer.Deserialize<List<string>>(storedCodesJson);
        if (storedCodes?.Contains(code) == true)
        {
            storedCodes.Remove(code);
            await _cache.SetStringAsync($"recovery_codes_{userId}",
                JsonSerializer.Serialize(storedCodes),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) });
            return true;
        }

        return false;
    }

    public async Task<bool> IsTwoFactorEnabledAsync(string userId)
    {
        var enabled = await _cache.GetStringAsync($"2fa_enabled_{userId}");
        return enabled == "true";
    }

    // CORRECCIÓN: Este método ahora SI envía códigos realmente
    public async Task<bool> SendTwoFactorCodeAsync(string userId, TwoFactorAuthType type)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            string code;
            string destination;

            switch (type)
            {
                case TwoFactorAuthType.SMS:
                    if (string.IsNullOrEmpty(user.PhoneNumber))
                        return false;

                    code = await GenerateSmsCodeAsync(userId);
                    destination = user.PhoneNumber;
                    await _notificationService.SendTwoFactorCodeAsync(destination, code, TwoFactorAuthType.SMS);
                    break;

                case TwoFactorAuthType.Email:
                    code = await GenerateEmailCodeAsync(userId);
                    destination = user.Email!;
                    await _notificationService.SendTwoFactorCodeAsync(destination, code, TwoFactorAuthType.Email);
                    break;

                default:
                    return false;
            }

            return true;
        }
        catch (Exception)
        {
            // Log the exception
            return false;
        }
    }

    // NUEVO MÉTODO: Enviar código a un destino específico
    public async Task<bool> SendTwoFactorCodeToDestinationAsync(string userId, TwoFactorAuthType type, string destination)
    {
        try
        {
            string code;

            switch (type)
            {
                case TwoFactorAuthType.SMS:
                    code = await GenerateSmsCodeAsync(userId);
                    await _notificationService.SendTwoFactorCodeAsync(destination, code, TwoFactorAuthType.SMS);
                    break;

                case TwoFactorAuthType.Email:
                    code = await GenerateEmailCodeAsync(userId);
                    await _notificationService.SendTwoFactorCodeAsync(destination, code, TwoFactorAuthType.Email);
                    break;

                default:
                    return false;
            }

            return true;
        }
        catch (Exception)
        {
            // Log the exception
            return false;
        }
    }

    private string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
