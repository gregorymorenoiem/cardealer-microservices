using System;

namespace CarDealer.Shared.Encryption;

/// <summary>
/// Service for field-level encryption/decryption of PII fields.
/// Ley 172-13 compliance: All PII must be encrypted at rest using AES-256.
/// </summary>
public interface IFieldEncryptor
{
    /// <summary>
    /// Encrypts a plaintext string using AES-256-GCM with a unique nonce.
    /// Returns a base64-encoded ciphertext (nonce + ciphertext + tag).
    /// </summary>
    string Encrypt(string plaintext);

    /// <summary>
    /// Decrypts a base64-encoded AES-256-GCM ciphertext.
    /// Returns the original plaintext string.
    /// </summary>
    string Decrypt(string ciphertext);

    /// <summary>
    /// Attempts to decrypt. If the value doesn't appear to be encrypted
    /// (not valid base64 or wrong format), returns the original value unchanged.
    /// Useful during migration from plaintext to encrypted data.
    /// </summary>
    string DecryptOrPassthrough(string value);

    /// <summary>
    /// Returns true if the value appears to be an encrypted ciphertext
    /// (valid base64, correct minimum length for nonce + tag).
    /// </summary>
    bool IsEncrypted(string value);
}
