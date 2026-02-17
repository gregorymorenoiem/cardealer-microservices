// =====================================================
// DigitalSignatureService - Repositories
// Ley 339-22 Firma Digital de República Dominicana
// =====================================================

using Microsoft.EntityFrameworkCore;
using DigitalSignatureService.Domain.Entities;
using DigitalSignatureService.Domain.Interfaces;
using DigitalSignatureService.Domain.Enums;
using DigitalSignatureService.Infrastructure.Persistence;

namespace DigitalSignatureService.Infrastructure.Repositories;

public class DigitalCertificateRepository : IDigitalCertificateRepository
{
    private readonly SignatureDbContext _context;

    public DigitalCertificateRepository(SignatureDbContext context) => _context = context;

    public async Task<DigitalCertificate?> GetByIdAsync(Guid id)
        => await _context.Certificates.FindAsync(id);

    public async Task<DigitalCertificate?> GetBySerialNumberAsync(string serialNumber)
        => await _context.Certificates.FirstOrDefaultAsync(c => c.SerialNumber == serialNumber);

    public async Task<IEnumerable<DigitalCertificate>> GetByUserIdAsync(Guid userId)
        => await _context.Certificates.Where(c => c.UserId == userId).ToListAsync();

    public async Task<IEnumerable<DigitalCertificate>> GetActiveByUserIdAsync(Guid userId)
        => await _context.Certificates
            .Where(c => c.UserId == userId && c.Status == CertificateStatus.Active && c.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

    public async Task<IEnumerable<DigitalCertificate>> GetExpiringCertificatesAsync(int daysAhead)
    {
        var expirationDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.Certificates
            .Where(c => c.Status == CertificateStatus.Active && c.ExpiresAt <= expirationDate && c.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<DigitalCertificate> AddAsync(DigitalCertificate certificate)
    {
        _context.Certificates.Add(certificate);
        await _context.SaveChangesAsync();
        return certificate;
    }

    public async Task UpdateAsync(DigitalCertificate certificate)
    {
        _context.Entry(certificate).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
        => await _context.Certificates.CountAsync();
}

public class DigitalSignatureRepository : IDigitalSignatureRepository
{
    private readonly SignatureDbContext _context;

    public DigitalSignatureRepository(SignatureDbContext context) => _context = context;

    public async Task<DigitalSignature?> GetByIdAsync(Guid id)
        => await _context.Signatures.FindAsync(id);

    public async Task<IEnumerable<DigitalSignature>> GetByDocumentIdAsync(Guid documentId)
        => await _context.Signatures.Where(s => s.DocumentId == documentId).ToListAsync();

    public async Task<IEnumerable<DigitalSignature>> GetByCertificateIdAsync(Guid certificateId)
        => await _context.Signatures.Where(s => s.CertificateId == certificateId).ToListAsync();

    public async Task<IEnumerable<DigitalSignature>> GetBySignerIdentificationAsync(string identification)
        => await _context.Signatures.Where(s => s.SignerIdentification == identification).ToListAsync();

    public async Task<DigitalSignature> AddAsync(DigitalSignature signature)
    {
        _context.Signatures.Add(signature);
        await _context.SaveChangesAsync();
        return signature;
    }

    public async Task UpdateAsync(DigitalSignature signature)
    {
        _context.Entry(signature).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
        => await _context.Signatures.CountAsync();
}

public class SignatureVerificationRepository : ISignatureVerificationRepository
{
    private readonly SignatureDbContext _context;

    public SignatureVerificationRepository(SignatureDbContext context) => _context = context;

    public async Task<SignatureVerification?> GetByIdAsync(Guid id)
        => await _context.Verifications.FindAsync(id);

    public async Task<IEnumerable<SignatureVerification>> GetBySignatureIdAsync(Guid signatureId)
        => await _context.Verifications.Where(v => v.SignatureId == signatureId).ToListAsync();

    public async Task<SignatureVerification> AddAsync(SignatureVerification verification)
    {
        _context.Verifications.Add(verification);
        await _context.SaveChangesAsync();
        return verification;
    }
}

public class TimeStampRepository : ITimeStampRepository
{
    private readonly SignatureDbContext _context;

    public TimeStampRepository(SignatureDbContext context) => _context = context;

    public async Task<TimeStamp?> GetByIdAsync(Guid id)
        => await _context.TimeStamps.FindAsync(id);

    public async Task<TimeStamp?> GetBySignatureIdAsync(Guid signatureId)
        => await _context.TimeStamps.FirstOrDefaultAsync(t => t.SignatureId == signatureId);

    public async Task<TimeStamp> AddAsync(TimeStamp timestamp)
    {
        _context.TimeStamps.Add(timestamp);
        await _context.SaveChangesAsync();
        return timestamp;
    }
}
