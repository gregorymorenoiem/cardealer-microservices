using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CarDealer.Shared.Encryption;

// ═══════════════════════════════════════════════════════════════════════════════
// ENCRYPTED STRING VALUE CONVERTER — EF CORE INTEGRATION
//
// Transparent encryption/decryption for PII properties in EF Core entities.
// Apply to any string property via HasConversion() in OnModelCreating:
//
//   builder.Property(e => e.Email)
//       .HasConversion(new EncryptedStringConverter(encryptor));
//
// The converter:
//   - Encrypts on write (SaveChanges)
//   - Decrypts on read (queries)
//   - Handles null values transparently
//   - Uses DecryptOrPassthrough for migration from plaintext data
// ═══════════════════════════════════════════════════════════════════════════════

public class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(IFieldEncryptor encryptor)
        : base(
            plaintext => encryptor.Encrypt(plaintext),
            ciphertext => encryptor.DecryptOrPassthrough(ciphertext))
    {
    }
}

/// <summary>
/// Converter for nullable strings. Same encryption behavior, handles nulls.
/// </summary>
public class NullableEncryptedStringConverter : ValueConverter<string?, string?>
{
    public NullableEncryptedStringConverter(IFieldEncryptor encryptor)
        : base(
            plaintext => plaintext == null ? null : encryptor.Encrypt(plaintext),
            ciphertext => ciphertext == null ? null : encryptor.DecryptOrPassthrough(ciphertext))
    {
    }
}
