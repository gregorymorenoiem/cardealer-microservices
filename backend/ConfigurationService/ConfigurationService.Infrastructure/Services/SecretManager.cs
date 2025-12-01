using ConfigurationService.Application.Interfaces;
using ConfigurationService.Domain.Entities;
using ConfigurationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationService.Infrastructure.Services;

public class SecretManager : ISecretManager
{
    private readonly ConfigurationDbContext _context;
    private readonly IEncryptionService _encryptionService;

    public SecretManager(ConfigurationDbContext context, IEncryptionService encryptionService)
    {
        _context = context;
        _encryptionService = encryptionService;
    }

    public async Task<string?> GetDecryptedSecretAsync(string key, string environment, string? tenantId = null)
    {
        var secret = await _context.EncryptedSecrets
            .Where(s => s.Key == key && s.Environment == environment &&
                       (tenantId == null || s.TenantId == tenantId) && s.IsActive)
            .FirstOrDefaultAsync();

        if (secret == null)
            return null;

        // Check expiration
        if (secret.ExpiresAt.HasValue && secret.ExpiresAt.Value < DateTime.UtcNow)
            return null;

        return _encryptionService.Decrypt(secret.EncryptedValue);
    }

    public async Task<EncryptedSecret> CreateSecretAsync(string key, string plainValue, string environment, string createdBy,
        string? description = null, string? tenantId = null, DateTime? expiresAt = null)
    {
        var encryptedValue = _encryptionService.Encrypt(plainValue);

        var secret = new EncryptedSecret
        {
            Id = Guid.NewGuid(),
            Key = key,
            EncryptedValue = encryptedValue,
            Environment = environment,
            Description = description,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            IsActive = true,
            ExpiresAt = expiresAt
        };

        await _context.EncryptedSecrets.AddAsync(secret);
        await _context.SaveChangesAsync();

        return secret;
    }

    public async Task<EncryptedSecret> UpdateSecretAsync(Guid id, string plainValue, string updatedBy)
    {
        var secret = await _context.EncryptedSecrets.FindAsync(id);
        if (secret == null)
            throw new InvalidOperationException($"Secret with ID {id} not found");

        secret.EncryptedValue = _encryptionService.Encrypt(plainValue);
        secret.UpdatedAt = DateTime.UtcNow;
        secret.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();

        return secret;
    }

    public async Task DeleteSecretAsync(Guid id)
    {
        var secret = await _context.EncryptedSecrets.FindAsync(id);
        if (secret != null)
        {
            secret.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<EncryptedSecret>> GetAllSecretsAsync(string environment, string? tenantId = null)
    {
        return await _context.EncryptedSecrets
            .Where(s => s.Environment == environment &&
                       (tenantId == null || s.TenantId == tenantId) && s.IsActive)
            .ToListAsync();
    }
}
