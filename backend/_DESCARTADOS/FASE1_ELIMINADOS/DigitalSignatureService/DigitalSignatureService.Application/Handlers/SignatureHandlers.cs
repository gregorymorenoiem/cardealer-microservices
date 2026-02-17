// =====================================================
// DigitalSignatureService - Handlers
// Ley 339-22 Firma Digital de República Dominicana
// =====================================================

using MediatR;
using DigitalSignatureService.Application.Commands;
using DigitalSignatureService.Application.Queries;
using DigitalSignatureService.Application.DTOs;
using DigitalSignatureService.Domain.Entities;
using DigitalSignatureService.Domain.Interfaces;
using DigitalSignatureService.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace DigitalSignatureService.Application.Handlers;

// ==================== Certificate Handlers ====================

public class CreateCertificateHandler : IRequestHandler<CreateCertificateCommand, DigitalCertificateDto>
{
    private readonly IDigitalCertificateRepository _repository;

    public CreateCertificateHandler(IDigitalCertificateRepository repository) => _repository = repository;

    public async Task<DigitalCertificateDto> Handle(CreateCertificateCommand request, CancellationToken ct)
    {
        var serialNumber = $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        
        var certificate = new DigitalCertificate
        {
            Id = Guid.NewGuid(),
            SerialNumber = serialNumber,
            SubjectName = request.Data.SubjectName,
            SubjectIdentification = request.Data.SubjectIdentification,
            IssuerName = "OGTIC - Autoridad Certificadora RD",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(request.Data.ValidityYears),
            CertificateType = request.Data.CertificateType,
            Status = CertificateStatus.Active,
            PublicKey = GenerateMockPublicKey(),
            UserId = request.Data.UserId,
            OrganizationId = request.Data.OrganizationId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(certificate);

        return new DigitalCertificateDto(
            certificate.Id, certificate.SerialNumber, certificate.SubjectName,
            certificate.SubjectIdentification, certificate.IssuerName, certificate.IssuedAt,
            certificate.ExpiresAt, certificate.CertificateType, certificate.Status,
            certificate.ExpiresAt < DateTime.UtcNow.AddDays(30), certificate.CreatedAt
        );
    }

    private static string GenerateMockPublicKey()
    {
        using var rsa = RSA.Create(2048);
        return Convert.ToBase64String(rsa.ExportRSAPublicKey());
    }
}

public class RevokeCertificateHandler : IRequestHandler<RevokeCertificateCommand, bool>
{
    private readonly IDigitalCertificateRepository _repository;

    public RevokeCertificateHandler(IDigitalCertificateRepository repository) => _repository = repository;

    public async Task<bool> Handle(RevokeCertificateCommand request, CancellationToken ct)
    {
        var certificate = await _repository.GetByIdAsync(request.CertificateId);
        if (certificate == null) return false;

        certificate.Status = CertificateStatus.Revoked;
        certificate.RevocationReason = request.Data.RevocationReason;
        certificate.RevokedAt = DateTime.UtcNow;
        certificate.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(certificate);
        return true;
    }
}

public class GetCertificateByIdHandler : IRequestHandler<GetCertificateByIdQuery, DigitalCertificateDto?>
{
    private readonly IDigitalCertificateRepository _repository;

    public GetCertificateByIdHandler(IDigitalCertificateRepository repository) => _repository = repository;

    public async Task<DigitalCertificateDto?> Handle(GetCertificateByIdQuery request, CancellationToken ct)
    {
        var cert = await _repository.GetByIdAsync(request.Id);
        if (cert == null) return null;

        return new DigitalCertificateDto(
            cert.Id, cert.SerialNumber, cert.SubjectName, cert.SubjectIdentification,
            cert.IssuerName, cert.IssuedAt, cert.ExpiresAt, cert.CertificateType, cert.Status,
            cert.ExpiresAt < DateTime.UtcNow.AddDays(30), cert.CreatedAt
        );
    }
}

public class GetCertificatesByUserHandler : IRequestHandler<GetCertificatesByUserQuery, IEnumerable<DigitalCertificateDto>>
{
    private readonly IDigitalCertificateRepository _repository;

    public GetCertificatesByUserHandler(IDigitalCertificateRepository repository) => _repository = repository;

    public async Task<IEnumerable<DigitalCertificateDto>> Handle(GetCertificatesByUserQuery request, CancellationToken ct)
    {
        var certs = await _repository.GetByUserIdAsync(request.UserId);
        return certs.Select(c => new DigitalCertificateDto(
            c.Id, c.SerialNumber, c.SubjectName, c.SubjectIdentification,
            c.IssuerName, c.IssuedAt, c.ExpiresAt, c.CertificateType, c.Status,
            c.ExpiresAt < DateTime.UtcNow.AddDays(30), c.CreatedAt
        ));
    }
}

public class GetExpiringCertificatesHandler : IRequestHandler<GetExpiringCertificatesQuery, IEnumerable<DigitalCertificateDto>>
{
    private readonly IDigitalCertificateRepository _repository;

    public GetExpiringCertificatesHandler(IDigitalCertificateRepository repository) => _repository = repository;

    public async Task<IEnumerable<DigitalCertificateDto>> Handle(GetExpiringCertificatesQuery request, CancellationToken ct)
    {
        var certs = await _repository.GetExpiringCertificatesAsync(request.DaysAhead);
        return certs.Select(c => new DigitalCertificateDto(
            c.Id, c.SerialNumber, c.SubjectName, c.SubjectIdentification,
            c.IssuerName, c.IssuedAt, c.ExpiresAt, c.CertificateType, c.Status,
            true, c.CreatedAt
        ));
    }
}

// ==================== Signature Handlers ====================

public class SignDocumentHandler : IRequestHandler<SignDocumentCommand, DigitalSignatureDto>
{
    private readonly IDigitalSignatureRepository _signatureRepo;
    private readonly IDigitalCertificateRepository _certificateRepo;
    private readonly IConfiguration _configuration;

    public SignDocumentHandler(
        IDigitalSignatureRepository signatureRepo,
        IDigitalCertificateRepository certificateRepo,
        IConfiguration configuration)
    {
        _signatureRepo = signatureRepo;
        _certificateRepo = certificateRepo;
        _configuration = configuration;
    }

    public async Task<DigitalSignatureDto> Handle(SignDocumentCommand request, CancellationToken ct)
    {
        var certificate = await _certificateRepo.GetByIdAsync(request.Data.CertificateId);
        if (certificate == null || certificate.Status != CertificateStatus.Active)
            throw new InvalidOperationException("El certificado no es válido o no está activo");

        var signatureValue = GenerateSignature(request.Data.DocumentHash, request.Data.Algorithm, _configuration);

        var signature = new DigitalSignature
        {
            Id = Guid.NewGuid(),
            CertificateId = request.Data.CertificateId,
            DocumentId = request.Data.DocumentId,
            DocumentHash = request.Data.DocumentHash,
            SignatureValue = signatureValue,
            SignatureAlgorithm = request.Data.Algorithm,
            SignedAt = DateTime.UtcNow,
            SignerName = certificate.SubjectName,
            SignerIdentification = certificate.SubjectIdentification,
            IpAddress = request.Data.IpAddress,
            IsValid = true,
            CreatedAt = DateTime.UtcNow
        };

        await _signatureRepo.AddAsync(signature);

        return new DigitalSignatureDto(
            signature.Id, signature.CertificateId, signature.DocumentId, signature.DocumentHash,
            signature.SignatureAlgorithm, signature.SignedAt, signature.SignerName,
            signature.SignerIdentification, signature.IsValid, signature.ValidationMessage, signature.ValidatedAt
        );
    }

    private static string GenerateSignature(string hash, SignatureAlgorithm algorithm, IConfiguration configuration)
    {
        // Read HMAC signing key from configuration/secrets — NEVER hardcode
        var hmacKey = configuration["SignatureService:HmacKey"]
            ?? Environment.GetEnvironmentVariable("SIGNATURE_HMAC_KEY")
            ?? throw new InvalidOperationException(
                "HMAC signing key not configured. Set 'SignatureService:HmacKey' in appsettings or 'SIGNATURE_HMAC_KEY' environment variable.");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(hmacKey));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(hash));
        return Convert.ToBase64String(signatureBytes);
    }
}

public class GetSignaturesByDocumentHandler : IRequestHandler<GetSignaturesByDocumentQuery, IEnumerable<DigitalSignatureDto>>
{
    private readonly IDigitalSignatureRepository _repository;

    public GetSignaturesByDocumentHandler(IDigitalSignatureRepository repository) => _repository = repository;

    public async Task<IEnumerable<DigitalSignatureDto>> Handle(GetSignaturesByDocumentQuery request, CancellationToken ct)
    {
        var sigs = await _repository.GetByDocumentIdAsync(request.DocumentId);
        return sigs.Select(s => new DigitalSignatureDto(
            s.Id, s.CertificateId, s.DocumentId, s.DocumentHash, s.SignatureAlgorithm,
            s.SignedAt, s.SignerName, s.SignerIdentification, s.IsValid, s.ValidationMessage, s.ValidatedAt
        ));
    }
}

public class VerifySignatureHandler : IRequestHandler<VerifySignatureCommand, VerificationResultDto>
{
    private readonly IDigitalSignatureRepository _signatureRepo;
    private readonly ISignatureVerificationRepository _verificationRepo;

    public VerifySignatureHandler(IDigitalSignatureRepository signatureRepo, ISignatureVerificationRepository verificationRepo)
    {
        _signatureRepo = signatureRepo;
        _verificationRepo = verificationRepo;
    }

    public async Task<VerificationResultDto> Handle(VerifySignatureCommand request, CancellationToken ct)
    {
        var signature = await _signatureRepo.GetByIdAsync(request.Data.SignatureId);
        if (signature == null)
            return new VerificationResultDto(false, "Firma no encontrada", DateTime.UtcNow, null, null, null);

        // En producción, verificar la firma criptográficamente
        var isValid = signature.IsValid;
        
        var verification = new SignatureVerification
        {
            Id = Guid.NewGuid(),
            SignatureId = signature.Id,
            VerifiedBy = Guid.Empty, // Usuario que verifica
            IsValid = isValid,
            VerificationDetails = isValid ? "Firma válida" : "Firma inválida",
            VerifiedAt = DateTime.UtcNow,
            IpAddress = request.Data.IpAddress
        };

        await _verificationRepo.AddAsync(verification);

        return new VerificationResultDto(
            isValid,
            verification.VerificationDetails,
            verification.VerifiedAt,
            signature.SignerName,
            signature.SignerIdentification,
            signature.SignedAt
        );
    }
}

// ==================== Statistics Handler ====================

public class GetSignatureStatisticsHandler : IRequestHandler<GetSignatureStatisticsQuery, SignatureStatisticsDto>
{
    private readonly IDigitalCertificateRepository _certRepo;
    private readonly IDigitalSignatureRepository _sigRepo;

    public GetSignatureStatisticsHandler(IDigitalCertificateRepository certRepo, IDigitalSignatureRepository sigRepo)
    {
        _certRepo = certRepo;
        _sigRepo = sigRepo;
    }

    public async Task<SignatureStatisticsDto> Handle(GetSignatureStatisticsQuery request, CancellationToken ct)
    {
        var totalCerts = await _certRepo.GetCountAsync();
        var expiring = await _certRepo.GetExpiringCertificatesAsync(30);
        var totalSigs = await _sigRepo.GetCountAsync();

        return new SignatureStatisticsDto(
            TotalCertificates: totalCerts,
            ActiveCertificates: totalCerts, // Simplificado
            ExpiringCertificates: expiring.Count(),
            TotalSignatures: totalSigs,
            ValidSignatures: totalSigs,
            InvalidSignatures: 0
        );
    }
}
