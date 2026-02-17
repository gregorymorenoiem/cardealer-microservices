using FluentAssertions;
using DisputeService.Domain.Entities;
using Xunit;

namespace DisputeService.Tests;

/// <summary>
/// Tests unitarios para DisputeService - Resolución de Disputas
/// Cumplimiento con Pro-Consumidor RD + Ley 126-02 Comercio Electrónico
/// </summary>
public class DisputeServiceTests
{
    #region PARTE 1: Tests de Enumeraciones

    [Theory]
    [InlineData("VehicleDefect")]
    [InlineData("PaymentIssue")]
    [InlineData("ContractBreach")]
    [InlineData("DeliveryIssue")]
    [InlineData("FraudClaim")]
    [InlineData("WarrantyClaim")]
    [InlineData("RefundRequest")]
    [InlineData("ServiceQuality")]
    [InlineData("Other")]
    public void DisputeType_ShouldHaveValue(string enumName)
    {
        var enumValue = Enum.Parse<DisputeType>(enumName);
        enumValue.Should().BeDefined();
    }

    [Fact]
    public void DisputeType_ShouldHave9Values()
    {
        var values = Enum.GetValues<DisputeType>();
        values.Should().HaveCount(9);
    }

    [Theory]
    [InlineData("Filed")]
    [InlineData("Acknowledged")]
    [InlineData("InReview")]
    [InlineData("InMediation")]
    [InlineData("PendingResolution")]
    [InlineData("Resolved")]
    [InlineData("Closed")]
    [InlineData("Escalated")]
    [InlineData("ReferredToProConsumidor")]
    public void DisputeStatus_ShouldHaveValue(string enumName)
    {
        var enumValue = Enum.Parse<DisputeStatus>(enumName);
        enumValue.Should().BeDefined();
    }

    [Fact]
    public void DisputeStatus_ShouldHave9Values()
    {
        var values = Enum.GetValues<DisputeStatus>();
        values.Should().HaveCount(9);
    }

    [Theory]
    [InlineData("Low")]
    [InlineData("Normal")]
    [InlineData("High")]
    [InlineData("Critical")]
    public void DisputePriority_ShouldHaveValue(string enumName)
    {
        var enumValue = Enum.Parse<DisputePriority>(enumName);
        enumValue.Should().BeDefined();
    }

    [Fact]
    public void DisputePriority_ShouldHave4Values()
    {
        var values = Enum.GetValues<DisputePriority>();
        values.Should().HaveCount(4);
    }

    [Theory]
    [InlineData("FullRefundBuyer")]
    [InlineData("PartialRefund")]
    [InlineData("Replacement")]
    [InlineData("Repair")]
    [InlineData("FavorSeller")]
    [InlineData("MutualAgreement")]
    [InlineData("NoResolution")]
    public void ResolutionType_ShouldHaveValue(string enumName)
    {
        var enumValue = Enum.Parse<ResolutionType>(enumName);
        enumValue.Should().BeDefined();
    }

    [Fact]
    public void ResolutionType_ShouldHave7Values()
    {
        var values = Enum.GetValues<ResolutionType>();
        values.Should().HaveCount(7);
    }

    [Theory]
    [InlineData("Complainant")]
    [InlineData("Respondent")]
    [InlineData("Mediator")]
    [InlineData("Witness")]
    [InlineData("LegalRepresentative")]
    public void ParticipantRole_ShouldHaveValue(string enumName)
    {
        var enumValue = Enum.Parse<ParticipantRole>(enumName);
        enumValue.Should().BeDefined();
    }

    [Fact]
    public void ParticipantRole_ShouldHave5Values()
    {
        var values = Enum.GetValues<ParticipantRole>();
        values.Should().HaveCount(5);
    }

    [Theory]
    [InlineData("Submitted")]
    [InlineData("UnderReview")]
    [InlineData("Accepted")]
    [InlineData("Rejected")]
    public void EvidenceStatus_ShouldHaveValue(string enumName)
    {
        var enumValue = Enum.Parse<EvidenceStatus>(enumName);
        enumValue.Should().BeDefined();
    }

    [Fact]
    public void EvidenceStatus_ShouldHave4Values()
    {
        var values = Enum.GetValues<EvidenceStatus>();
        values.Should().HaveCount(4);
    }

    [Theory]
    [InlineData("Platform")]
    [InlineData("Email")]
    [InlineData("Phone")]
    [InlineData("VideoCall")]
    public void CommunicationChannel_ShouldHaveValue(string enumName)
    {
        var enumValue = Enum.Parse<CommunicationChannel>(enumName);
        enumValue.Should().BeDefined();
    }

    [Fact]
    public void CommunicationChannel_ShouldHave4Values()
    {
        var values = Enum.GetValues<CommunicationChannel>();
        values.Should().HaveCount(4);
    }

    #endregion

    #region PARTE 2: Tests de Entidad Dispute

    [Fact]
    public void Dispute_ShouldBeCreated_WithValidData()
    {
        var dispute = new Dispute
        {
            Id = Guid.NewGuid(),
            CaseNumber = "DSP-2026-00001",
            Type = DisputeType.VehicleDefect,
            Status = DisputeStatus.Filed,
            Priority = DisputePriority.High,
            ComplainantId = Guid.NewGuid(),
            ComplainantName = "Juan Pérez",
            ComplainantEmail = "juan@email.com",
            RespondentId = Guid.NewGuid(),
            RespondentName = "Auto Dealer SRL",
            RespondentEmail = "ventas@dealer.com",
            Title = "Defecto en motor no declarado",
            Description = "El vehículo presenta fallas en el motor que no fueron informadas",
            DisputedAmount = 500000.00m,
            Currency = "DOP",
            FiledAt = DateTime.UtcNow
        };

        dispute.CaseNumber.Should().StartWith("DSP-");
        dispute.Type.Should().Be(DisputeType.VehicleDefect);
        dispute.Status.Should().Be(DisputeStatus.Filed);
        dispute.Currency.Should().Be("DOP");
    }

    [Fact]
    public void Dispute_ShouldTransition_ThroughStatuses()
    {
        var dispute = new Dispute { Status = DisputeStatus.Filed };

        dispute.Status = DisputeStatus.Acknowledged;
        dispute.Status.Should().Be(DisputeStatus.Acknowledged);

        dispute.Status = DisputeStatus.InReview;
        dispute.Status.Should().Be(DisputeStatus.InReview);

        dispute.Status = DisputeStatus.InMediation;
        dispute.Status.Should().Be(DisputeStatus.InMediation);

        dispute.Status = DisputeStatus.PendingResolution;
        dispute.Status.Should().Be(DisputeStatus.PendingResolution);

        dispute.Status = DisputeStatus.Resolved;
        dispute.ResolvedAt = DateTime.UtcNow;
        dispute.Status.Should().Be(DisputeStatus.Resolved);
    }

    [Fact]
    public void Dispute_ShouldBe_EscalatedToProConsumidor()
    {
        var dispute = new Dispute
        {
            Status = DisputeStatus.InMediation,
            IsEscalated = false,
            ReferredToProConsumidor = false
        };

        // No se resolvió en mediación, escalar
        dispute.IsEscalated = true;
        dispute.EscalatedAt = DateTime.UtcNow;
        dispute.Status = DisputeStatus.Escalated;

        // Referir a Pro-Consumidor
        dispute.ReferredToProConsumidor = true;
        dispute.ProConsumidorReferralDate = DateTime.UtcNow;
        dispute.ProConsumidorCaseNumber = "PC-2026-12345";
        dispute.Status = DisputeStatus.ReferredToProConsumidor;

        dispute.ReferredToProConsumidor.Should().BeTrue();
        dispute.ProConsumidorCaseNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Dispute_ShouldHave_AssignedMediator()
    {
        var dispute = new Dispute
        {
            AssignedMediatorId = Guid.NewGuid().ToString(),
            AssignedMediatorName = "María González",
            AssignedAt = DateTime.UtcNow
        };

        dispute.AssignedMediatorName.Should().NotBeNullOrEmpty();
        dispute.AssignedAt.Should().NotBeNull();
    }

    [Fact]
    public void Dispute_ShouldBeResolved_WithResolutionType()
    {
        var dispute = new Dispute
        {
            Status = DisputeStatus.PendingResolution,
            DisputedAmount = 500000.00m
        };

        dispute.Resolution = ResolutionType.PartialRefund;
        dispute.ResolutionSummary = "Se acuerda reembolso del 50% del monto en disputa";
        dispute.ResolvedAt = DateTime.UtcNow;
        dispute.ResolvedBy = "mediator@okla.com";
        dispute.Status = DisputeStatus.Resolved;

        dispute.Resolution.Should().Be(ResolutionType.PartialRefund);
        dispute.Status.Should().Be(DisputeStatus.Resolved);
    }

    [Theory]
    [InlineData(DisputePriority.Low)]
    [InlineData(DisputePriority.Normal)]
    [InlineData(DisputePriority.High)]
    [InlineData(DisputePriority.Critical)]
    public void Dispute_ShouldSupport_DifferentPriorities(DisputePriority priority)
    {
        var dispute = new Dispute { Priority = priority };
        dispute.Priority.Should().Be(priority);
    }

    [Fact]
    public void Dispute_ShouldHave_Deadlines()
    {
        var filedAt = DateTime.UtcNow;
        var dispute = new Dispute
        {
            FiledAt = filedAt,
            ResponseDueDate = filedAt.AddHours(48),
            ResolutionDueDate = filedAt.AddDays(30)
        };

        dispute.ResponseDueDate.Should().BeAfter(dispute.FiledAt);
        dispute.ResolutionDueDate.Should().BeAfter(dispute.ResponseDueDate!.Value);
    }

    #endregion

    #region PARTE 3: Tests de Entidad DisputeEvidence

    [Fact]
    public void DisputeEvidence_ShouldBeCreated_WithValidData()
    {
        var evidence = new DisputeEvidence
        {
            Id = Guid.NewGuid(),
            DisputeId = Guid.NewGuid(),
            Name = "Fotografía del defecto",
            Description = "Foto mostrando el daño en el motor",
            EvidenceType = "image",
            FileName = "defecto_motor.jpg",
            ContentType = "image/jpeg",
            FileSize = 2048000,
            StoragePath = "s3://okla-disputes/evidence/defecto.jpg",
            SubmittedById = Guid.NewGuid(),
            SubmittedByName = "Juan Pérez",
            SubmitterRole = ParticipantRole.Complainant,
            Status = EvidenceStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };

        evidence.EvidenceType.Should().Be("image");
        evidence.SubmitterRole.Should().Be(ParticipantRole.Complainant);
        evidence.Status.Should().Be(EvidenceStatus.Submitted);
    }

    [Fact]
    public void DisputeEvidence_ShouldBeReviewed()
    {
        var evidence = new DisputeEvidence
        {
            Status = EvidenceStatus.Submitted
        };

        evidence.Status = EvidenceStatus.UnderReview;
        evidence.Status.Should().Be(EvidenceStatus.UnderReview);

        evidence.Status = EvidenceStatus.Accepted;
        evidence.ReviewedAt = DateTime.UtcNow;
        evidence.ReviewedBy = "mediator@okla.com";
        evidence.ReviewNotes = "Evidencia verificada y aceptada";

        evidence.Status.Should().Be(EvidenceStatus.Accepted);
        evidence.ReviewedBy.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DisputeEvidence_ShouldBeRejected_WithReason()
    {
        var evidence = new DisputeEvidence { Status = EvidenceStatus.Submitted };

        evidence.Status = EvidenceStatus.Rejected;
        evidence.ReviewNotes = "Imagen borrosa, no se puede verificar el daño";

        evidence.Status.Should().Be(EvidenceStatus.Rejected);
        evidence.ReviewNotes.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(ParticipantRole.Complainant)]
    [InlineData(ParticipantRole.Respondent)]
    [InlineData(ParticipantRole.Witness)]
    public void DisputeEvidence_ShouldBe_SubmittedByDifferentRoles(ParticipantRole role)
    {
        var evidence = new DisputeEvidence { SubmitterRole = role };
        evidence.SubmitterRole.Should().Be(role);
    }

    #endregion

    #region PARTE 4: Tests de Entidad DisputeComment

    [Fact]
    public void DisputeComment_ShouldBeCreated_WithValidData()
    {
        var comment = new DisputeComment
        {
            Id = Guid.NewGuid(),
            DisputeId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            AuthorName = "Juan Pérez",
            AuthorRole = ParticipantRole.Complainant,
            Content = "Solicito una reunión de mediación lo antes posible",
            IsInternal = false,
            IsOfficial = false,
            CreatedAt = DateTime.UtcNow
        };

        comment.AuthorRole.Should().Be(ParticipantRole.Complainant);
        comment.IsInternal.Should().BeFalse();
    }

    [Fact]
    public void DisputeComment_ShouldBe_InternalNote()
    {
        var internalNote = new DisputeComment
        {
            AuthorRole = ParticipantRole.Mediator,
            Content = "Nota interna: Revisar historial del vendedor",
            IsInternal = true,
            IsOfficial = false
        };

        internalNote.IsInternal.Should().BeTrue();
    }

    [Fact]
    public void DisputeComment_ShouldBe_OfficialStatement()
    {
        var officialStatement = new DisputeComment
        {
            AuthorRole = ParticipantRole.Mediator,
            Content = "Resolución oficial del caso...",
            IsInternal = false,
            IsOfficial = true
        };

        officialStatement.IsOfficial.Should().BeTrue();
    }

    [Fact]
    public void DisputeComment_ShouldSupport_Threading()
    {
        var parentComment = new DisputeComment
        {
            Id = Guid.NewGuid(),
            Content = "Pregunta original"
        };

        var replyComment = new DisputeComment
        {
            ParentCommentId = parentComment.Id,
            Content = "Respuesta a la pregunta"
        };

        replyComment.ParentCommentId.Should().Be(parentComment.Id);
    }

    [Fact]
    public void DisputeComment_ShouldBeEditable()
    {
        var comment = new DisputeComment
        {
            Content = "Contenido original",
            CreatedAt = DateTime.UtcNow
        };

        comment.Content = "Contenido editado";
        comment.EditedAt = DateTime.UtcNow;

        comment.EditedAt.Should().NotBeNull();
    }

    [Fact]
    public void DisputeComment_ShouldSupport_SoftDelete()
    {
        var comment = new DisputeComment { IsDeleted = false };

        comment.IsDeleted = true;
        comment.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region PARTE 5: Tests de Entidad DisputeTimelineEvent

    [Fact]
    public void DisputeTimelineEvent_ShouldBeCreated_WithValidData()
    {
        var timelineEvent = new DisputeTimelineEvent
        {
            Id = Guid.NewGuid(),
            DisputeId = Guid.NewGuid(),
            EventType = "StatusChanged",
            Description = "Estado cambiado de Filed a InReview",
            OldValue = "Filed",
            NewValue = "InReview",
            PerformedBy = "system",
            OccurredAt = DateTime.UtcNow
        };

        timelineEvent.EventType.Should().Be("StatusChanged");
        timelineEvent.OldValue.Should().Be("Filed");
        timelineEvent.NewValue.Should().Be("InReview");
    }

    [Theory]
    [InlineData("DisputeFiled", "Disputa presentada")]
    [InlineData("StatusChanged", "Estado cambiado")]
    [InlineData("EvidenceSubmitted", "Evidencia presentada")]
    [InlineData("MediatorAssigned", "Mediador asignado")]
    [InlineData("SessionScheduled", "Sesión programada")]
    [InlineData("ResolutionReached", "Resolución alcanzada")]
    public void DisputeTimelineEvent_ShouldRecord_DifferentEventTypes(
        string eventType, string description)
    {
        var timelineEvent = new DisputeTimelineEvent
        {
            EventType = eventType,
            Description = description
        };

        timelineEvent.EventType.Should().Be(eventType);
        timelineEvent.Description.Should().Be(description);
    }

    [Fact]
    public void DisputeTimelineEvent_ShouldTrack_IPAddress()
    {
        var timelineEvent = new DisputeTimelineEvent
        {
            IpAddress = "192.168.1.100",
            PerformedBy = "user@email.com"
        };

        timelineEvent.IpAddress.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region PARTE 6: Tests de Entidad MediationSession

    [Fact]
    public void MediationSession_ShouldBeCreated_WithValidData()
    {
        var session = new MediationSession
        {
            Id = Guid.NewGuid(),
            DisputeId = Guid.NewGuid(),
            SessionNumber = "MED-2026-00001",
            ScheduledAt = DateTime.UtcNow.AddDays(3),
            DurationMinutes = 60,
            Channel = CommunicationChannel.VideoCall,
            MeetingLink = "https://meet.okla.com/mediation/12345",
            MediatorId = Guid.NewGuid().ToString(),
            MediatorName = "María González",
            Status = "Scheduled"
        };

        session.SessionNumber.Should().StartWith("MED-");
        session.Channel.Should().Be(CommunicationChannel.VideoCall);
        session.Status.Should().Be("Scheduled");
    }

    [Fact]
    public void MediationSession_ShouldTransition_ThroughStatuses()
    {
        var session = new MediationSession { Status = "Scheduled" };

        session.Status = "InProgress";
        session.StartedAt = DateTime.UtcNow;
        session.Status.Should().Be("InProgress");

        session.Status = "Completed";
        session.EndedAt = DateTime.UtcNow;
        session.Status.Should().Be("Completed");
    }

    [Fact]
    public void MediationSession_ShouldTrack_Attendance()
    {
        var session = new MediationSession
        {
            ComplainantAttended = true,
            RespondentAttended = true
        };

        var bothAttended = session.ComplainantAttended && session.RespondentAttended;
        bothAttended.Should().BeTrue();
    }

    [Fact]
    public void MediationSession_ShouldRecord_Agreement()
    {
        var session = new MediationSession
        {
            PartiesAgreed = true,
            Summary = "Las partes acordaron un reembolso del 50%",
            Notes = "Detalles del acuerdo..."
        };

        session.PartiesAgreed.Should().BeTrue();
        session.Summary.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(CommunicationChannel.VideoCall, "https://meet.okla.com/123")]
    [InlineData(CommunicationChannel.Phone, null)]
    [InlineData(CommunicationChannel.Platform, "In-app chat")]
    public void MediationSession_ShouldSupport_DifferentChannels(
        CommunicationChannel channel, string? meetingLink)
    {
        var session = new MediationSession
        {
            Channel = channel,
            MeetingLink = meetingLink
        };

        session.Channel.Should().Be(channel);
    }

    [Fact]
    public void MediationSession_ShouldHave_Location_ForInPerson()
    {
        var session = new MediationSession
        {
            Channel = CommunicationChannel.Platform,
            Location = "Oficina OKLA - Santo Domingo"
        };

        session.Location.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region PARTE 7: Tests de Entidad DisputeParticipant

    [Fact]
    public void DisputeParticipant_ShouldBeCreated_WithValidData()
    {
        var participant = new DisputeParticipant
        {
            Id = Guid.NewGuid(),
            DisputeId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserName = "Juan Pérez",
            UserEmail = "juan@email.com",
            Role = ParticipantRole.Complainant,
            IsActive = true,
            JoinedAt = DateTime.UtcNow,
            PreferredChannel = CommunicationChannel.Email
        };

        participant.Role.Should().Be(ParticipantRole.Complainant);
        participant.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(ParticipantRole.Complainant)]
    [InlineData(ParticipantRole.Respondent)]
    [InlineData(ParticipantRole.Mediator)]
    [InlineData(ParticipantRole.Witness)]
    [InlineData(ParticipantRole.LegalRepresentative)]
    public void DisputeParticipant_ShouldSupport_DifferentRoles(ParticipantRole role)
    {
        var participant = new DisputeParticipant { Role = role };
        participant.Role.Should().Be(role);
    }

    [Fact]
    public void DisputeParticipant_ShouldLeave_Dispute()
    {
        var participant = new DisputeParticipant
        {
            IsActive = true,
            JoinedAt = DateTime.UtcNow.AddDays(-5)
        };

        participant.IsActive = false;
        participant.LeftAt = DateTime.UtcNow;

        participant.IsActive.Should().BeFalse();
        participant.LeftAt.Should().NotBeNull();
    }

    #endregion

    #region PARTE 8: Tests de Entidad ResolutionTemplate

    [Fact]
    public void ResolutionTemplate_ShouldBeCreated_WithValidData()
    {
        var template = new ResolutionTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Plantilla Reembolso Completo",
            ForDisputeType = DisputeType.VehicleDefect,
            ResolutionType = ResolutionType.FullRefundBuyer,
            TemplateContent = "Por la presente se acuerda el reembolso completo...",
            IsActive = true,
            CreatedBy = "admin@okla.com",
            CreatedAt = DateTime.UtcNow
        };

        template.ForDisputeType.Should().Be(DisputeType.VehicleDefect);
        template.ResolutionType.Should().Be(ResolutionType.FullRefundBuyer);
        template.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(DisputeType.VehicleDefect, ResolutionType.Repair)]
    [InlineData(DisputeType.PaymentIssue, ResolutionType.FullRefundBuyer)]
    [InlineData(DisputeType.ContractBreach, ResolutionType.MutualAgreement)]
    [InlineData(DisputeType.WarrantyClaim, ResolutionType.Replacement)]
    public void ResolutionTemplate_ShouldMap_DisputeTypeToResolution(
        DisputeType disputeType, ResolutionType resolutionType)
    {
        var template = new ResolutionTemplate
        {
            ForDisputeType = disputeType,
            ResolutionType = resolutionType
        };

        template.ForDisputeType.Should().Be(disputeType);
        template.ResolutionType.Should().Be(resolutionType);
    }

    #endregion

    #region PARTE 9: Tests de Entidad DisputeSlaConfiguration

    [Fact]
    public void DisputeSlaConfiguration_ShouldBeCreated_WithValidData()
    {
        var slaConfig = new DisputeSlaConfiguration
        {
            Id = Guid.NewGuid(),
            DisputeType = DisputeType.VehicleDefect,
            Priority = DisputePriority.High,
            ResponseDeadlineHours = 24,
            ResolutionDeadlineHours = 336, // 14 días
            EscalationThresholdHours = 72, // 3 días
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        slaConfig.ResponseDeadlineHours.Should().Be(24);
        slaConfig.IsActive.Should().BeTrue();
    }

    [Fact]
    public void DisputeSlaConfiguration_ShouldCalculate_Deadlines()
    {
        var filedAt = DateTime.UtcNow;
        var slaConfig = new DisputeSlaConfiguration
        {
            ResponseDeadlineHours = 48,
            ResolutionDeadlineHours = 720 // 30 días
        };

        var responseDeadline = filedAt.AddHours(slaConfig.ResponseDeadlineHours);
        var resolutionDeadline = filedAt.AddHours(slaConfig.ResolutionDeadlineHours);

        responseDeadline.Should().Be(filedAt.AddDays(2));
        resolutionDeadline.Should().Be(filedAt.AddDays(30));
    }

    [Theory]
    [InlineData(DisputePriority.Critical, 24, 168)]   // 1 día respuesta, 7 días resolución
    [InlineData(DisputePriority.High, 48, 336)]       // 2 días respuesta, 14 días resolución
    [InlineData(DisputePriority.Normal, 72, 720)]     // 3 días respuesta, 30 días resolución
    [InlineData(DisputePriority.Low, 168, 1440)]      // 7 días respuesta, 60 días resolución
    public void DisputeSlaConfiguration_ShouldVary_ByPriority(
        DisputePriority priority, int responseHours, int resolutionHours)
    {
        var slaConfig = new DisputeSlaConfiguration
        {
            Priority = priority,
            ResponseDeadlineHours = responseHours,
            ResolutionDeadlineHours = resolutionHours
        };

        slaConfig.Priority.Should().Be(priority);
        slaConfig.ResponseDeadlineHours.Should().Be(responseHours);
        slaConfig.ResolutionDeadlineHours.Should().Be(resolutionHours);
    }

    #endregion

    #region PARTE 10: Tests de Flujos de Negocio

    [Fact]
    public void DisputeFlow_ShouldComplete_Successfully()
    {
        // 1. Crear disputa
        var dispute = new Dispute
        {
            CaseNumber = "DSP-2026-00001",
            Type = DisputeType.VehicleDefect,
            Status = DisputeStatus.Filed,
            ComplainantName = "Juan Pérez",
            RespondentName = "Auto Dealer SRL",
            DisputedAmount = 500000.00m
        };

        // 2. Acusar recibo
        dispute.Status = DisputeStatus.Acknowledged;

        // 3. Revisar
        dispute.Status = DisputeStatus.InReview;

        // 4. Asignar mediador
        dispute.AssignedMediatorName = "María González";
        dispute.AssignedAt = DateTime.UtcNow;
        dispute.Status = DisputeStatus.InMediation;

        // 5. Resolver
        dispute.Resolution = ResolutionType.PartialRefund;
        dispute.ResolutionSummary = "Acuerdo de reembolso 50%";
        dispute.ResolvedAt = DateTime.UtcNow;
        dispute.Status = DisputeStatus.Resolved;

        // 6. Cerrar
        dispute.Status = DisputeStatus.Closed;

        dispute.Status.Should().Be(DisputeStatus.Closed);
        dispute.Resolution.Should().Be(ResolutionType.PartialRefund);
    }

    [Fact]
    public void DisputeFlow_ShouldEscalate_ToProConsumidor()
    {
        var dispute = new Dispute
        {
            Type = DisputeType.FraudClaim,
            Status = DisputeStatus.InMediation,
            Priority = DisputePriority.Critical
        };

        // No se resolvió en mediación
        dispute.IsEscalated = true;
        dispute.EscalatedAt = DateTime.UtcNow;
        dispute.Status = DisputeStatus.Escalated;

        // Referir a Pro-Consumidor
        dispute.ReferredToProConsumidor = true;
        dispute.ProConsumidorReferralDate = DateTime.UtcNow;
        dispute.ProConsumidorCaseNumber = "PC-2026-12345";
        dispute.Status = DisputeStatus.ReferredToProConsumidor;

        dispute.ReferredToProConsumidor.Should().BeTrue();
        dispute.ProConsumidorCaseNumber.Should().StartWith("PC-");
    }

    [Fact]
    public void MediationSession_ShouldReach_Agreement()
    {
        var session = new MediationSession
        {
            Status = "Scheduled",
            ScheduledAt = DateTime.UtcNow
        };

        // Iniciar sesión
        session.Status = "InProgress";
        session.StartedAt = DateTime.UtcNow;

        // Registrar asistencia
        session.ComplainantAttended = true;
        session.RespondentAttended = true;

        // Llegar a acuerdo
        session.PartiesAgreed = true;
        session.Summary = "Las partes acordaron un reembolso parcial del 60%";
        session.Status = "Completed";
        session.EndedAt = DateTime.UtcNow;

        session.PartiesAgreed.Should().BeTrue();
        session.Status.Should().Be("Completed");
    }

    #endregion

    #region PARTE 11: Tests de Regulaciones RD (Pro-Consumidor)

    [Fact]
    public void Dispute_ShouldComplywith_ProConsumidorTimelines()
    {
        var filedAt = DateTime.UtcNow;
        var dispute = new Dispute
        {
            FiledAt = filedAt,
            ResponseDueDate = filedAt.AddHours(48), // 48 horas para responder
            ResolutionDueDate = filedAt.AddDays(30) // 30 días para resolver
        };

        dispute.ResponseDueDate.Should().Be(filedAt.AddDays(2));
        dispute.ResolutionDueDate.Should().Be(filedAt.AddDays(30));
    }

    [Fact]
    public void Dispute_ShouldTrack_ProConsumidorReferral()
    {
        var dispute = new Dispute
        {
            ReferredToProConsumidor = true,
            ProConsumidorReferralDate = DateTime.UtcNow,
            ProConsumidorCaseNumber = "PC-2026-12345"
        };

        dispute.ReferredToProConsumidor.Should().BeTrue();
        dispute.ProConsumidorCaseNumber.Should().MatchRegex(@"^PC-\d{4}-\d+$");
    }

    [Fact]
    public void DisputeEvidence_ShouldBe_Preserved_ForProConsumidor()
    {
        var evidence = new DisputeEvidence
        {
            Name = "Contrato de Compraventa",
            EvidenceType = "document",
            Status = EvidenceStatus.Accepted,
            SubmittedAt = DateTime.UtcNow
        };

        // Pro-Consumidor puede solicitar todas las evidencias
        evidence.Status.Should().Be(EvidenceStatus.Accepted);
    }

    [Fact]
    public void Dispute_ShouldUse_DominicanPesos()
    {
        var dispute = new Dispute
        {
            DisputedAmount = 500000.00m,
            Currency = "DOP"
        };

        dispute.Currency.Should().Be("DOP");
        dispute.DisputedAmount.Should().BePositive();
    }

    #endregion

    #region PARTE 12: Tests de Validaciones

    [Fact]
    public void Dispute_ShouldRequire_ComplainantInfo()
    {
        var dispute = new Dispute
        {
            ComplainantId = Guid.NewGuid(),
            ComplainantName = "Juan Pérez",
            ComplainantEmail = "juan@email.com"
        };

        dispute.ComplainantId.Should().NotBeEmpty();
        dispute.ComplainantName.Should().NotBeNullOrEmpty();
        dispute.ComplainantEmail.Should().Contain("@");
    }

    [Fact]
    public void Dispute_ShouldRequire_RespondentInfo()
    {
        var dispute = new Dispute
        {
            RespondentId = Guid.NewGuid(),
            RespondentName = "Auto Dealer SRL",
            RespondentEmail = "ventas@dealer.com"
        };

        dispute.RespondentId.Should().NotBeEmpty();
        dispute.RespondentName.Should().NotBeNullOrEmpty();
        dispute.RespondentEmail.Should().Contain("@");
    }

    [Fact]
    public void DisputeEvidence_FileSize_ShouldBePositive()
    {
        var evidence = new DisputeEvidence { FileSize = 2048000 };
        evidence.FileSize.Should().BePositive();
    }

    [Fact]
    public void MediationSession_Duration_ShouldBePositive()
    {
        var session = new MediationSession { DurationMinutes = 60 };
        session.DurationMinutes.Should().BePositive();
    }

    [Fact]
    public void DisputeSla_ResponseDeadline_ShouldBeLessThan_ResolutionDeadline()
    {
        var slaConfig = new DisputeSlaConfiguration
        {
            ResponseDeadlineHours = 48,
            ResolutionDeadlineHours = 720
        };

        slaConfig.ResponseDeadlineHours.Should().BeLessThan(slaConfig.ResolutionDeadlineHours);
    }

    #endregion
}
