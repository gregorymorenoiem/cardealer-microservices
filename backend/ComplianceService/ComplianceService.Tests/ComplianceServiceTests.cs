// =====================================================
// C5: ComplianceService - Tests Unitarios
// Cumplimiento General: Ley 155-17 (PLD), Ley 172-13, Ley 358-05
// =====================================================

using FluentAssertions;
using ComplianceService.Domain.Entities;
using Xunit;

namespace ComplianceService.Tests;

public class ComplianceServiceTests
{
    // =====================================================
    // PARTE 1: Tests de Enumeraciones
    // =====================================================

    [Theory]
    [InlineData(ComplianceStatus.NotEvaluated, 0)]
    [InlineData(ComplianceStatus.Pending, 1)]
    [InlineData(ComplianceStatus.InProgress, 2)]
    [InlineData(ComplianceStatus.Compliant, 3)]
    [InlineData(ComplianceStatus.NonCompliant, 4)]
    [InlineData(ComplianceStatus.PartiallyCompliant, 5)]
    [InlineData(ComplianceStatus.UnderRemediation, 6)]
    [InlineData(ComplianceStatus.Exempted, 7)]
    public void ComplianceStatus_ShouldHaveExpectedValues(ComplianceStatus status, int expectedValue)
    {
        ((int)status).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(RegulationType.PLD_AML, 1)]
    [InlineData(RegulationType.DataProtection, 2)]
    [InlineData(RegulationType.ConsumerProtection, 3)]
    [InlineData(RegulationType.ElectronicCommerce, 4)]
    [InlineData(RegulationType.FinancialRegulation, 5)]
    [InlineData(RegulationType.TaxCompliance, 6)]
    [InlineData(RegulationType.VehicleRegistration, 7)]
    [InlineData(RegulationType.Environmental, 8)]
    [InlineData(RegulationType.Other, 99)]
    public void RegulationType_ShouldHaveExpectedValues(RegulationType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(CriticalityLevel.Low, 1)]
    [InlineData(CriticalityLevel.Medium, 2)]
    [InlineData(CriticalityLevel.High, 3)]
    [InlineData(CriticalityLevel.Critical, 4)]
    public void CriticalityLevel_ShouldHaveExpectedValues(CriticalityLevel level, int expectedValue)
    {
        ((int)level).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(ComplianceService.Domain.Entities.TaskStatus.Pending, 1)]
    [InlineData(ComplianceService.Domain.Entities.TaskStatus.InProgress, 2)]
    [InlineData(ComplianceService.Domain.Entities.TaskStatus.Completed, 3)]
    [InlineData(ComplianceService.Domain.Entities.TaskStatus.Overdue, 4)]
    [InlineData(ComplianceService.Domain.Entities.TaskStatus.Cancelled, 5)]
    [InlineData(ComplianceService.Domain.Entities.TaskStatus.OnHold, 6)]
    public void TaskStatus_ShouldHaveExpectedValues(ComplianceService.Domain.Entities.TaskStatus status, int expectedValue)
    {
        ((int)status).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(FindingType.Observation, 1)]
    [InlineData(FindingType.MinorNonConformity, 2)]
    [InlineData(FindingType.MajorNonConformity, 3)]
    [InlineData(FindingType.CriticalNonConformity, 4)]
    [InlineData(FindingType.Recommendation, 5)]
    [InlineData(FindingType.BestPractice, 6)]
    public void FindingType_ShouldHaveExpectedValues(FindingType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(FindingStatus.Open, 1)]
    [InlineData(FindingStatus.InProgress, 2)]
    [InlineData(FindingStatus.Resolved, 3)]
    [InlineData(FindingStatus.Verified, 4)]
    [InlineData(FindingStatus.Closed, 5)]
    [InlineData(FindingStatus.Overdue, 6)]
    [InlineData(FindingStatus.Escalated, 7)]
    public void FindingStatus_ShouldHaveExpectedValues(FindingStatus status, int expectedValue)
    {
        ((int)status).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(RegulatoryReportType.AnnualCompliance, 1)]
    [InlineData(RegulatoryReportType.QuarterlyPLD, 2)]
    [InlineData(RegulatoryReportType.IncidentReport, 3)]
    [InlineData(RegulatoryReportType.AuditReport, 4)]
    [InlineData(RegulatoryReportType.RiskAssessment, 5)]
    [InlineData(RegulatoryReportType.TrainingReport, 6)]
    [InlineData(RegulatoryReportType.TransactionReport, 7)]
    [InlineData(RegulatoryReportType.UAFReport, 8)]
    [InlineData(RegulatoryReportType.SIBReport, 9)]
    [InlineData(RegulatoryReportType.DGIIReport, 10)]
    public void RegulatoryReportType_ShouldHaveExpectedValues(RegulatoryReportType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(ReportStatus.Draft, 1)]
    [InlineData(ReportStatus.PendingReview, 2)]
    [InlineData(ReportStatus.Approved, 3)]
    [InlineData(ReportStatus.Submitted, 4)]
    [InlineData(ReportStatus.Acknowledged, 5)]
    [InlineData(ReportStatus.Rejected, 6)]
    [InlineData(ReportStatus.RequiresCorrection, 7)]
    [InlineData(ReportStatus.Accepted, 8)]
    public void ReportStatus_ShouldHaveExpectedValues(ReportStatus status, int expectedValue)
    {
        ((int)status).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(ControlType.Preventive, 1)]
    [InlineData(ControlType.Detective, 2)]
    [InlineData(ControlType.Corrective, 3)]
    [InlineData(ControlType.Directive, 4)]
    public void ControlType_ShouldHaveExpectedValues(ControlType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(EvaluationFrequency.Daily, 1)]
    [InlineData(EvaluationFrequency.Weekly, 2)]
    [InlineData(EvaluationFrequency.Monthly, 3)]
    [InlineData(EvaluationFrequency.Quarterly, 4)]
    [InlineData(EvaluationFrequency.SemiAnnual, 5)]
    [InlineData(EvaluationFrequency.Annual, 6)]
    [InlineData(EvaluationFrequency.OnDemand, 7)]
    [InlineData(EvaluationFrequency.Continuous, 8)]
    public void EvaluationFrequency_ShouldHaveExpectedValues(EvaluationFrequency freq, int expectedValue)
    {
        ((int)freq).Should().Be(expectedValue);
    }

    // =====================================================
    // PARTE 2: Tests de RegulatoryFramework
    // =====================================================

    [Fact]
    public void RegulatoryFramework_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var framework = new RegulatoryFramework
        {
            Id = Guid.NewGuid(),
            Code = "PLD-155-17",
            Name = "Ley 155-17 Prevención de Lavado de Activos",
            Description = "Ley contra el lavado de activos y financiamiento del terrorismo",
            Type = RegulationType.PLD_AML,
            LegalReference = "Ley 155-17",
            RegulatoryBody = "UAF",
            EffectiveDate = new DateTime(2017, 6, 1),
            IsActive = true,
            Version = "1.0",
            CreatedBy = "admin"
        };

        // Assert
        framework.Code.Should().Be("PLD-155-17");
        framework.Type.Should().Be(RegulationType.PLD_AML);
        framework.RegulatoryBody.Should().Be("UAF");
        framework.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(RegulationType.PLD_AML, "UAF")]
    [InlineData(RegulationType.DataProtection, "OGDAI")]
    [InlineData(RegulationType.TaxCompliance, "DGII")]
    [InlineData(RegulationType.VehicleRegistration, "DGTT")]
    public void RegulatoryFramework_ShouldSupport_AllRegulationTypes(RegulationType type, string body)
    {
        // Arrange & Act
        var framework = new RegulatoryFramework
        {
            Id = Guid.NewGuid(),
            Code = $"REG-{type}",
            Name = $"Regulación {type}",
            Type = type,
            RegulatoryBody = body
        };

        // Assert
        framework.Type.Should().Be(type);
        framework.RegulatoryBody.Should().Be(body);
    }

    // =====================================================
    // PARTE 3: Tests de ComplianceRequirement
    // =====================================================

    [Fact]
    public void ComplianceRequirement_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var requirement = new ComplianceRequirement
        {
            Id = Guid.NewGuid(),
            FrameworkId = Guid.NewGuid(),
            Code = "REQ-001",
            Title = "Identificación de Clientes",
            Description = "Implementar procedimientos KYC para todos los clientes",
            Criticality = CriticalityLevel.Critical,
            ArticleReference = "Artículo 27",
            DeadlineDays = 30,
            EvaluationFrequency = EvaluationFrequency.Monthly,
            RequiresEvidence = true,
            RequiresApproval = true,
            IsActive = true
        };

        // Assert
        requirement.Code.Should().Be("REQ-001");
        requirement.Criticality.Should().Be(CriticalityLevel.Critical);
        requirement.DeadlineDays.Should().Be(30);
        requirement.RequiresEvidence.Should().BeTrue();
    }

    [Theory]
    [InlineData(CriticalityLevel.Critical, 1)]
    [InlineData(CriticalityLevel.High, 7)]
    [InlineData(CriticalityLevel.Medium, 30)]
    [InlineData(CriticalityLevel.Low, 90)]
    public void ComplianceRequirement_DeadlineDays_ShouldVaryByCriticality(CriticalityLevel level, int days)
    {
        // Arrange & Act
        var requirement = new ComplianceRequirement
        {
            Id = Guid.NewGuid(),
            FrameworkId = Guid.NewGuid(),
            Code = $"REQ-{level}",
            Title = $"Requirement {level}",
            Criticality = level,
            DeadlineDays = days
        };

        // Assert
        requirement.DeadlineDays.Should().Be(days);
    }

    // =====================================================
    // PARTE 4: Tests de ComplianceControl
    // =====================================================

    [Fact]
    public void ComplianceControl_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var control = new ComplianceControl
        {
            Id = Guid.NewGuid(),
            FrameworkId = Guid.NewGuid(),
            RequirementId = Guid.NewGuid(),
            Code = "CTRL-001",
            Name = "Verificación de Identidad",
            Description = "Control preventivo para verificar identidad de clientes",
            Type = ControlType.Preventive,
            ImplementationDetails = "Validar cédula contra base de datos JCE",
            ResponsibleRole = "Compliance Officer",
            TestingFrequency = EvaluationFrequency.Monthly,
            Status = ComplianceStatus.Compliant,
            EffectivenessScore = 95,
            IsActive = true
        };

        // Assert
        control.Code.Should().Be("CTRL-001");
        control.Type.Should().Be(ControlType.Preventive);
        control.Status.Should().Be(ComplianceStatus.Compliant);
        control.EffectivenessScore.Should().Be(95);
    }

    [Theory]
    [InlineData(ControlType.Preventive)]
    [InlineData(ControlType.Detective)]
    [InlineData(ControlType.Corrective)]
    [InlineData(ControlType.Directive)]
    public void ComplianceControl_ShouldSupport_AllControlTypes(ControlType type)
    {
        // Arrange & Act
        var control = new ComplianceControl
        {
            Id = Guid.NewGuid(),
            FrameworkId = Guid.NewGuid(),
            Code = $"CTRL-{type}",
            Name = $"Control {type}",
            Type = type
        };

        // Assert
        control.Type.Should().Be(type);
    }

    // =====================================================
    // PARTE 5: Tests de ControlTest
    // =====================================================

    [Fact]
    public void ControlTest_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var test = new ControlTest
        {
            Id = Guid.NewGuid(),
            ControlId = Guid.NewGuid(),
            TestDate = DateTime.UtcNow,
            TestedAt = DateTime.UtcNow,
            TestedBy = "auditor@okla.com.do",
            TestProcedure = "Revisión de muestra de 50 transacciones",
            TestResults = "48/50 cumplían con el control",
            IsPassed = true,
            EffectivenessScore = 96,
            Findings = "2 casos con documentación incompleta",
            Recommendations = "Reforzar validación de documentos"
        };

        // Assert
        test.IsPassed.Should().BeTrue();
        test.EffectivenessScore.Should().Be(96);
        test.Findings.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ControlTest_ShouldTrackEvidence()
    {
        // Arrange & Act
        var test = new ControlTest
        {
            Id = Guid.NewGuid(),
            ControlId = Guid.NewGuid(),
            TestedBy = "auditor",
            EvidenceDocuments = new List<string>
            {
                "evidencia_01.pdf",
                "screenshots_02.zip",
                "reporte_muestra.xlsx"
            }
        };

        // Assert
        test.EvidenceDocuments.Should().HaveCount(3);
    }

    // =====================================================
    // PARTE 6: Tests de ComplianceAssessment
    // =====================================================

    [Fact]
    public void ComplianceAssessment_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var assessment = new ComplianceAssessment
        {
            Id = Guid.NewGuid(),
            EntityType = "Dealer",
            EntityId = Guid.NewGuid(),
            RequirementId = Guid.NewGuid(),
            Status = ComplianceStatus.Compliant,
            AssessmentDate = DateTime.UtcNow,
            AssessedBy = "compliance.officer@okla.com.do",
            Score = 92,
            Observations = "Cumple con todos los requisitos de PLD",
            NextAssessmentDate = DateTime.UtcNow.AddMonths(3)
        };

        // Assert
        assessment.EntityType.Should().Be("Dealer");
        assessment.Status.Should().Be(ComplianceStatus.Compliant);
        assessment.Score.Should().Be(92);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Dealer")]
    [InlineData("Transaction")]
    [InlineData("Vehicle")]
    public void ComplianceAssessment_ShouldSupport_AllEntityTypes(string entityType)
    {
        // Arrange & Act
        var assessment = new ComplianceAssessment
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = Guid.NewGuid(),
            RequirementId = Guid.NewGuid(),
            Status = ComplianceStatus.Pending
        };

        // Assert
        assessment.EntityType.Should().Be(entityType);
    }

    // =====================================================
    // PARTE 7: Tests de ComplianceFinding
    // =====================================================

    [Fact]
    public void ComplianceFinding_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var finding = new ComplianceFinding
        {
            Id = Guid.NewGuid(),
            AssessmentId = Guid.NewGuid(),
            Title = "Documentación KYC Incompleta",
            Description = "Se encontraron registros sin documentos de identidad verificados",
            Type = FindingType.MinorNonConformity,
            Status = FindingStatus.Open,
            Criticality = CriticalityLevel.Medium,
            RootCause = "Falta de validación automática",
            Impact = "Riesgo de incumplimiento regulatorio",
            Recommendation = "Implementar validación obligatoria de documentos",
            AssignedTo = "it@okla.com.do",
            DueDate = DateTime.UtcNow.AddDays(15),
            CreatedBy = "auditor"
        };

        // Assert
        finding.Type.Should().Be(FindingType.MinorNonConformity);
        finding.Status.Should().Be(FindingStatus.Open);
        finding.Criticality.Should().Be(CriticalityLevel.Medium);
    }

    [Fact]
    public void ComplianceFinding_ShouldTransition_ToResolved()
    {
        // Arrange
        var finding = new ComplianceFinding
        {
            Id = Guid.NewGuid(),
            AssessmentId = Guid.NewGuid(),
            Title = "Hallazgo de prueba",
            Type = FindingType.Observation,
            Status = FindingStatus.Open,
            Criticality = CriticalityLevel.Low,
            CreatedBy = "auditor"
        };

        // Act
        finding.Status = FindingStatus.Resolved;
        finding.ResolvedAt = DateTime.UtcNow;
        finding.ResolvedBy = "developer";
        finding.Resolution = "Se implementó validación automática";

        // Assert
        finding.Status.Should().Be(FindingStatus.Resolved);
        finding.ResolvedAt.Should().NotBeNull();
        finding.Resolution.Should().NotBeNullOrEmpty();
    }

    // =====================================================
    // PARTE 8: Tests de RemediationAction
    // =====================================================

    [Fact]
    public void RemediationAction_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var action = new RemediationAction
        {
            Id = Guid.NewGuid(),
            FindingId = Guid.NewGuid(),
            Title = "Implementar validación de cédula",
            Description = "Agregar validación con API de JCE",
            Status = ComplianceService.Domain.Entities.TaskStatus.Pending,
            AssignedTo = "developer@okla.com.do",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = 1,
            RequiresVerification = true,
            CreatedBy = "compliance.officer"
        };

        // Assert
        action.Status.Should().Be(ComplianceService.Domain.Entities.TaskStatus.Pending);
        action.Priority.Should().Be(1);
        action.RequiresVerification.Should().BeTrue();
    }

    [Fact]
    public void RemediationAction_ShouldTrack_CompletionAndVerification()
    {
        // Arrange
        var action = new RemediationAction
        {
            Id = Guid.NewGuid(),
            FindingId = Guid.NewGuid(),
            Title = "Acción correctiva",
            Status = ComplianceService.Domain.Entities.TaskStatus.Pending,
            RequiresVerification = true,
            CreatedBy = "auditor"
        };

        // Act - Complete
        action.Status = ComplianceService.Domain.Entities.TaskStatus.Completed;
        action.CompletedAt = DateTime.UtcNow;
        action.CompletedBy = "developer";
        action.CompletionNotes = "Implementación completada";

        // Act - Verify
        action.VerifiedAt = DateTime.UtcNow.AddDays(1);
        action.VerifiedBy = "qa@okla.com.do";

        // Assert
        action.Status.Should().Be(ComplianceService.Domain.Entities.TaskStatus.Completed);
        action.CompletedAt.Should().NotBeNull();
        action.VerifiedAt.Should().NotBeNull();
    }

    // =====================================================
    // PARTE 9: Tests de RegulatoryReport
    // =====================================================

    [Fact]
    public void RegulatoryReport_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var report = new RegulatoryReport
        {
            Id = Guid.NewGuid(),
            ReportNumber = "UAF-2026-Q1-001",
            Type = RegulatoryReportType.QuarterlyPLD,
            RegulationType = RegulationType.PLD_AML,
            Title = "Reporte Trimestral PLD Q1 2026",
            Description = "Reporte de cumplimiento anti-lavado primer trimestre",
            PeriodStart = new DateTime(2026, 1, 1),
            PeriodEnd = new DateTime(2026, 3, 31),
            Status = ReportStatus.Draft,
            RegulatoryBody = "UAF",
            SubmissionDeadline = new DateTime(2026, 4, 15),
            CreatedBy = "compliance.officer"
        };

        // Assert
        report.ReportNumber.Should().Be("UAF-2026-Q1-001");
        report.Type.Should().Be(RegulatoryReportType.QuarterlyPLD);
        report.RegulatoryBody.Should().Be("UAF");
        report.Status.Should().Be(ReportStatus.Draft);
    }

    [Fact]
    public void RegulatoryReport_ShouldTrack_ApprovalWorkflow()
    {
        // Arrange
        var report = new RegulatoryReport
        {
            Id = Guid.NewGuid(),
            ReportNumber = "DGII-2026-001",
            Type = RegulatoryReportType.DGIIReport,
            RegulationType = RegulationType.TaxCompliance,
            Title = "Reporte DGII",
            Status = ReportStatus.Draft,
            CreatedBy = "contador"
        };

        // Act - Prepare
        report.PreparedBy = "contador@okla.com.do";
        report.PreparedAt = DateTime.UtcNow;
        report.Status = ReportStatus.PendingReview;

        // Act - Review
        report.ReviewedBy = "supervisor@okla.com.do";
        report.ReviewedAt = DateTime.UtcNow.AddHours(2);

        // Act - Approve
        report.ApprovedBy = "cfo@okla.com.do";
        report.ApprovedAt = DateTime.UtcNow.AddHours(4);
        report.Status = ReportStatus.Approved;

        // Assert
        report.PreparedBy.Should().NotBeNull();
        report.ReviewedBy.Should().NotBeNull();
        report.ApprovedBy.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.Approved);
    }

    [Fact]
    public void RegulatoryReport_ShouldTrack_Submission()
    {
        // Arrange
        var report = new RegulatoryReport
        {
            Id = Guid.NewGuid(),
            ReportNumber = "UAF-2026-001",
            Type = RegulatoryReportType.UAFReport,
            Status = ReportStatus.Approved,
            CreatedBy = "compliance"
        };

        // Act
        report.SubmittedAt = DateTime.UtcNow;
        report.SubmittedBy = "oficial.cumplimiento@okla.com.do";
        report.SubmissionReference = "UAF-REC-2026-12345";
        report.Status = ReportStatus.Submitted;

        // Assert
        report.Status.Should().Be(ReportStatus.Submitted);
        report.SubmissionReference.Should().NotBeNullOrEmpty();
    }

    // =====================================================
    // PARTE 10: Tests de Navegación y Relaciones
    // =====================================================

    [Fact]
    public void RegulatoryFramework_ShouldHave_RequirementsCollection()
    {
        // Arrange
        var framework = new RegulatoryFramework
        {
            Id = Guid.NewGuid(),
            Code = "PLD-155-17",
            Name = "Ley 155-17",
            Type = RegulationType.PLD_AML,
            CreatedBy = "admin"
        };

        var req1 = new ComplianceRequirement
        {
            Id = Guid.NewGuid(),
            FrameworkId = framework.Id,
            Code = "REQ-001",
            Title = "KYC",
            CreatedBy = "admin"
        };

        var req2 = new ComplianceRequirement
        {
            Id = Guid.NewGuid(),
            FrameworkId = framework.Id,
            Code = "REQ-002",
            Title = "Due Diligence",
            CreatedBy = "admin"
        };

        // Act
        framework.Requirements.Add(req1);
        framework.Requirements.Add(req2);

        // Assert
        framework.Requirements.Should().HaveCount(2);
    }

    [Fact]
    public void ComplianceFinding_ShouldHave_RemediationActionsCollection()
    {
        // Arrange
        var finding = new ComplianceFinding
        {
            Id = Guid.NewGuid(),
            AssessmentId = Guid.NewGuid(),
            Title = "Hallazgo",
            Type = FindingType.MajorNonConformity,
            Criticality = CriticalityLevel.High,
            CreatedBy = "auditor"
        };

        var action1 = new RemediationAction
        {
            Id = Guid.NewGuid(),
            FindingId = finding.Id,
            Title = "Acción 1",
            CreatedBy = "auditor"
        };

        var action2 = new RemediationAction
        {
            Id = Guid.NewGuid(),
            FindingId = finding.Id,
            Title = "Acción 2",
            CreatedBy = "auditor"
        };

        // Act
        finding.RemediationActions.Add(action1);
        finding.RemediationActions.Add(action2);

        // Assert
        finding.RemediationActions.Should().HaveCount(2);
    }

    // =====================================================
    // PARTE 11: Tests de Regulaciones Dominicanas
    // =====================================================

    [Fact]
    public void RegulatoryFramework_ShouldSupport_DominicanLaws()
    {
        // Ley 155-17 (PLD/AML)
        var pldFramework = new RegulatoryFramework
        {
            Id = Guid.NewGuid(),
            Code = "LEY-155-17",
            Name = "Ley 155-17 Prevención de Lavado de Activos",
            Type = RegulationType.PLD_AML,
            LegalReference = "Gaceta Oficial No. 10895",
            RegulatoryBody = "UAF",
            EffectiveDate = new DateTime(2017, 6, 1),
            CreatedBy = "admin"
        };

        // Ley 172-13 (Protección de Datos)
        var dataProtectionFramework = new RegulatoryFramework
        {
            Id = Guid.NewGuid(),
            Code = "LEY-172-13",
            Name = "Ley 172-13 Protección de Datos Personales",
            Type = RegulationType.DataProtection,
            LegalReference = "Gaceta Oficial No. 10737",
            RegulatoryBody = "OGDAI",
            EffectiveDate = new DateTime(2013, 12, 15),
            CreatedBy = "admin"
        };

        // Assert
        pldFramework.Type.Should().Be(RegulationType.PLD_AML);
        dataProtectionFramework.Type.Should().Be(RegulationType.DataProtection);
    }

    [Fact]
    public void RegulatoryReport_ShouldSupport_AllDominicanRegulatoryBodies()
    {
        var bodies = new[] { "UAF", "DGII", "SIB", "SIPEN", "DGTT", "ProConsumidor", "MARENA" };

        foreach (var body in bodies)
        {
            var report = new RegulatoryReport
            {
                Id = Guid.NewGuid(),
                ReportNumber = $"{body}-2026-001",
                Type = RegulatoryReportType.AnnualCompliance,
                Title = $"Reporte {body}",
                RegulatoryBody = body,
                CreatedBy = "compliance"
            };

            report.RegulatoryBody.Should().Be(body);
        }
    }
}
