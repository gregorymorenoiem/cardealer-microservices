using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarDealer.Shared.Encryption;

// ═══════════════════════════════════════════════════════════════════════════════
// ENCRYPTION EXTENSIONS — DI REGISTRATION + EF CORE HELPERS
//
// Usage in Program.cs:
//   builder.Services.AddPiiEncryption(builder.Configuration);
//
// Usage in DbContext.OnModelCreating:
//   var encryptor = this.GetService<IFieldEncryptor>();  // or inject via ctor
//   builder.Entity<User>().Property(e => e.Email)
//       .HasEncryptedConversion(encryptor);
//
// Key source priority:
//   1. Environment variable OKLA_PII_ENCRYPTION_KEY
//   2. Configuration key Encryption:PiiKey
//
// Generate a key: openssl rand -base64 32
// ═══════════════════════════════════════════════════════════════════════════════

public static class EncryptionExtensions
{
    /// <summary>
    /// Registers IFieldEncryptor as a singleton using AES-256-GCM.
    /// Key is read from OKLA_PII_ENCRYPTION_KEY env var or Encryption:PiiKey config.
    /// </summary>
    public static IServiceCollection AddPiiEncryption(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Priority: env var > config
        var key = Environment.GetEnvironmentVariable("OKLA_PII_ENCRYPTION_KEY")
                ?? configuration["Encryption:PiiKey"]
                ?? throw new InvalidOperationException(
                    "PII encryption key not found. " +
                    "Set OKLA_PII_ENCRYPTION_KEY environment variable or Encryption:PiiKey in configuration. " +
                    "Generate with: openssl rand -base64 32");

        var encryptor = new AesFieldEncryptor(key);
        services.AddSingleton<IFieldEncryptor>(encryptor);

        return services;
    }

    /// <summary>
    /// Configures a string property to use AES-256-GCM encryption at rest.
    /// The column stores base64-encoded ciphertext.
    /// NOTE: Encrypted columns cannot be used in WHERE clauses or indexes.
    /// Works for both nullable and non-nullable string properties.
    /// </summary>
    public static PropertyBuilder<string> HasEncryptedConversion(
        this PropertyBuilder<string> builder, IFieldEncryptor encryptor)
    {
        return builder.HasConversion(
            plaintext => plaintext == null ? null! : encryptor.Encrypt(plaintext),
            ciphertext => ciphertext == null ? null! : encryptor.DecryptOrPassthrough(ciphertext));
    }
}
