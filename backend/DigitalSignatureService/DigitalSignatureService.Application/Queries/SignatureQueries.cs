// =====================================================
// DigitalSignatureService - Queries
// Ley 339-22 Firma Digital de República Dominicana
// =====================================================

using MediatR;
using DigitalSignatureService.Application.DTOs;

namespace DigitalSignatureService.Application.Queries;

// ==================== Certificados ====================
public record GetCertificateByIdQuery(Guid Id) : IRequest<DigitalCertificateDto?>;
public record GetCertificateBySerialQuery(string SerialNumber) : IRequest<DigitalCertificateDto?>;
public record GetCertificatesByUserQuery(Guid UserId) : IRequest<IEnumerable<DigitalCertificateDto>>;
public record GetActiveCertificatesByUserQuery(Guid UserId) : IRequest<IEnumerable<DigitalCertificateDto>>;
public record GetExpiringCertificatesQuery(int DaysAhead = 30) : IRequest<IEnumerable<DigitalCertificateDto>>;

// ==================== Firmas ====================
public record GetSignatureByIdQuery(Guid Id) : IRequest<DigitalSignatureDto?>;
public record GetSignaturesByDocumentQuery(Guid DocumentId) : IRequest<IEnumerable<DigitalSignatureDto>>;
public record GetSignaturesByCertificateQuery(Guid CertificateId) : IRequest<IEnumerable<DigitalSignatureDto>>;
public record GetSignaturesBySignerQuery(string Identification) : IRequest<IEnumerable<DigitalSignatureDto>>;

// ==================== Verificación ====================
public record GetVerificationHistoryQuery(Guid SignatureId) : IRequest<IEnumerable<VerificationResultDto>>;

// ==================== TimeStamp ====================
public record GetTimeStampBySignatureQuery(Guid SignatureId) : IRequest<TimeStampDto?>;

// ==================== Estadísticas ====================
public record GetSignatureStatisticsQuery() : IRequest<SignatureStatisticsDto>;
