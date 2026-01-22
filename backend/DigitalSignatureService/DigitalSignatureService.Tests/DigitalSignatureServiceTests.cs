// =====================================================
// C7: DigitalSignatureService - Tests Unitarios
// Ley 339-22 Firma Digital de República Dominicana
// =====================================================

using FluentAssertions;
using DigitalSignatureService.Domain.Entities;
using DigitalSignatureService.Domain.Enums;
using Xunit;

namespace DigitalSignatureService.Tests;

public class DigitalSignatureServiceTests
{
    // =====================================================
    // Tests de Certificados Digitales
    // =====================================================

    [Fact]
    public void DigitalCertificate_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var certificate = new DigitalCertificate
        {
            Id = Guid.NewGuid(),
            SerialNumber = "CERT-2026-00001",
            SubjectName = "Juan Pérez García",
            SubjectIdentification = "00112345678",
            IssuerName = "OGTIC - Autoridad Certificadora RD",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(2),
            CertificateType = CertificateType.Personal,
            Status = CertificateStatus.Active,
            PublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A...",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        certificate.Should().NotBeNull();
        certificate.SerialNumber.Should().StartWith("CERT-");
        certificate.ExpiresAt.Should().BeAfter(certificate.IssuedAt);
        certificate.Status.Should().Be(CertificateStatus.Active);
    }

    [Theory]
    [InlineData(CertificateType.Personal)]
    [InlineData(CertificateType.Corporate)]
    [InlineData(CertificateType.Government)]
    [InlineData(CertificateType.SSL)]
    [InlineData(CertificateType.CodeSigning)]
    public void CertificateType_ShouldHaveExpectedValues(CertificateType type)
    {
        // Assert
        Enum.IsDefined(typeof(CertificateType), type).Should().BeTrue();
    }

    [Theory]
    [InlineData(CertificateStatus.Active)]
    [InlineData(CertificateStatus.Expired)]
    [InlineData(CertificateStatus.Revoked)]
    [InlineData(CertificateStatus.Suspended)]
    [InlineData(CertificateStatus.Pending)]
    public void CertificateStatus_ShouldHaveExpectedValues(CertificateStatus status)
    {
        // Assert
        Enum.IsDefined(typeof(CertificateStatus), status).Should().BeTrue();
    }

    // =====================================================
    // Tests de Firma Digital
    // =====================================================

    [Fact]
    public void DigitalSignature_ShouldBeCreated_WithRequiredFields()
    {
        // Arrange & Act
        var signature = new DigitalSignature
        {
            Id = Guid.NewGuid(),
            CertificateId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            SignatureValue = "MEUCIQDx7VxP8J5v3vN...",
            SignatureAlgorithm = SignatureAlgorithm.SHA256withRSA,
            SignedAt = DateTime.UtcNow,
            SignerName = "Juan Pérez",
            SignerIdentification = "00112345678",
            IpAddress = "192.168.1.100",
            IsValid = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        signature.Should().NotBeNull();
        signature.SignatureValue.Should().NotBeNullOrEmpty();
        signature.IsValid.Should().BeTrue();
        signature.IpAddress.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(SignatureAlgorithm.SHA256withRSA)]
    [InlineData(SignatureAlgorithm.SHA384withRSA)]
    [InlineData(SignatureAlgorithm.SHA512withRSA)]
    [InlineData(SignatureAlgorithm.SHA256withECDSA)]
    public void SignatureAlgorithm_ShouldHaveExpectedValues(SignatureAlgorithm algorithm)
    {
        // Assert
        Enum.IsDefined(typeof(SignatureAlgorithm), algorithm).Should().BeTrue();
    }

    // =====================================================
    // Tests de Validación de Firma
    // =====================================================

    [Fact]
    public void SignatureValidation_ShouldReturnValidResult()
    {
        // Arrange & Act
        var validation = new SignatureValidation
        {
            Id = Guid.NewGuid(),
            SignatureId = Guid.NewGuid(),
            ValidationDate = DateTime.UtcNow,
            IsSignatureValid = true,
            IsCertificateValid = true,
            IsChainValid = true,
            IsNotRevoked = true,
            IsWithinValidity = true,
            OverallResult = ValidationResult.Valid,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        validation.OverallResult.Should().Be(ValidationResult.Valid);
        validation.IsSignatureValid.Should().BeTrue();
        validation.IsCertificateValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(ValidationResult.Valid)]
    [InlineData(ValidationResult.Invalid)]
    [InlineData(ValidationResult.Indeterminate)]
    [InlineData(ValidationResult.CertificateExpired)]
    [InlineData(ValidationResult.CertificateRevoked)]
    public void ValidationResult_ShouldHaveExpectedValues(ValidationResult result)
    {
        // Assert
        Enum.IsDefined(typeof(ValidationResult), result).Should().BeTrue();
    }

    // =====================================================
    // Tests de Documento Firmado
    // =====================================================

    [Fact]
    public void SignedDocument_ShouldBeCreated_WithMetadata()
    {
        // Arrange & Act
        var document = new SignedDocument
        {
            Id = Guid.NewGuid(),
            DocumentName = "Contrato de Compraventa",
            DocumentType = DocumentType.Contract,
            OriginalHash = "a3f5c8d9e2b1a4f7...",
            HashAlgorithm = HashAlgorithm.SHA256,
            FileSize = 1024 * 1024, // 1MB
            MimeType = "application/pdf",
            SignaturesCount = 2,
            IsFullySigned = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        document.DocumentName.Should().NotBeNullOrEmpty();
        document.OriginalHash.Should().NotBeNullOrEmpty();
        document.SignaturesCount.Should().Be(2);
        document.IsFullySigned.Should().BeTrue();
    }

    [Theory]
    [InlineData(DocumentType.Contract)]
    [InlineData(DocumentType.Invoice)]
    [InlineData(DocumentType.LegalDocument)]
    [InlineData(DocumentType.Certificate)]
    [InlineData(DocumentType.Report)]
    public void DocumentType_ShouldHaveExpectedValues(DocumentType type)
    {
        // Assert
        Enum.IsDefined(typeof(DocumentType), type).Should().BeTrue();
    }

    // =====================================================
    // Tests de Estampado de Tiempo (Timestamp)
    // =====================================================

    [Fact]
    public void Timestamp_ShouldBeCreated_WithTSAInfo()
    {
        // Arrange & Act
        var timestamp = new Timestamp
        {
            Id = Guid.NewGuid(),
            SignatureId = Guid.NewGuid(),
            TsaName = "OGTIC TSA",
            TsaSerialNumber = "TSA-2026-00001",
            TimestampValue = DateTime.UtcNow,
            HashValue = "b4e7c9f2a1d3e5...",
            HashAlgorithm = HashAlgorithm.SHA256,
            IsValid = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        timestamp.TsaName.Should().NotBeNullOrEmpty();
        timestamp.TimestampValue.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        timestamp.IsValid.Should().BeTrue();
    }

    // =====================================================
    // Tests de Revocación de Certificado
    // =====================================================

    [Fact]
    public void CertificateRevocation_ShouldBeCreated_WithReason()
    {
        // Arrange & Act
        var revocation = new CertificateRevocation
        {
            Id = Guid.NewGuid(),
            CertificateId = Guid.NewGuid(),
            RevocationDate = DateTime.UtcNow,
            RevocationReason = RevocationReason.KeyCompromise,
            RequestedBy = "Usuario",
            ApprovedBy = "Administrador CA",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        revocation.RevocationReason.Should().Be(RevocationReason.KeyCompromise);
        revocation.RevocationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Theory]
    [InlineData(RevocationReason.Unspecified)]
    [InlineData(RevocationReason.KeyCompromise)]
    [InlineData(RevocationReason.CACompromise)]
    [InlineData(RevocationReason.AffiliationChanged)]
    [InlineData(RevocationReason.Superseded)]
    [InlineData(RevocationReason.CessationOfOperation)]
    public void RevocationReason_ShouldHaveExpectedValues(RevocationReason reason)
    {
        // Assert
        Enum.IsDefined(typeof(RevocationReason), reason).Should().BeTrue();
    }

    // =====================================================
    // Tests de Validez Legal (Ley 339-22)
    // =====================================================

    [Fact]
    public void DigitalSignature_ShouldHaveLegalValidity()
    {
        // Arrange
        var signature = new DigitalSignature
        {
            Id = Guid.NewGuid(),
            SignatureAlgorithm = SignatureAlgorithm.SHA256withRSA,
            IsValid = true,
            HasTimestamp = true,
            HasValidCertificate = true
        };

        // Act
        var hasLegalValidity = signature.IsValid && 
                               signature.HasTimestamp && 
                               signature.HasValidCertificate;

        // Assert - Ley 339-22 requiere firma válida con timestamp y certificado válido
        hasLegalValidity.Should().BeTrue();
    }

    // =====================================================
    // Tests de Múltiples Firmantes
    // =====================================================

    [Fact]
    public void Document_CanHaveMultipleSignatures()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var signatures = new List<DigitalSignature>
        {
            new DigitalSignature { Id = Guid.NewGuid(), DocumentId = documentId, SignerName = "Vendedor", SignerRole = SignerRole.Seller },
            new DigitalSignature { Id = Guid.NewGuid(), DocumentId = documentId, SignerName = "Comprador", SignerRole = SignerRole.Buyer },
            new DigitalSignature { Id = Guid.NewGuid(), DocumentId = documentId, SignerName = "Testigo", SignerRole = SignerRole.Witness }
        };

        // Assert
        signatures.Should().HaveCount(3);
        signatures.Select(s => s.SignerRole).Should().Contain(SignerRole.Seller);
        signatures.Select(s => s.SignerRole).Should().Contain(SignerRole.Buyer);
    }

    [Theory]
    [InlineData(SignerRole.Seller)]
    [InlineData(SignerRole.Buyer)]
    [InlineData(SignerRole.Witness)]
    [InlineData(SignerRole.Notary)]
    [InlineData(SignerRole.Representative)]
    public void SignerRole_ShouldHaveExpectedValues(SignerRole role)
    {
        // Assert
        Enum.IsDefined(typeof(SignerRole), role).Should().BeTrue();
    }
}
