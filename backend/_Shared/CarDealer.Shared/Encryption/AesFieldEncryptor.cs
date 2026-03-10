using System;
using System.Security.Cryptography;
using System.Text;

namespace CarDealer.Shared.Encryption;

// ═══════════════════════════════════════════════════════════════════════════════
// AES-256-GCM FIELD ENCRYPTOR — LEY 172-13 COMPLIANCE
//
// Provides authenticated encryption for PII fields stored in PostgreSQL.
// Algorithm: AES-256-GCM (256-bit key, 96-bit nonce, 128-bit auth tag)
//
// Wire format: Base64( nonce[12] + ciphertext[N] + tag[16] )
//
// Key management:
//   - Key MUST be stored in environment variable OKLA_PII_ENCRYPTION_KEY
//   - Key format: 32-byte value encoded as Base64 (44 chars)
//   - Key MUST NOT be in appsettings.json or any committed file
//
// Migration support:
//   - DecryptOrPassthrough() handles mixed plaintext/encrypted data
//   - IsEncrypted() detects whether a value is already encrypted
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class AesFieldEncryptor : IFieldEncryptor
{
    private const int NonceSizeBytes = 12;   // AES-GCM standard nonce
    private const int TagSizeBytes = 16;     // AES-GCM standard auth tag
    private const int KeySizeBytes = 32;     // AES-256 = 256 bits
    private const int MinCiphertextLength = NonceSizeBytes + TagSizeBytes + 1; // At least 1 byte of data

    private readonly byte[] _key;

    /// <summary>
    /// Creates a new AesFieldEncryptor with the specified key.
    /// </summary>
    /// <param name="base64Key">Base64-encoded 32-byte AES-256 key</param>
    /// <exception cref="ArgumentException">If key is invalid</exception>
    public AesFieldEncryptor(string base64Key)
    {
        if (string.IsNullOrWhiteSpace(base64Key))
            throw new ArgumentException(
                "PII encryption key is not configured. " +
                "Set the OKLA_PII_ENCRYPTION_KEY environment variable with a Base64-encoded 32-byte key. " +
                "Generate one with: openssl rand -base64 32",
                nameof(base64Key));

        try
        {
            _key = Convert.FromBase64String(base64Key);
        }
        catch (FormatException)
        {
            throw new ArgumentException(
                "PII encryption key is not valid Base64. " +
                "Generate a valid key with: openssl rand -base64 32",
                nameof(base64Key));
        }

        if (_key.Length != KeySizeBytes)
            throw new ArgumentException(
                $"PII encryption key must be exactly {KeySizeBytes} bytes ({KeySizeBytes * 8} bits). " +
                $"Got {_key.Length} bytes. Generate with: openssl rand -base64 32",
                nameof(base64Key));
    }

    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return plaintext;

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[NonceSizeBytes];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSizeBytes];

        using var aesGcm = new AesGcm(_key, TagSizeBytes);
        aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Wire format: nonce[12] + ciphertext[N] + tag[16]
        var result = new byte[NonceSizeBytes + ciphertext.Length + TagSizeBytes];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSizeBytes);
        Buffer.BlockCopy(ciphertext, 0, result, NonceSizeBytes, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, result, NonceSizeBytes + ciphertext.Length, TagSizeBytes);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return ciphertext;

        var fullCipher = Convert.FromBase64String(ciphertext);

        if (fullCipher.Length < MinCiphertextLength)
            throw new CryptographicException(
                $"Ciphertext too short: {fullCipher.Length} bytes (minimum {MinCiphertextLength}).");

        var nonce = new byte[NonceSizeBytes];
        var tag = new byte[TagSizeBytes];
        var encryptedDataLength = fullCipher.Length - NonceSizeBytes - TagSizeBytes;
        var encrypted = new byte[encryptedDataLength];

        Buffer.BlockCopy(fullCipher, 0, nonce, 0, NonceSizeBytes);
        Buffer.BlockCopy(fullCipher, NonceSizeBytes, encrypted, 0, encryptedDataLength);
        Buffer.BlockCopy(fullCipher, NonceSizeBytes + encryptedDataLength, tag, 0, TagSizeBytes);

        var plaintext = new byte[encryptedDataLength];
        using var aesGcm = new AesGcm(_key, TagSizeBytes);
        aesGcm.Decrypt(nonce, encrypted, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    public string DecryptOrPassthrough(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (!IsEncrypted(value))
            return value;

        try
        {
            return Decrypt(value);
        }
        catch
        {
            // If decryption fails, assume it's plaintext (migration period)
            return value;
        }
    }

    public bool IsEncrypted(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        // Quick check: encrypted values are Base64 and have minimum length
        // A 1-char plaintext encrypted = 12 (nonce) + 1 (cipher) + 16 (tag) = 29 bytes → 40 chars base64
        if (value.Length < 40)
            return false;

        try
        {
            var bytes = Convert.FromBase64String(value);
            return bytes.Length >= MinCiphertextLength;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a new random 256-bit encryption key as Base64.
    /// Use this for initial key generation only.
    /// </summary>
    public static string GenerateKey()
    {
        var key = RandomNumberGenerator.GetBytes(KeySizeBytes);
        return Convert.ToBase64String(key);
    }
}
