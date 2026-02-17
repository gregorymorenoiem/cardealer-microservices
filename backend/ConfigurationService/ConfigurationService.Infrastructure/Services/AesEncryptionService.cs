using System.Security.Cryptography;
using System.Text;
using ConfigurationService.Application.Interfaces;

namespace ConfigurationService.Infrastructure.Services;

/// <summary>
/// AES-256-CBC encryption with random IV per encryption operation.
/// The IV is prepended to the ciphertext (first 16 bytes) so each
/// encryption of the same plaintext produces different output.
/// 
/// ⚠️ SECURITY: The encryption key MUST come from environment variables
/// or Kubernetes Secrets in production. Never hardcode it.
/// </summary>
public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(string encryptionKey)
    {
        if (string.IsNullOrWhiteSpace(encryptionKey) || encryptionKey.Length < 16)
            throw new ArgumentException("Encryption key must be at least 16 characters", nameof(encryptionKey));

        // Derive a 256-bit key using SHA-256
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Generate a cryptographically random IV for each encryption
        aes.GenerateIV();

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream();
        // Prepend the IV (16 bytes) so we can extract it during decryption
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            throw new ArgumentNullException(nameof(encryptedText));

        var fullCipher = Convert.FromBase64String(encryptedText);

        // Extract the IV from the first 16 bytes
        var iv = new byte[16];
        var cipher = new byte[fullCipher.Length - 16];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, 16);
        Buffer.BlockCopy(fullCipher, 16, cipher, 0, cipher.Length);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream(cipher);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}
