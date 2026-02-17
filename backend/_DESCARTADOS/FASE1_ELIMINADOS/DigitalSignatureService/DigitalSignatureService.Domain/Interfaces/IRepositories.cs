// =====================================================
// DigitalSignatureService - Interfaces
// Ley 339-22 Firma Digital de República Dominicana
// =====================================================

using DigitalSignatureService.Domain.Entities;
using DigitalSignatureService.Domain.Enums;

namespace DigitalSignatureService.Domain.Interfaces;

public interface IDigitalCertificateRepository
{
    Task<DigitalCertificate?> GetByIdAsync(Guid id);
    Task<DigitalCertificate?> GetBySerialNumberAsync(string serialNumber);
    Task<IEnumerable<DigitalCertificate>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<DigitalCertificate>> GetActiveByUserIdAsync(Guid userId);
    Task<IEnumerable<DigitalCertificate>> GetExpiringCertificatesAsync(int daysAhead);
    Task<DigitalCertificate> AddAsync(DigitalCertificate certificate);
    Task UpdateAsync(DigitalCertificate certificate);
    Task<int> GetCountAsync();
}

public interface IDigitalSignatureRepository
{
    Task<DigitalSignature?> GetByIdAsync(Guid id);
    Task<IEnumerable<DigitalSignature>> GetByDocumentIdAsync(Guid documentId);
    Task<IEnumerable<DigitalSignature>> GetByCertificateIdAsync(Guid certificateId);
    Task<IEnumerable<DigitalSignature>> GetBySignerIdentificationAsync(string identification);
    Task<DigitalSignature> AddAsync(DigitalSignature signature);
    Task UpdateAsync(DigitalSignature signature);
    Task<int> GetCountAsync();
}

public interface ISignatureVerificationRepository
{
    Task<SignatureVerification?> GetByIdAsync(Guid id);
    Task<IEnumerable<SignatureVerification>> GetBySignatureIdAsync(Guid signatureId);
    Task<SignatureVerification> AddAsync(SignatureVerification verification);
}

public interface ITimeStampRepository
{
    Task<TimeStamp?> GetByIdAsync(Guid id);
    Task<TimeStamp?> GetBySignatureIdAsync(Guid signatureId);
    Task<TimeStamp> AddAsync(TimeStamp timestamp);
}
