// =====================================================
// C2: DataProtectionService - Tests Unitarios
// Ley 172-13 Protección de Datos Personales (ARCO)
// =====================================================

using FluentAssertions;
using DataProtectionService.Domain.Entities;
using Xunit;

namespace DataProtectionService.Tests;

public class DataProtectionServiceTests
{
    // =====================================================
    // PARTE 1: Tests de Enumeraciones
    // =====================================================

    [Theory]
    [InlineData(ConsentType.TermsOfService, 1)]
    [InlineData(ConsentType.PrivacyPolicy, 2)]
    [InlineData(ConsentType.MarketingCommunications, 3)]
    [InlineData(ConsentType.DataProcessing, 4)]
    [InlineData(ConsentType.ThirdPartySharing, 5)]
    [InlineData(ConsentType.Cookies, 6)]
    [InlineData(ConsentType.LocationTracking, 7)]
    [InlineData(ConsentType.PersonalizedAds, 8)]
    public void ConsentType_ShouldHaveExpectedValues(ConsentType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(ARCOType.Access, 1)]
    [InlineData(ARCOType.Rectification, 2)]
    [InlineData(ARCOType.Cancellation, 3)]
    [InlineData(ARCOType.Opposition, 4)]
    public void ARCOType_ShouldHaveExpectedValues(ARCOType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(ARCOStatus.Received, 1)]
    [InlineData(ARCOStatus.IdentityVerification, 2)]
    [InlineData(ARCOStatus.InProgress, 3)]
    [InlineData(ARCOStatus.PendingInformation, 4)]
    [InlineData(ARCOStatus.Completed, 5)]
    [InlineData(ARCOStatus.Rejected, 6)]
    [InlineData(ARCOStatus.Expired, 7)]
    public void ARCOStatus_ShouldHaveExpectedValues(ARCOStatus status, int expectedValue)
    {
        ((int)status).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(ExportStatus.Pending, 1)]
    [InlineData(ExportStatus.Processing, 2)]
    [InlineData(ExportStatus.Completed, 3)]
    [InlineData(ExportStatus.Failed, 4)]
    [InlineData(ExportStatus.Expired, 5)]
    public void ExportStatus_ShouldHaveExpectedValues(ExportStatus status, int expectedValue)
    {
        ((int)status).Should().Be(expectedValue);
    }

    // =====================================================
    // PARTE 2: Tests de UserConsent
    // =====================================================

    [Fact]
    public void UserConsent_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var consent = new UserConsent
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = ConsentType.PrivacyPolicy,
            Version = "2.0.1",
            DocumentHash = "sha256:abc123def456",
            Granted = true,
            GrantedAt = DateTime.UtcNow,
            IpAddress = "192.168.1.100",
            UserAgent = "Mozilla/5.0",
            CollectionMethod = "web"
        };

        // Assert
        consent.Should().NotBeNull();
        consent.Type.Should().Be(ConsentType.PrivacyPolicy);
        consent.Version.Should().Be("2.0.1");
        consent.Granted.Should().BeTrue();
        consent.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UserConsent_IsActive_ShouldBeFalse_WhenRevoked()
    {
        // Arrange & Act
        var consent = new UserConsent
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = ConsentType.MarketingCommunications,
            Granted = true,
            GrantedAt = DateTime.UtcNow.AddDays(-30),
            RevokedAt = DateTime.UtcNow,
            RevokeReason = "User requested removal"
        };

        // Assert
        consent.IsActive.Should().BeFalse();
        consent.RevokeReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void UserConsent_IsActive_ShouldBeFalse_WhenNotGranted()
    {
        // Arrange & Act
        var consent = new UserConsent
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = ConsentType.ThirdPartySharing,
            Granted = false
        };

        // Assert
        consent.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData(ConsentType.TermsOfService)]
    [InlineData(ConsentType.PrivacyPolicy)]
    [InlineData(ConsentType.DataProcessing)]
    public void UserConsent_ShouldSupport_RequiredConsentTypes(ConsentType type)
    {
        // Arrange & Act
        var consent = new UserConsent
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = type,
            Granted = true,
            GrantedAt = DateTime.UtcNow
        };

        // Assert
        consent.Type.Should().Be(type);
        consent.IsActive.Should().BeTrue();
    }

    // =====================================================
    // PARTE 3: Tests de ARCORequest
    // =====================================================

    [Fact]
    public void ARCORequest_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var deadline = DateTime.UtcNow.AddDays(30);
        var request = new ARCORequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RequestNumber = "ARCO-2026-00001",
            Type = ARCOType.Access,
            Status = ARCOStatus.Received,
            Description = "Solicito acceso a mis datos personales",
            RequestedAt = DateTime.UtcNow,
            Deadline = deadline,
            IpAddress = "192.168.1.100",
            UserAgent = "Mozilla/5.0"
        };

        // Assert
        request.Should().NotBeNull();
        request.RequestNumber.Should().Be("ARCO-2026-00001");
        request.Type.Should().Be(ARCOType.Access);
        request.Status.Should().Be(ARCOStatus.Received);
    }

    [Fact]
    public void ARCORequest_IsOverdue_ShouldBeTrue_WhenPastDeadline()
    {
        // Arrange & Act
        var request = new ARCORequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RequestNumber = "ARCO-2026-00002",
            Type = ARCOType.Rectification,
            Status = ARCOStatus.InProgress,
            Deadline = DateTime.UtcNow.AddDays(-5) // Past deadline
        };

        // Assert
        request.IsOverdue.Should().BeTrue();
    }

    [Fact]
    public void ARCORequest_IsOverdue_ShouldBeFalse_WhenCompleted()
    {
        // Arrange & Act
        var request = new ARCORequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RequestNumber = "ARCO-2026-00003",
            Type = ARCOType.Cancellation,
            Status = ARCOStatus.Completed,
            Deadline = DateTime.UtcNow.AddDays(-5),
            CompletedAt = DateTime.UtcNow.AddDays(-10)
        };

        // Assert
        request.IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void ARCORequest_DaysRemaining_ShouldCalculateCorrectly()
    {
        // Arrange
        var daysToDeadline = 15;
        var request = new ARCORequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RequestNumber = "ARCO-2026-00004",
            Type = ARCOType.Access,
            Status = ARCOStatus.InProgress,
            Deadline = DateTime.UtcNow.AddDays(daysToDeadline)
        };

        // Act & Assert
        request.DaysRemaining.Should().BeGreaterOrEqualTo(daysToDeadline - 1);
        request.DaysRemaining.Should().BeLessThanOrEqualTo(daysToDeadline + 1);
    }

    [Theory]
    [InlineData(ARCOType.Access, "Solicito ver mis datos")]
    [InlineData(ARCOType.Rectification, "Solicito corregir mi dirección")]
    [InlineData(ARCOType.Cancellation, "Solicito eliminar mis datos")]
    [InlineData(ARCOType.Opposition, "Me opongo al tratamiento")]
    public void ARCORequest_ShouldSupport_AllARCORights(ARCOType type, string description)
    {
        // Arrange & Act
        var request = new ARCORequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RequestNumber = $"ARCO-2026-{Guid.NewGuid():N}".Substring(0, 20),
            Type = type,
            Description = description,
            Deadline = DateTime.UtcNow.AddDays(30)
        };

        // Assert
        request.Type.Should().Be(type);
        request.Description.Should().Be(description);
    }

    [Fact]
    public void ARCORequest_ForRectification_ShouldHaveProposedChanges()
    {
        // Arrange & Act
        var request = new ARCORequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RequestNumber = "ARCO-2026-00005",
            Type = ARCOType.Rectification,
            Description = "Corregir dirección",
            SpecificDataRequested = "Address",
            ProposedChanges = "Nueva dirección: Calle Principal #123"
        };

        // Assert
        request.ProposedChanges.Should().NotBeNullOrEmpty();
        request.SpecificDataRequested.Should().Be("Address");
    }

    [Fact]
    public void ARCORequest_ForOpposition_ShouldHaveOppositionReason()
    {
        // Arrange & Act
        var request = new ARCORequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RequestNumber = "ARCO-2026-00006",
            Type = ARCOType.Opposition,
            Description = "Oposición a marketing",
            OppositionReason = "No deseo recibir comunicaciones comerciales"
        };

        // Assert
        request.OppositionReason.Should().NotBeNullOrEmpty();
    }

    // =====================================================
    // PARTE 4: Tests de ARCOStatusHistory
    // =====================================================

    [Fact]
    public void ARCOStatusHistory_ShouldTrackStatusChanges()
    {
        // Arrange & Act
        var history = new ARCOStatusHistory
        {
            Id = Guid.NewGuid(),
            ARCORequestId = Guid.NewGuid(),
            OldStatus = ARCOStatus.Received,
            NewStatus = ARCOStatus.InProgress,
            Comment = "Iniciando procesamiento de solicitud",
            ChangedBy = Guid.NewGuid(),
            ChangedByName = "Admin User"
        };

        // Assert
        history.OldStatus.Should().Be(ARCOStatus.Received);
        history.NewStatus.Should().Be(ARCOStatus.InProgress);
        history.Comment.Should().NotBeNullOrEmpty();
    }

    // =====================================================
    // PARTE 5: Tests de ARCOAttachment
    // =====================================================

    [Fact]
    public void ARCOAttachment_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var attachment = new ARCOAttachment
        {
            Id = Guid.NewGuid(),
            ARCORequestId = Guid.NewGuid(),
            FileName = "documento_identidad.pdf",
            FileUrl = "https://storage.okla.com/attachments/doc123.pdf",
            FileType = "application/pdf",
            FileSize = 1024 * 500, // 500KB
            Description = "Copia de cédula",
            UploadedBy = Guid.NewGuid()
        };

        // Assert
        attachment.FileName.Should().Be("documento_identidad.pdf");
        attachment.FileType.Should().Be("application/pdf");
        attachment.FileSize.Should().Be(512000);
    }

    // =====================================================
    // PARTE 6: Tests de DataExport
    // =====================================================

    [Fact]
    public void DataExport_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var export = new DataExport
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ARCORequestId = Guid.NewGuid(),
            Status = ExportStatus.Pending,
            Format = "JSON",
            IncludeTransactions = true,
            IncludeMessages = true,
            IncludeVehicleHistory = true,
            IncludeUserActivity = true,
            IpAddress = "192.168.1.100"
        };

        // Assert
        export.Status.Should().Be(ExportStatus.Pending);
        export.Format.Should().Be("JSON");
        export.IncludeTransactions.Should().BeTrue();
    }

    [Theory]
    [InlineData("JSON")]
    [InlineData("PDF")]
    [InlineData("CSV")]
    public void DataExport_ShouldSupport_MultipleFormats(string format)
    {
        // Arrange & Act
        var export = new DataExport
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Format = format
        };

        // Assert
        export.Format.Should().Be(format);
    }

    [Fact]
    public void DataExport_ShouldSetCompletionDetails_WhenCompleted()
    {
        // Arrange & Act
        var export = new DataExport
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = ExportStatus.Completed,
            CompletedAt = DateTime.UtcNow,
            DownloadUrl = "https://storage.okla.com/exports/export123.json",
            DownloadExpiresAt = DateTime.UtcNow.AddHours(24),
            FileSizeBytes = 1024 * 1024 * 5 // 5MB
        };

        // Assert
        export.Status.Should().Be(ExportStatus.Completed);
        export.DownloadUrl.Should().NotBeNullOrEmpty();
        export.FileSizeBytes.Should().Be(5242880);
    }

    // =====================================================
    // PARTE 7: Tests de PrivacyPolicy
    // =====================================================

    [Fact]
    public void PrivacyPolicy_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var policy = new PrivacyPolicy
        {
            Id = Guid.NewGuid(),
            Version = "3.0.0",
            DocumentType = "PrivacyPolicy",
            Content = "Política de Privacidad de OKLA...",
            ChangesSummary = "Actualización de términos de cookies",
            Language = "es",
            EffectiveDate = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            RequiresReAcceptance = true,
            CreatedBy = Guid.NewGuid(),
            CreatedByName = "Admin Legal"
        };

        // Assert
        policy.Version.Should().Be("3.0.0");
        policy.DocumentType.Should().Be("PrivacyPolicy");
        policy.Language.Should().Be("es");
        policy.RequiresReAcceptance.Should().BeTrue();
    }

    [Theory]
    [InlineData("PrivacyPolicy")]
    [InlineData("TermsOfService")]
    [InlineData("CookiePolicy")]
    public void PrivacyPolicy_ShouldSupport_AllDocumentTypes(string docType)
    {
        // Arrange & Act
        var policy = new PrivacyPolicy
        {
            Id = Guid.NewGuid(),
            Version = "1.0.0",
            DocumentType = docType,
            Content = $"Contenido del documento {docType}"
        };

        // Assert
        policy.DocumentType.Should().Be(docType);
    }

    // =====================================================
    // PARTE 8: Tests de AnonymizationRecord
    // =====================================================

    [Fact]
    public void AnonymizationRecord_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var record = new AnonymizationRecord
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ARCORequestId = Guid.NewGuid(),
            RequestedBy = Guid.NewGuid(),
            Reason = "User requested account deletion via ARCO",
            OriginalEmail = "hash:abc123",
            OriginalPhone = "hash:def456",
            AnonymizedEmail = "deleted_user_123@anonymized.okla.com",
            AnonymizedPhone = "000-000-0000",
            AffectedTables = new List<string> { "Users", "Vehicles", "Messages" },
            AffectedRecordsCount = 150,
            IsComplete = true
        };

        // Assert
        record.Reason.Should().NotBeNullOrEmpty();
        record.AffectedTables.Should().HaveCount(3);
        record.AffectedRecordsCount.Should().Be(150);
        record.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void AnonymizationRecord_RetentionEndDate_ShouldBeSetForCompliance()
    {
        // Arrange & Act
        var record = new AnonymizationRecord
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Reason = "ARCO Cancellation request"
        };

        // Assert - Retention should be 5 years from now
        record.RetentionEndDate.Should().BeCloseTo(DateTime.UtcNow.AddYears(5), TimeSpan.FromMinutes(5));
    }

    // =====================================================
    // PARTE 9: Tests de DataChangeLog
    // =====================================================

    [Fact]
    public void DataChangeLog_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var log = new DataChangeLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            DataField = "Email",
            DataCategory = "Contact",
            OldValueHash = "sha256:oldvalue123",
            NewValueHash = "sha256:newvalue456",
            OldValueMasked = "j***@old.com",
            NewValueMasked = "j***@new.com",
            ChangedByType = "User",
            ChangedById = Guid.NewGuid(),
            ChangedByName = "Juan Pérez",
            Reason = "User updated email",
            SourceService = "UserService",
            IpAddress = "192.168.1.100",
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Assert
        log.DataField.Should().Be("Email");
        log.DataCategory.Should().Be("Contact");
        log.ChangedByType.Should().Be("User");
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData("System")]
    public void DataChangeLog_ShouldSupport_AllChangedByTypes(string changedByType)
    {
        // Arrange & Act
        var log = new DataChangeLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            DataField = "Phone",
            DataCategory = "Contact",
            ChangedByType = changedByType
        };

        // Assert
        log.ChangedByType.Should().Be(changedByType);
    }

    [Theory]
    [InlineData("Personal")]
    [InlineData("Contact")]
    [InlineData("Financial")]
    [InlineData("Location")]
    public void DataChangeLog_ShouldSupport_AllDataCategories(string category)
    {
        // Arrange & Act
        var log = new DataChangeLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            DataField = "TestField",
            DataCategory = category
        };

        // Assert
        log.DataCategory.Should().Be(category);
    }

    // =====================================================
    // PARTE 10: Tests de Integración de Entidades
    // =====================================================

    [Fact]
    public void ARCORequest_ShouldHaveAttachmentsCollection()
    {
        // Arrange
        var request = new ARCORequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RequestNumber = "ARCO-2026-00100",
            Type = ARCOType.Access
        };

        var attachment = new ARCOAttachment
        {
            Id = Guid.NewGuid(),
            ARCORequestId = request.Id,
            FileName = "cedula.pdf",
            FileUrl = "https://storage.okla.com/cedula.pdf",
            FileType = "application/pdf"
        };

        // Act
        request.Attachments.Add(attachment);

        // Assert
        request.Attachments.Should().HaveCount(1);
        request.Attachments[0].FileName.Should().Be("cedula.pdf");
    }

    [Fact]
    public void ARCORequest_ShouldHaveStatusHistoryCollection()
    {
        // Arrange
        var request = new ARCORequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RequestNumber = "ARCO-2026-00101",
            Type = ARCOType.Rectification
        };

        var history1 = new ARCOStatusHistory
        {
            Id = Guid.NewGuid(),
            ARCORequestId = request.Id,
            OldStatus = ARCOStatus.Received,
            NewStatus = ARCOStatus.IdentityVerification
        };

        var history2 = new ARCOStatusHistory
        {
            Id = Guid.NewGuid(),
            ARCORequestId = request.Id,
            OldStatus = ARCOStatus.IdentityVerification,
            NewStatus = ARCOStatus.InProgress
        };

        // Act
        request.StatusHistory.Add(history1);
        request.StatusHistory.Add(history2);

        // Assert
        request.StatusHistory.Should().HaveCount(2);
    }

    [Fact]
    public void DataExport_CanBeLinked_ToARCORequest()
    {
        // Arrange
        var arcoRequestId = Guid.NewGuid();

        var export = new DataExport
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ARCORequestId = arcoRequestId,
            Status = ExportStatus.Completed
        };

        // Assert
        export.ARCORequestId.Should().Be(arcoRequestId);
    }

    [Fact]
    public void AnonymizationRecord_CanBeLinked_ToARCORequest()
    {
        // Arrange
        var arcoRequestId = Guid.NewGuid();

        var record = new AnonymizationRecord
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ARCORequestId = arcoRequestId,
            Reason = "ARCO Cancellation"
        };

        // Assert
        record.ARCORequestId.Should().Be(arcoRequestId);
    }

    // =====================================================
    // PARTE 11: Tests de Compliance Ley 172-13
    // =====================================================

    [Fact]
    public void ARCORequest_Deadline_ShouldBe30Days_AsPerLaw17213()
    {
        // Según Ley 172-13, las solicitudes ARCO deben responderse en 30 días
        var requestDate = DateTime.UtcNow;
        var expectedDeadline = requestDate.AddDays(30);

        var request = new ARCORequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RequestNumber = "ARCO-2026-00200",
            Type = ARCOType.Access,
            RequestedAt = requestDate,
            Deadline = expectedDeadline
        };

        // Assert
        (request.Deadline - request.RequestedAt).Days.Should().Be(30);
    }

    [Fact]
    public void UserConsent_ShouldTrack_AllRequiredMetadata()
    {
        // Ley 172-13 requiere registro completo del consentimiento
        var consent = new UserConsent
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = ConsentType.DataProcessing,
            Version = "1.0.0",
            DocumentHash = "sha256:hash_del_documento",
            Granted = true,
            GrantedAt = DateTime.UtcNow,
            IpAddress = "192.168.1.100",
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
            CollectionMethod = "web"
        };

        // Assert - Todos los campos requeridos por la ley
        consent.Version.Should().NotBeNullOrEmpty();
        consent.DocumentHash.Should().NotBeNullOrEmpty();
        consent.IpAddress.Should().NotBeNullOrEmpty();
        consent.UserAgent.Should().NotBeNullOrEmpty();
        consent.CollectionMethod.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AnonymizationRecord_ShouldRetainFor5Years_AsPerLaw()
    {
        // La ley requiere retención de registros de anonimización
        var record = new AnonymizationRecord();
        
        var expectedRetention = DateTime.UtcNow.AddYears(5);

        // Assert
        record.RetentionEndDate.Should().BeCloseTo(expectedRetention, TimeSpan.FromMinutes(5));
    }
}
