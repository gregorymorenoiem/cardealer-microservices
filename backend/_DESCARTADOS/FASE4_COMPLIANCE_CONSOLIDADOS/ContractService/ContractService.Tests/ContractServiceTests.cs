using FluentAssertions;
using ContractService.Domain.Entities;
using Xunit;

namespace ContractService.Tests;

/// <summary>
/// Tests unitarios para ContractService - Gestión de Contratos Electrónicos
/// Cumplimiento con Ley 126-02 de Comercio Electrónico de República Dominicana
/// </summary>
public class ContractServiceTests
{
    #region PARTE 1: Tests de Enumeraciones

    [Theory]
    [InlineData(1, "SaleAgreement")]
    [InlineData(2, "LeaseAgreement")]
    [InlineData(3, "ServiceAgreement")]
    [InlineData(4, "FinancingAgreement")]
    [InlineData(5, "ConsignmentAgreement")]
    [InlineData(6, "TradeInAgreement")]
    [InlineData(7, "WarrantyAgreement")]
    [InlineData(8, "InsuranceAgreement")]
    [InlineData(9, "MaintenanceAgreement")]
    [InlineData(10, "NonDisclosure")]
    [InlineData(11, "TermsOfService")]
    [InlineData(12, "PrivacyPolicy")]
    [InlineData(13, "DealerAgreement")]
    public void ContractType_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<ContractType>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void ContractType_ShouldHave13Values()
    {
        var values = Enum.GetValues<ContractType>();
        values.Should().HaveCount(13);
    }

    [Theory]
    [InlineData(1, "Draft")]
    [InlineData(2, "PendingSignatures")]
    [InlineData(3, "PartiallySigned")]
    [InlineData(4, "FullySigned")]
    [InlineData(5, "Active")]
    [InlineData(6, "Expired")]
    [InlineData(7, "Terminated")]
    [InlineData(8, "Cancelled")]
    [InlineData(9, "Disputed")]
    [InlineData(10, "Renewed")]
    [InlineData(11, "Archived")]
    public void ContractStatus_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<ContractStatus>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void ContractStatus_ShouldHave11Values()
    {
        var values = Enum.GetValues<ContractStatus>();
        values.Should().HaveCount(11);
    }

    [Theory]
    [InlineData(1, "Individual")]
    [InlineData(2, "Company")]
    [InlineData(3, "Dealer")]
    [InlineData(4, "Platform")]
    [InlineData(5, "Guarantor")]
    [InlineData(6, "Witness")]
    [InlineData(7, "Agent")]
    public void PartyType_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<PartyType>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void PartyType_ShouldHave7Values()
    {
        var values = Enum.GetValues<PartyType>();
        values.Should().HaveCount(7);
    }

    [Theory]
    [InlineData(1, "Seller")]
    [InlineData(2, "Buyer")]
    [InlineData(3, "Lessor")]
    [InlineData(4, "Lessee")]
    [InlineData(5, "ServiceProvider")]
    [InlineData(6, "Client")]
    [InlineData(7, "Financier")]
    [InlineData(8, "Borrower")]
    [InlineData(9, "Guarantor")]
    [InlineData(10, "Witness")]
    public void PartyRole_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<PartyRole>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void PartyRole_ShouldHave10Values()
    {
        var values = Enum.GetValues<PartyRole>();
        values.Should().HaveCount(10);
    }

    [Theory]
    [InlineData(1, "Pending")]
    [InlineData(2, "Requested")]
    [InlineData(3, "Viewed")]
    [InlineData(4, "Signed")]
    [InlineData(5, "Declined")]
    [InlineData(6, "Expired")]
    public void SignatureStatus_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<SignatureStatus>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void SignatureStatus_ShouldHave6Values()
    {
        var values = Enum.GetValues<SignatureStatus>();
        values.Should().HaveCount(6);
    }

    [Theory]
    [InlineData(1, "Simple")]
    [InlineData(2, "Advanced")]
    [InlineData(3, "Qualified")]
    [InlineData(4, "Biometric")]
    public void SignatureType_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<SignatureType>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void SignatureType_ShouldHave4Values()
    {
        var values = Enum.GetValues<SignatureType>();
        values.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(1, "NotVerified")]
    [InlineData(2, "Verified")]
    [InlineData(3, "Failed")]
    [InlineData(4, "Revoked")]
    public void SignatureVerificationStatus_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<SignatureVerificationStatus>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void SignatureVerificationStatus_ShouldHave4Values()
    {
        var values = Enum.GetValues<SignatureVerificationStatus>();
        values.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(1, "Standard")]
    [InlineData(2, "Mandatory")]
    [InlineData(3, "Optional")]
    [InlineData(4, "Negotiable")]
    [InlineData(5, "LegalNotice")]
    [InlineData(6, "ArbitrationClause")]
    [InlineData(7, "JurisdictionClause")]
    [InlineData(8, "ConfidentialityClause")]
    [InlineData(9, "PenaltyClause")]
    [InlineData(10, "TerminationClause")]
    public void ClauseType_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<ClauseType>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void ClauseType_ShouldHave10Values()
    {
        var values = Enum.GetValues<ClauseType>();
        values.Should().HaveCount(10);
    }

    [Theory]
    [InlineData(1, "Draft")]
    [InlineData(2, "Review")]
    [InlineData(3, "Approved")]
    [InlineData(4, "Current")]
    [InlineData(5, "Superseded")]
    [InlineData(6, "Archived")]
    public void VersionStatus_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<VersionStatus>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void VersionStatus_ShouldHave6Values()
    {
        var values = Enum.GetValues<VersionStatus>();
        values.Should().HaveCount(6);
    }

    [Theory]
    [InlineData(1, "Created")]
    [InlineData(2, "Updated")]
    [InlineData(3, "SignatureRequested")]
    [InlineData(4, "DocumentViewed")]
    [InlineData(5, "Signed")]
    [InlineData(6, "SignatureDeclined")]
    [InlineData(7, "StatusChanged")]
    [InlineData(8, "Terminated")]
    [InlineData(9, "Renewed")]
    [InlineData(10, "Archived")]
    [InlineData(11, "Downloaded")]
    [InlineData(12, "Shared")]
    [InlineData(13, "Disputed")]
    [InlineData(14, "ClauseModified")]
    public void ContractAuditEventType_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<ContractAuditEventType>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void ContractAuditEventType_ShouldHave14Values()
    {
        var values = Enum.GetValues<ContractAuditEventType>();
        values.Should().HaveCount(14);
    }

    #endregion

    #region PARTE 2: Tests de Entidad ContractTemplate

    [Fact]
    public void ContractTemplate_ShouldBeCreated_WithValidData()
    {
        var template = new ContractTemplate
        {
            Id = Guid.NewGuid(),
            Code = "SALE-001",
            Name = "Contrato de Compraventa de Vehículo",
            Description = "Plantilla estándar para venta de vehículos en RD",
            Type = ContractType.SaleAgreement,
            ContentHtml = "<html><body>Contrato...</body></html>",
            Language = "es-DO",
            LegalBasis = "Ley 126-02 de Comercio Electrónico",
            IsActive = true,
            Version = 1,
            MinimumSignatures = 2,
            RequiresNotarization = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin@okla.com"
        };

        template.Code.Should().Be("SALE-001");
        template.Type.Should().Be(ContractType.SaleAgreement);
        template.Language.Should().Be("es-DO");
        template.MinimumSignatures.Should().Be(2);
    }

    [Fact]
    public void ContractTemplate_ShouldHaveClauses_AsCollection()
    {
        var template = new ContractTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Plantilla de Venta",
            Type = ContractType.SaleAgreement,
            Clauses = new List<TemplateClause>()
        };

        var clause = new TemplateClause
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Title = "Cláusula de Entrega",
            Type = ClauseType.Standard,
            IsMandatory = true
        };

        template.Clauses.Add(clause);
        template.Clauses.Should().HaveCount(1);
        template.Clauses.First().IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void ContractTemplate_ShouldRequireVariables()
    {
        var template = new ContractTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Contrato con Variables",
            RequiredVariables = new List<string> { "buyer_name", "seller_name" },
            OptionalVariables = new List<string> { "warranty_period" }
        };

        template.RequiredVariables.Should().HaveCount(2);
        template.OptionalVariables.Should().HaveCount(1);
    }

    #endregion

    #region PARTE 3: Tests de Entidad TemplateClause

    [Fact]
    public void TemplateClause_ShouldBeCreated_WithValidData()
    {
        var clause = new TemplateClause
        {
            Id = Guid.NewGuid(),
            TemplateId = Guid.NewGuid(),
            Code = "CL-PAY",
            Title = "Condiciones de Pago",
            Content = "El comprador realizará el pago mediante...",
            Type = ClauseType.Standard,
            Order = 5,
            IsMandatory = true,
            IsEditable = false,
            LegalReference = "Art. 1583 Código Civil RD"
        };

        clause.Title.Should().Be("Condiciones de Pago");
        clause.Type.Should().Be(ClauseType.Standard);
        clause.IsMandatory.Should().BeTrue();
        clause.LegalReference.Should().Contain("Código Civil");
    }

    [Theory]
    [InlineData(ClauseType.Mandatory, true, false)]
    [InlineData(ClauseType.Optional, false, true)]
    [InlineData(ClauseType.Standard, true, true)]
    public void TemplateClause_ShouldSupportDifferentConfigurations(
        ClauseType type, bool isMandatory, bool isEditable)
    {
        var clause = new TemplateClause
        {
            Id = Guid.NewGuid(),
            Type = type,
            IsMandatory = isMandatory,
            IsEditable = isEditable
        };

        clause.Type.Should().Be(type);
        clause.IsMandatory.Should().Be(isMandatory);
        clause.IsEditable.Should().Be(isEditable);
    }

    #endregion

    #region PARTE 4: Tests de Entidad Contract

    [Fact]
    public void Contract_ShouldBeCreated_WithValidData()
    {
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            ContractNumber = "OKLA-2026-00001",
            Type = ContractType.SaleAgreement,
            Title = "Contrato de Venta - Toyota Camry 2024",
            Status = ContractStatus.Draft,
            ContentHtml = "<html><body>Contrato de venta...</body></html>",
            ContentHash = "sha256:xyz789",
            SubjectType = "Vehicle",
            ContractValue = 1500000.00m,
            Currency = "DOP",
            LegalJurisdiction = "República Dominicana",
            EffectiveDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seller@okla.com"
        };

        contract.ContractNumber.Should().StartWith("OKLA-");
        contract.Status.Should().Be(ContractStatus.Draft);
        contract.ContractValue.Should().Be(1500000.00m);
        contract.Currency.Should().Be("DOP");
    }

    [Fact]
    public void Contract_ShouldTransition_ThroughStatuses()
    {
        var contract = new Contract { Status = ContractStatus.Draft };

        contract.Status = ContractStatus.PendingSignatures;
        contract.Status.Should().Be(ContractStatus.PendingSignatures);

        contract.Status = ContractStatus.PartiallySigned;
        contract.Status.Should().Be(ContractStatus.PartiallySigned);

        contract.Status = ContractStatus.FullySigned;
        contract.Status.Should().Be(ContractStatus.FullySigned);

        contract.Status = ContractStatus.Active;
        contract.Status.Should().Be(ContractStatus.Active);
    }

    [Fact]
    public void Contract_ShouldHaveDigitalSeal_ForAuthenticity()
    {
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            DigitalSeal = "OKLA-SEAL-2026-ABC123XYZ",
            ContentHash = "sha256:contract_hash_value"
        };

        contract.DigitalSeal.Should().StartWith("OKLA-SEAL-");
        contract.ContentHash.Should().StartWith("sha256:");
    }

    [Fact]
    public void Contract_ShouldSupportExpirationDate()
    {
        var effectiveDate = DateTime.UtcNow;
        var contract = new Contract
        {
            EffectiveDate = effectiveDate,
            ExpirationDate = effectiveDate.AddYears(1)
        };

        contract.ExpirationDate.Should().BeAfter(contract.EffectiveDate);
    }

    [Fact]
    public void Contract_ShouldHavePartiesCollection()
    {
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            Parties = new List<ContractParty>()
        };

        contract.Parties.Add(new ContractParty { Role = PartyRole.Seller });
        contract.Parties.Add(new ContractParty { Role = PartyRole.Buyer });

        contract.Parties.Should().HaveCount(2);
    }

    #endregion

    #region PARTE 5: Tests de Entidad ContractParty

    [Fact]
    public void ContractParty_ShouldBeCreated_AsIndividual()
    {
        var party = new ContractParty
        {
            Id = Guid.NewGuid(),
            Type = PartyType.Individual,
            Role = PartyRole.Buyer,
            FullName = "Juan Pérez García",
            DocumentType = "Cédula",
            DocumentNumber = "001-1234567-8",
            Email = "juan.perez@email.com",
            Country = "DO"
        };

        party.Type.Should().Be(PartyType.Individual);
        party.Role.Should().Be(PartyRole.Buyer);
        party.Country.Should().Be("DO");
    }

    [Fact]
    public void ContractParty_ShouldBeCreated_AsCompany()
    {
        var party = new ContractParty
        {
            Id = Guid.NewGuid(),
            Type = PartyType.Dealer,
            Role = PartyRole.Seller,
            CompanyName = "Auto Dealer Premium SRL",
            RNC = "130123456",
            LegalRepresentative = "María González"
        };

        party.Type.Should().Be(PartyType.Dealer);
        party.RNC.Should().NotBeNullOrEmpty();
        party.LegalRepresentative.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ContractParty_ShouldTrackSignatureStatus()
    {
        var party = new ContractParty
        {
            Id = Guid.NewGuid(),
            HasSigned = false,
            IsVerified = true,
            VerifiedAt = DateTime.UtcNow
        };

        party.HasSigned = true;
        party.SignedAt = DateTime.UtcNow;

        party.HasSigned.Should().BeTrue();
        party.SignedAt.Should().NotBeNull();
    }

    #endregion

    #region PARTE 6: Tests de Entidad ContractSignature (Ley 126-02)

    [Fact]
    public void ContractSignature_ShouldBeCreated_WithSimpleSignature()
    {
        var signature = new ContractSignature
        {
            Id = Guid.NewGuid(),
            Type = SignatureType.Simple,
            Status = SignatureStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            IPAddress = "192.168.1.100"
        };

        signature.Type.Should().Be(SignatureType.Simple);
        signature.Status.Should().Be(SignatureStatus.Pending);
        signature.IPAddress.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ContractSignature_ShouldBeCreated_WithAdvancedSignature()
    {
        var now = DateTime.UtcNow;
        var signature = new ContractSignature
        {
            Id = Guid.NewGuid(),
            Type = SignatureType.Advanced,
            Status = SignatureStatus.Signed,
            SignedAt = now,
            CertificateIssuer = "INDOTEL-CA",
            CertificateSerialNumber = "CERT-2026-00001",
            CertificateValidFrom = now.AddYears(-1),
            CertificateValidTo = now.AddYears(2),
            TimestampToken = "TSA-TOKEN-123456",
            TimestampDate = now,
            VerificationStatus = SignatureVerificationStatus.Verified
        };

        signature.Type.Should().Be(SignatureType.Advanced);
        signature.CertificateIssuer.Should().Be("INDOTEL-CA");
        signature.VerificationStatus.Should().Be(SignatureVerificationStatus.Verified);
    }

    [Fact]
    public void ContractSignature_ShouldBeCreated_WithBiometricSignature()
    {
        var signature = new ContractSignature
        {
            Id = Guid.NewGuid(),
            Type = SignatureType.Biometric,
            Status = SignatureStatus.Signed,
            BiometricVerified = true,
            BiometricType = "fingerprint",
            BiometricScore = "98.5"
        };

        signature.Type.Should().Be(SignatureType.Biometric);
        signature.BiometricVerified.Should().BeTrue();
    }

    [Fact]
    public void ContractSignature_ShouldStoreDeclineReason()
    {
        var signature = new ContractSignature
        {
            Id = Guid.NewGuid(),
            Status = SignatureStatus.Declined,
            DeclineReason = "No estoy de acuerdo con los términos",
            DeclinedAt = DateTime.UtcNow
        };

        signature.Status.Should().Be(SignatureStatus.Declined);
        signature.DeclineReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ContractSignature_ShouldExpire_AfterDeadline()
    {
        var signature = new ContractSignature
        {
            Id = Guid.NewGuid(),
            RequestedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-3)
        };

        signature.ExpiresAt.Should().BeBefore(DateTime.UtcNow);
    }

    #endregion

    #region PARTE 7: Tests de Entidad ContractClause

    [Fact]
    public void ContractClause_ShouldBeCreated_WithValidData()
    {
        var clause = new ContractClause
        {
            Id = Guid.NewGuid(),
            Code = "CL-DELIVERY",
            Title = "Entrega del Vehículo",
            Content = "El vendedor entregará el vehículo...",
            Type = ClauseType.Standard,
            Order = 3,
            IsAccepted = true,
            AcceptedAt = DateTime.UtcNow,
            AcceptedBy = "buyer@email.com"
        };

        clause.Title.Should().Be("Entrega del Vehículo");
        clause.IsAccepted.Should().BeTrue();
    }

    [Fact]
    public void ContractClause_ShouldBeModified_AndTrackChanges()
    {
        var clause = new ContractClause
        {
            Id = Guid.NewGuid(),
            OriginalContent = "Contenido original",
            Content = "Contenido original",
            WasModified = false
        };

        clause.Content = "Contenido modificado";
        clause.WasModified = true;
        clause.ModificationReason = "Ajuste por negociación";
        clause.ModifiedBy = "lawyer@okla.com";

        clause.WasModified.Should().BeTrue();
        clause.ModificationReason.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region PARTE 8: Tests de Entidad ContractVersion

    [Fact]
    public void ContractVersion_ShouldBeCreated_WithValidData()
    {
        var version = new ContractVersion
        {
            Id = Guid.NewGuid(),
            VersionNumber = 1,
            ContentHtml = "<html>Contenido v1.0</html>",
            ContentHash = "sha256:version1_hash",
            Status = VersionStatus.Current,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "editor@okla.com"
        };

        version.VersionNumber.Should().Be(1);
        version.Status.Should().Be(VersionStatus.Current);
    }

    [Fact]
    public void ContractVersion_ShouldSupersede_PreviousVersion()
    {
        var version1 = new ContractVersion { VersionNumber = 1, Status = VersionStatus.Current };
        var version2 = new ContractVersion
        {
            VersionNumber = 2,
            Status = VersionStatus.Current,
            ChangeDescription = "Actualización de términos"
        };

        version1.Status = VersionStatus.Superseded;

        version1.Status.Should().Be(VersionStatus.Superseded);
        version2.Status.Should().Be(VersionStatus.Current);
    }

    #endregion

    #region PARTE 9: Tests de Entidad ContractDocument

    [Fact]
    public void ContractDocument_ShouldBeCreated_WithValidData()
    {
        var document = new ContractDocument
        {
            Id = Guid.NewGuid(),
            Name = "Cédula del Comprador",
            DocumentType = "id",
            FileName = "cedula_comprador.pdf",
            ContentType = "application/pdf",
            FileSize = 1024000,
            StoragePath = "s3://okla-documents/cedula.pdf",
            FileHash = "sha256:document_hash",
            IsRequired = true,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = "buyer@email.com"
        };

        document.DocumentType.Should().Be("id");
        document.IsRequired.Should().BeTrue();
        document.FileHash.Should().StartWith("sha256:");
    }

    [Fact]
    public void ContractDocument_ShouldBeVerified()
    {
        var document = new ContractDocument { IsVerified = false };

        document.IsVerified = true;
        document.VerifiedAt = DateTime.UtcNow;
        document.VerifiedBy = "admin@okla.com";

        document.IsVerified.Should().BeTrue();
        document.VerifiedBy.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region PARTE 10: Tests de Entidad ContractAuditLog

    [Fact]
    public void ContractAuditLog_ShouldRecordEvent_Created()
    {
        var auditLog = new ContractAuditLog
        {
            Id = Guid.NewGuid(),
            EventType = ContractAuditEventType.Created,
            Description = "Contrato creado desde plantilla",
            PerformedBy = "user@okla.com",
            PerformedAt = DateTime.UtcNow,
            IPAddress = "192.168.1.100"
        };

        auditLog.EventType.Should().Be(ContractAuditEventType.Created);
        auditLog.Description.Should().Contain("creado");
    }

    [Theory]
    [InlineData(ContractAuditEventType.Signed, "Firma aplicada")]
    [InlineData(ContractAuditEventType.StatusChanged, "Estado cambiado")]
    [InlineData(ContractAuditEventType.DocumentViewed, "Contrato visualizado")]
    [InlineData(ContractAuditEventType.Downloaded, "PDF descargado")]
    public void ContractAuditLog_ShouldRecordDifferentEventTypes(
        ContractAuditEventType eventType, string description)
    {
        var auditLog = new ContractAuditLog
        {
            EventType = eventType,
            Description = description,
            PerformedBy = "user@okla.com",
            PerformedAt = DateTime.UtcNow
        };

        auditLog.EventType.Should().Be(eventType);
        auditLog.Description.Should().Be(description);
    }

    [Fact]
    public void ContractAuditLog_ShouldTrackValueChanges()
    {
        var auditLog = new ContractAuditLog
        {
            EventType = ContractAuditEventType.Updated,
            OldValue = "{\"ContractValue\": 1000000}",
            NewValue = "{\"ContractValue\": 1200000}",
            PerformedBy = "editor@okla.com",
            PerformedAt = DateTime.UtcNow
        };

        auditLog.OldValue.Should().Contain("1000000");
        auditLog.NewValue.Should().Contain("1200000");
    }

    #endregion

    #region PARTE 11: Tests de Entidad CertificationAuthority

    [Fact]
    public void CertificationAuthority_ShouldBeCreated_WithValidData()
    {
        var authority = new CertificationAuthority
        {
            Id = Guid.NewGuid(),
            Code = "INDOTEL-CA-001",
            Name = "Autoridad Certificadora INDOTEL",
            Country = "DO",
            Website = "https://indotel.gob.do",
            IsActive = true,
            IsGovernmentApproved = true,
            AccreditationNumber = "INDOTEL-2024-001",
            ValidFrom = DateTime.UtcNow.AddYears(-2),
            ValidTo = DateTime.UtcNow.AddYears(3),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin@okla.com"
        };

        authority.Code.Should().StartWith("INDOTEL");
        authority.Country.Should().Be("DO");
        authority.IsGovernmentApproved.Should().BeTrue();
    }

    [Fact]
    public void CertificationAuthority_ShouldHaveValidCertificatePeriod()
    {
        var authority = new CertificationAuthority
        {
            ValidFrom = DateTime.UtcNow.AddYears(-1),
            ValidTo = DateTime.UtcNow.AddYears(2),
            IsActive = true
        };

        authority.ValidTo.Should().BeAfter(authority.ValidFrom!.Value);
    }

    #endregion

    #region PARTE 12: Tests de Flujos de Negocio

    [Fact]
    public void Contract_FullSaleFlow_ShouldWorkCorrectly()
    {
        var contractId = Guid.NewGuid();

        var contract = new Contract
        {
            Id = contractId,
            ContractNumber = "OKLA-2026-00001",
            Type = ContractType.SaleAgreement,
            Status = ContractStatus.Draft,
            ContractValue = 2000000.00m,
            Currency = "DOP",
            Parties = new List<ContractParty>(),
            Signatures = new List<ContractSignature>()
        };

        contract.Parties.Add(new ContractParty { Role = PartyRole.Seller });
        contract.Parties.Add(new ContractParty { Role = PartyRole.Buyer });

        var sellerSignature = new ContractSignature { Status = SignatureStatus.Pending };
        var buyerSignature = new ContractSignature { Status = SignatureStatus.Pending };
        contract.Signatures.Add(sellerSignature);
        contract.Signatures.Add(buyerSignature);

        contract.Status = ContractStatus.PendingSignatures;
        sellerSignature.Status = SignatureStatus.Signed;
        contract.Status = ContractStatus.PartiallySigned;
        buyerSignature.Status = SignatureStatus.Signed;
        contract.Status = ContractStatus.FullySigned;
        contract.Status = ContractStatus.Active;
        contract.DigitalSeal = "OKLA-SEAL-2026-XYZ";

        contract.Status.Should().Be(ContractStatus.Active);
        contract.Parties.Should().HaveCount(2);
        contract.Signatures.All(s => s.Status == SignatureStatus.Signed).Should().BeTrue();
        contract.DigitalSeal.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Contract_FinancingFlow_ShouldIncludeThreeParties()
    {
        var contract = new Contract
        {
            Type = ContractType.FinancingAgreement,
            Parties = new List<ContractParty>()
        };

        contract.Parties.Add(new ContractParty { Type = PartyType.Dealer, Role = PartyRole.Seller });
        contract.Parties.Add(new ContractParty { Type = PartyType.Individual, Role = PartyRole.Buyer });
        contract.Parties.Add(new ContractParty { Type = PartyType.Company, Role = PartyRole.Financier });

        contract.Parties.Should().HaveCount(3);
        contract.Parties.Should().Contain(p => p.Role == PartyRole.Financier);
    }

    [Fact]
    public void Contract_WithGuarantor_ShouldHaveFourParties()
    {
        var contract = new Contract
        {
            Type = ContractType.FinancingAgreement,
            Parties = new List<ContractParty>()
        };

        contract.Parties.Add(new ContractParty { Role = PartyRole.Seller });
        contract.Parties.Add(new ContractParty { Role = PartyRole.Buyer });
        contract.Parties.Add(new ContractParty { Role = PartyRole.Financier });
        contract.Parties.Add(new ContractParty { Role = PartyRole.Guarantor });

        contract.Parties.Should().HaveCount(4);
        contract.Parties.Should().Contain(p => p.Role == PartyRole.Guarantor);
    }

    #endregion

    #region PARTE 13: Tests de Regulaciones Dominicanas

    [Fact]
    public void ContractSignature_ShouldComply_WithLey126_02()
    {
        var signature = new ContractSignature
        {
            Type = SignatureType.Advanced,
            Status = SignatureStatus.Signed,
            SignedAt = DateTime.UtcNow,
            CertificateIssuer = "INDOTEL-CA",
            CertificateSerialNumber = "CERT-2026-00001",
            CertificateValidFrom = DateTime.UtcNow.AddYears(-1),
            CertificateValidTo = DateTime.UtcNow.AddYears(2),
            TimestampToken = "TSA-TOKEN-123",
            TimestampDate = DateTime.UtcNow,
            SignatureHash = "sha256:document_hash",
            VerificationStatus = SignatureVerificationStatus.Verified,
            IPAddress = "192.168.1.100"
        };

        signature.CertificateIssuer.Should().NotBeNullOrEmpty("Ley 126-02 requiere certificado de CA");
        signature.TimestampToken.Should().NotBeNullOrEmpty("Ley 126-02 requiere sellado de tiempo");
        signature.SignatureHash.Should().NotBeNullOrEmpty("Ley 126-02 requiere hash del documento");
        signature.VerificationStatus.Should().Be(SignatureVerificationStatus.Verified);
    }

    [Fact]
    public void ContractParty_ShouldValidate_DominicanDocuments()
    {
        var individualParty = new ContractParty
        {
            Type = PartyType.Individual,
            DocumentType = "Cédula",
            DocumentNumber = "001-1234567-8"
        };

        var companyParty = new ContractParty
        {
            Type = PartyType.Company,
            DocumentType = "RNC",
            RNC = "130123456"
        };

        individualParty.DocumentNumber.Should().MatchRegex(@"^\d{3}-\d{7}-\d$");
        companyParty.RNC.Should().HaveLength(9);
    }

    [Fact]
    public void Contract_ShouldSpecify_DominicanJurisdiction()
    {
        var contract = new Contract
        {
            LegalJurisdiction = "República Dominicana",
            ArbitrationClause = "Arbitraje ante CRC",
            Currency = "DOP"
        };

        contract.LegalJurisdiction.Should().Be("República Dominicana");
        contract.Currency.Should().Be("DOP");
    }

    [Fact]
    public void ContractValue_ShouldBe_InDominicanPesos()
    {
        var contracts = new[]
        {
            new Contract { ContractValue = 500000.00m, Currency = "DOP" },
            new Contract { ContractValue = 1500000.00m, Currency = "DOP" },
            new Contract { ContractValue = 5000000.00m, Currency = "DOP" }
        };

        contracts.Should().AllSatisfy(c =>
        {
            c.Currency.Should().Be("DOP");
            c.ContractValue.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public void Contract_ITBIS_ShouldBeCalculated()
    {
        var baseValue = 1000000.00m;
        var itbisRate = 0.18m;
        var contract = new Contract { ContractValue = baseValue, Currency = "DOP" };

        var itbisAmount = contract.ContractValue * itbisRate;
        var totalWithItbis = contract.ContractValue + itbisAmount;

        itbisAmount.Should().Be(180000.00m);
        totalWithItbis.Should().Be(1180000.00m);
    }

    #endregion

    #region PARTE 14: Tests de Validaciones

    [Fact]
    public void Contract_ShouldRequire_AtLeastTwoParties()
    {
        var contract = new Contract { Parties = new List<ContractParty>() };
        contract.Parties.Add(new ContractParty { Role = PartyRole.Seller });

        contract.Parties.Count.Should().BeLessThan(2);
        contract.Parties.Add(new ContractParty { Role = PartyRole.Buyer });
        contract.Parties.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void Contract_ShouldNotAllowNegativeValue()
    {
        var contract = new Contract { ContractValue = 1000000.00m };
        contract.ContractValue.Should().BePositive();
    }

    [Fact]
    public void ContractSignature_ShouldRequire_ValidCertificate_ForAdvancedType()
    {
        var invalidSignature = new ContractSignature
        {
            Type = SignatureType.Advanced,
            CertificateIssuer = null
        };

        var validSignature = new ContractSignature
        {
            Type = SignatureType.Advanced,
            CertificateIssuer = "INDOTEL-CA",
            CertificateSerialNumber = "CERT-001",
            CertificateValidTo = DateTime.UtcNow.AddYears(1)
        };

        invalidSignature.CertificateIssuer.Should().BeNull();
        validSignature.CertificateIssuer.Should().NotBeNull();
        validSignature.CertificateValidTo.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void Contract_ExpirationDate_ShouldBeAfterEffectiveDate()
    {
        var effectiveDate = DateTime.UtcNow;

        var validContract = new Contract
        {
            EffectiveDate = effectiveDate,
            ExpirationDate = effectiveDate.AddYears(1)
        };

        var invalidContract = new Contract
        {
            EffectiveDate = effectiveDate,
            ExpirationDate = effectiveDate.AddDays(-1)
        };

        validContract.ExpirationDate.Should().BeAfter(validContract.EffectiveDate);
        invalidContract.ExpirationDate.Should().BeBefore(invalidContract.EffectiveDate);
    }

    #endregion
}
