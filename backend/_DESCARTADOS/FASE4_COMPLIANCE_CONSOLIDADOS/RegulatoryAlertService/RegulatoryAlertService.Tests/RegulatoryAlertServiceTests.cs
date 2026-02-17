// =====================================================
// C11: RegulatoryAlertService - Tests Unitarios
// Alertas de Cambios Regulatorios de República Dominicana
// =====================================================

using FluentAssertions;
using RegulatoryAlertService.Domain.Entities;
using RegulatoryAlertService.Domain.Enums;
using Xunit;

namespace RegulatoryAlertService.Tests;

public class RegulatoryAlertServiceTests
{
    // =====================================================
    // Tests de RegulatoryBody Enum - Entidades Reguladoras RD
    // =====================================================

    [Fact]
    public void RegulatoryBody_ShouldHave_DGII_AsTaxAuthority()
    {
        // DGII - Dirección General de Impuestos Internos
        RegulatoryBody.DGII.Should().BeDefined();
        ((int)RegulatoryBody.DGII).Should().Be(1);
    }

    [Fact]
    public void RegulatoryBody_ShouldHave_ProConsumidor()
    {
        // Pro-Consumidor - Protección del Consumidor
        RegulatoryBody.ProConsumidor.Should().BeDefined();
        ((int)RegulatoryBody.ProConsumidor).Should().Be(2);
    }

    [Fact]
    public void RegulatoryBody_ShouldHave_UAF_ForAML()
    {
        // UAF - Unidad de Análisis Financiero (Anti-Lavado)
        RegulatoryBody.UAF.Should().BeDefined();
        ((int)RegulatoryBody.UAF).Should().Be(3);
    }

    [Fact]
    public void RegulatoryBody_ShouldHave_AllDominicanAuthorities()
    {
        // Verificar todas las entidades reguladoras dominicanas
        RegulatoryBody.DGII.Should().BeDefined();
        RegulatoryBody.ProConsumidor.Should().BeDefined();
        RegulatoryBody.UAF.Should().BeDefined();
        RegulatoryBody.SuperintendenciaBancos.Should().BeDefined();
        RegulatoryBody.INDOTEL.Should().BeDefined();
        RegulatoryBody.ProCompetencia.Should().BeDefined();
        RegulatoryBody.OGTIC.Should().BeDefined();
        RegulatoryBody.MedioAmbiente.Should().BeDefined();
        RegulatoryBody.MinisterioTrabajo.Should().BeDefined();
        RegulatoryBody.CamaraDeCuentas.Should().BeDefined();
        RegulatoryBody.CongresoNacional.Should().BeDefined();
        RegulatoryBody.PoderJudicial.Should().BeDefined();
    }

    // =====================================================
    // Tests de AlertType Enum - Tipos de Alertas
    // =====================================================

    [Fact]
    public void AlertType_ShouldHave_DeadlineTypes()
    {
        // Fechas límite
        AlertType.FilingDeadline.Should().BeDefined();
        AlertType.PaymentDeadline.Should().BeDefined();
        AlertType.RenewalDeadline.Should().BeDefined();
        
        ((int)AlertType.FilingDeadline).Should().Be(1);
        ((int)AlertType.PaymentDeadline).Should().Be(2);
        ((int)AlertType.RenewalDeadline).Should().Be(3);
    }

    [Fact]
    public void AlertType_ShouldHave_RegulatoryChangeTypes()
    {
        // Cambios regulatorios
        AlertType.NewLaw.Should().BeDefined();
        AlertType.LawAmendment.Should().BeDefined();
        AlertType.NewRegulation.Should().BeDefined();
        AlertType.NewResolution.Should().BeDefined();
        
        ((int)AlertType.NewLaw).Should().Be(10);
        ((int)AlertType.LawAmendment).Should().Be(11);
        ((int)AlertType.NewRegulation).Should().Be(12);
        ((int)AlertType.NewResolution).Should().Be(13);
    }

    [Fact]
    public void AlertType_ShouldHave_ComplianceTypes()
    {
        // Tipos de cumplimiento
        AlertType.ComplianceReminder.Should().BeDefined();
        AlertType.AuditNotice.Should().BeDefined();
        AlertType.InspectionNotice.Should().BeDefined();
        
        ((int)AlertType.ComplianceReminder).Should().Be(20);
        ((int)AlertType.AuditNotice).Should().Be(21);
        ((int)AlertType.InspectionNotice).Should().Be(22);
    }

    [Fact]
    public void AlertType_ShouldHave_TaxRateTypes()
    {
        // Cambios de tasas
        AlertType.TaxRateChange.Should().BeDefined();
        AlertType.FeeIncrease.Should().BeDefined();
        
        ((int)AlertType.TaxRateChange).Should().Be(30);
        ((int)AlertType.FeeIncrease).Should().Be(31);
    }

    [Fact]
    public void AlertType_ShouldHave_SanctionTypes()
    {
        // Sanciones
        AlertType.SanctionWarning.Should().BeDefined();
        AlertType.FineNotice.Should().BeDefined();
        
        ((int)AlertType.SanctionWarning).Should().Be(50);
        ((int)AlertType.FineNotice).Should().Be(51);
    }

    // =====================================================
    // Tests de AlertPriority Enum - Prioridad de Alertas
    // =====================================================

    [Fact]
    public void AlertPriority_ShouldHave_AllLevels()
    {
        AlertPriority.Low.Should().BeDefined();
        AlertPriority.Medium.Should().BeDefined();
        AlertPriority.High.Should().BeDefined();
        AlertPriority.Critical.Should().BeDefined();
        AlertPriority.Urgent.Should().BeDefined();
    }

    [Fact]
    public void AlertPriority_ShouldBe_InCorrectOrder()
    {
        // Low < Medium < High < Critical < Urgent
        ((int)AlertPriority.Low).Should().Be(1);
        ((int)AlertPriority.Medium).Should().Be(2);
        ((int)AlertPriority.High).Should().Be(3);
        ((int)AlertPriority.Critical).Should().Be(4);
        ((int)AlertPriority.Urgent).Should().Be(5);
    }

    // =====================================================
    // Tests de AlertStatus Enum - Estados de Alertas
    // =====================================================

    [Fact]
    public void AlertStatus_ShouldHave_AllStates()
    {
        AlertStatus.Draft.Should().BeDefined();
        AlertStatus.Active.Should().BeDefined();
        AlertStatus.Acknowledged.Should().BeDefined();
        AlertStatus.InProgress.Should().BeDefined();
        AlertStatus.Resolved.Should().BeDefined();
        AlertStatus.Expired.Should().BeDefined();
        AlertStatus.Dismissed.Should().BeDefined();
        AlertStatus.Escalated.Should().BeDefined();
    }

    [Fact]
    public void AlertStatus_ShouldHave_CorrectValues()
    {
        ((int)AlertStatus.Draft).Should().Be(1);
        ((int)AlertStatus.Active).Should().Be(2);
        ((int)AlertStatus.Acknowledged).Should().Be(3);
        ((int)AlertStatus.InProgress).Should().Be(4);
        ((int)AlertStatus.Resolved).Should().Be(5);
    }

    // =====================================================
    // Tests de SubscriptionFrequency Enum
    // =====================================================

    [Fact]
    public void SubscriptionFrequency_ShouldHave_AllOptions()
    {
        SubscriptionFrequency.Immediate.Should().BeDefined();
        SubscriptionFrequency.Daily.Should().BeDefined();
        SubscriptionFrequency.Weekly.Should().BeDefined();
        SubscriptionFrequency.Monthly.Should().BeDefined();
        
        ((int)SubscriptionFrequency.Immediate).Should().Be(1);
        ((int)SubscriptionFrequency.Daily).Should().Be(2);
        ((int)SubscriptionFrequency.Weekly).Should().Be(3);
        ((int)SubscriptionFrequency.Monthly).Should().Be(4);
    }

    // =====================================================
    // Tests de NotificationChannel Enum
    // =====================================================

    [Fact]
    public void NotificationChannel_ShouldHave_AllChannels()
    {
        NotificationChannel.Email.Should().BeDefined();
        NotificationChannel.SMS.Should().BeDefined();
        NotificationChannel.Push.Should().BeDefined();
        NotificationChannel.InApp.Should().BeDefined();
        NotificationChannel.Webhook.Should().BeDefined();
        NotificationChannel.WhatsApp.Should().BeDefined();
    }

    [Fact]
    public void NotificationChannel_ShouldHave_CorrectValues()
    {
        ((int)NotificationChannel.Email).Should().Be(1);
        ((int)NotificationChannel.SMS).Should().Be(2);
        ((int)NotificationChannel.Push).Should().Be(3);
        ((int)NotificationChannel.InApp).Should().Be(4);
        ((int)NotificationChannel.Webhook).Should().Be(5);
        ((int)NotificationChannel.WhatsApp).Should().Be(6);
    }

    // =====================================================
    // Tests de RegulatoryCategory Enum
    // =====================================================

    [Fact]
    public void RegulatoryCategory_ShouldHave_DominicanLawCategories()
    {
        RegulatoryCategory.Tax.Should().BeDefined();               // DGII
        RegulatoryCategory.ConsumerProtection.Should().BeDefined(); // Pro-Consumidor
        RegulatoryCategory.AML.Should().BeDefined();               // UAF, Ley 155-17
        RegulatoryCategory.DataProtection.Should().BeDefined();    // Ley 172-13
        RegulatoryCategory.ECommerce.Should().BeDefined();         // Ley 126-02
    }

    [Fact]
    public void RegulatoryCategory_ShouldHave_AllCategories()
    {
        RegulatoryCategory.Tax.Should().BeDefined();
        RegulatoryCategory.ConsumerProtection.Should().BeDefined();
        RegulatoryCategory.AML.Should().BeDefined();
        RegulatoryCategory.DataProtection.Should().BeDefined();
        RegulatoryCategory.ECommerce.Should().BeDefined();
        RegulatoryCategory.Labor.Should().BeDefined();
        RegulatoryCategory.Environment.Should().BeDefined();
        RegulatoryCategory.Competition.Should().BeDefined();
        RegulatoryCategory.Financial.Should().BeDefined();
        RegulatoryCategory.Telecommunications.Should().BeDefined();
        RegulatoryCategory.General.Should().BeDefined();
    }

    [Fact]
    public void RegulatoryCategory_ShouldHave_CorrectValues()
    {
        ((int)RegulatoryCategory.Tax).Should().Be(1);
        ((int)RegulatoryCategory.ConsumerProtection).Should().Be(2);
        ((int)RegulatoryCategory.AML).Should().Be(3);
        ((int)RegulatoryCategory.DataProtection).Should().Be(4);
        ((int)RegulatoryCategory.ECommerce).Should().Be(5);
        ((int)RegulatoryCategory.General).Should().Be(99);
    }

    // =====================================================
    // Tests de RegulatoryAlert Entity
    // =====================================================

    [Fact]
    public void RegulatoryAlert_ShouldCreate_WithValidData()
    {
        // Arrange & Act
        var alert = new RegulatoryAlert(
            title: "Nueva Resolución DGII sobre Facturación Electrónica",
            description: "La DGII ha publicado nuevas normas sobre e-CF...",
            alertType: AlertType.NewResolution,
            priority: AlertPriority.High,
            regulatoryBody: RegulatoryBody.DGII,
            category: RegulatoryCategory.Tax,
            createdBy: "admin@okla.com.do"
        );

        // Assert
        alert.Should().NotBeNull();
        alert.Title.Should().Be("Nueva Resolución DGII sobre Facturación Electrónica");
        alert.AlertType.Should().Be(AlertType.NewResolution);
        alert.Priority.Should().Be(AlertPriority.High);
        alert.RegulatoryBody.Should().Be(RegulatoryBody.DGII);
        alert.Category.Should().Be(RegulatoryCategory.Tax);
    }

    [Fact]
    public void RegulatoryAlert_ShouldCreate_ForDGII_TaxDeadline()
    {
        // Arrange & Act - DGII filing deadline alert
        var alert = new RegulatoryAlert(
            title: "Fecha límite: Declaración ITBIS Enero 2026",
            description: "Recordatorio: La declaración de ITBIS debe presentarse antes del día 20",
            alertType: AlertType.FilingDeadline,
            priority: AlertPriority.Critical,
            regulatoryBody: RegulatoryBody.DGII,
            category: RegulatoryCategory.Tax
        );

        // Assert
        alert.RegulatoryBody.Should().Be(RegulatoryBody.DGII);
        alert.AlertType.Should().Be(AlertType.FilingDeadline);
        alert.Priority.Should().Be(AlertPriority.Critical);
        alert.Category.Should().Be(RegulatoryCategory.Tax);
    }

    [Fact]
    public void RegulatoryAlert_ShouldCreate_ForUAF_ComplianceReminder()
    {
        // Arrange - UAF compliance reminder
        var alert = new RegulatoryAlert(
            title: "Recordatorio: Reporte ROS mensual UAF",
            description: "Recordatorio mensual para envío de Reporte de Operaciones Sospechosas",
            alertType: AlertType.ComplianceReminder,
            priority: AlertPriority.High,
            regulatoryBody: RegulatoryBody.UAF,
            category: RegulatoryCategory.AML
        );

        // Assert
        alert.RegulatoryBody.Should().Be(RegulatoryBody.UAF);
        alert.AlertType.Should().Be(AlertType.ComplianceReminder);
        alert.Category.Should().Be(RegulatoryCategory.AML);
    }

    [Fact]
    public void RegulatoryAlert_ShouldCreate_ForProConsumidor()
    {
        // Arrange - Consumer protection new regulation
        var alert = new RegulatoryAlert(
            title: "Pro-Consumidor: Nueva resolución sobre garantías",
            description: "Nueva resolución que modifica requisitos de garantía",
            alertType: AlertType.NewResolution,
            priority: AlertPriority.Medium,
            regulatoryBody: RegulatoryBody.ProConsumidor,
            category: RegulatoryCategory.ConsumerProtection
        );

        // Assert
        alert.RegulatoryBody.Should().Be(RegulatoryBody.ProConsumidor);
        alert.AlertType.Should().Be(AlertType.NewResolution);
        alert.Category.Should().Be(RegulatoryCategory.ConsumerProtection);
    }

    [Fact]
    public void RegulatoryAlert_ShouldSetDeadline()
    {
        // Arrange
        var alert = new RegulatoryAlert(
            title: "DGII: Fecha límite declaración anual",
            description: "Declaración IR-2 anual",
            alertType: AlertType.FilingDeadline,
            priority: AlertPriority.Urgent,
            regulatoryBody: RegulatoryBody.DGII,
            category: RegulatoryCategory.Tax
        );
        var deadline = new DateTime(2026, 4, 30);

        // Act
        alert.SetDeadline(deadline);

        // Assert
        alert.DeadlineDate.Should().Be(deadline);
    }

    [Fact]
    public void RegulatoryAlert_ShouldSetEffectiveDate()
    {
        // Arrange
        var alert = new RegulatoryAlert(
            title: "Nueva Ley ITBIS",
            description: "Cambios en la Ley de ITBIS",
            alertType: AlertType.NewLaw,
            priority: AlertPriority.High,
            regulatoryBody: RegulatoryBody.DGII,
            category: RegulatoryCategory.Tax
        );
        var effectiveDate = new DateTime(2026, 7, 1);

        // Act
        alert.SetEffectiveDate(effectiveDate);

        // Assert
        alert.EffectiveDate.Should().Be(effectiveDate);
    }

    [Fact]
    public void RegulatoryAlert_ShouldSetLegalReference()
    {
        // Arrange
        var alert = new RegulatoryAlert(
            title: "Ley 155-17 Modificaciones",
            description: "Actualizaciones a la Ley contra el Lavado",
            alertType: AlertType.LawAmendment,
            priority: AlertPriority.High,
            regulatoryBody: RegulatoryBody.UAF,
            category: RegulatoryCategory.AML
        );

        // Act
        alert.SetLegalReference("Ley 155-17, Art. 25", "https://dgii.gov.do/ley-155-17");

        // Assert
        alert.LegalReference.Should().Be("Ley 155-17, Art. 25");
        alert.OfficialDocumentUrl.Should().Be("https://dgii.gov.do/ley-155-17");
    }

    [Fact]
    public void RegulatoryAlert_ShouldSetActionRequired()
    {
        // Arrange
        var alert = new RegulatoryAlert(
            title: "DGII: Nueva obligación tributaria",
            description: "Nueva obligación de reporte",
            alertType: AlertType.NewRegulation,
            priority: AlertPriority.Critical,
            regulatoryBody: RegulatoryBody.DGII,
            category: RegulatoryCategory.Tax
        );

        // Act
        alert.SetActionRequired("Actualizar sistema de facturación electrónica");

        // Assert
        alert.RequiresAction.Should().BeTrue();
        alert.ActionRequired.Should().Be("Actualizar sistema de facturación electrónica");
    }

    [Fact]
    public void RegulatoryAlert_ShouldPublish()
    {
        // Arrange
        var alert = new RegulatoryAlert(
            title: "Alerta de prueba",
            description: "Descripción de prueba",
            alertType: AlertType.ComplianceReminder,
            priority: AlertPriority.Medium,
            regulatoryBody: RegulatoryBody.DGII,
            category: RegulatoryCategory.Tax
        );

        // Initial status should be Draft
        alert.Status.Should().Be(AlertStatus.Draft);

        // Act
        alert.Publish();

        // Assert
        alert.Status.Should().Be(AlertStatus.Active);
    }

    [Fact]
    public void RegulatoryAlert_ShouldAcknowledge()
    {
        // Arrange
        var alert = new RegulatoryAlert(
            title: "Alerta de prueba",
            description: "Descripción de prueba",
            alertType: AlertType.ComplianceReminder,
            priority: AlertPriority.Medium,
            regulatoryBody: RegulatoryBody.DGII,
            category: RegulatoryCategory.Tax
        );
        alert.Publish();

        // Act
        alert.Acknowledge("user123");

        // Assert
        alert.Status.Should().Be(AlertStatus.Acknowledged);
    }

    // =====================================================
    // Tests de AlertSubscription Entity
    // =====================================================

    [Fact]
    public void AlertSubscription_ShouldCreate_WithValidData()
    {
        // Arrange & Act
        var subscription = new AlertSubscription(
            userId: Guid.NewGuid().ToString(),
            frequency: SubscriptionFrequency.Immediate,
            preferredChannel: NotificationChannel.Email,
            email: "dealer@okla.com.do"
        );

        // Assert
        subscription.Should().NotBeNull();
        subscription.Frequency.Should().Be(SubscriptionFrequency.Immediate);
        subscription.PreferredChannel.Should().Be(NotificationChannel.Email);
    }

    [Fact]
    public void AlertSubscription_ShouldCreate_ForUAFWithDailyDigest()
    {
        // Arrange & Act
        var subscription = new AlertSubscription(
            userId: Guid.NewGuid().ToString(),
            frequency: SubscriptionFrequency.Daily,
            preferredChannel: NotificationChannel.Email,
            email: "compliance@dealer.com"
        );
        subscription.FilterByRegulatoryBody(RegulatoryBody.UAF);
        subscription.FilterByCategory(RegulatoryCategory.AML);

        // Assert
        subscription.RegulatoryBody.Should().Be(RegulatoryBody.UAF);
        subscription.Category.Should().Be(RegulatoryCategory.AML);
        subscription.Frequency.Should().Be(SubscriptionFrequency.Daily);
    }

    // =====================================================
    // Tests de Dominican Regulatory Calendar
    // =====================================================

    [Theory]
    [InlineData(1, 20, "DGII: ITBIS mensual")]
    [InlineData(3, 31, "DGII: Cierre trimestre")]
    [InlineData(4, 30, "DGII: Declaración IR-2 anual")]
    public void DGIIDeadlines_ShouldBeConfigurable(int month, int day, string description)
    {
        // Arrange
        var year = DateTime.UtcNow.Year + 1;
        var expectedDate = new DateTime(year, month, day);

        // Act
        var alert = new RegulatoryAlert(
            title: description,
            description: $"Fecha límite: {description}",
            alertType: AlertType.FilingDeadline,
            priority: AlertPriority.Critical,
            regulatoryBody: RegulatoryBody.DGII,
            category: RegulatoryCategory.Tax
        );
        alert.SetDeadline(expectedDate);

        // Assert
        alert.DeadlineDate.Should().NotBeNull();
        alert.DeadlineDate!.Value.Month.Should().Be(month);
        alert.DeadlineDate.Value.Day.Should().Be(day);
        alert.RegulatoryBody.Should().Be(RegulatoryBody.DGII);
    }

    [Fact]
    public void UAFReporting_ShouldBeMonthlyCompliance()
    {
        // Arrange - UAF suspicious transaction reporting deadline (monthly)
        var alert = new RegulatoryAlert(
            title: "UAF: Reporte mensual de transacciones sospechosas",
            description: "Reporte mensual obligatorio bajo Ley 155-17",
            alertType: AlertType.ComplianceReminder,
            priority: AlertPriority.High,
            regulatoryBody: RegulatoryBody.UAF,
            category: RegulatoryCategory.AML
        );

        // Assert
        alert.RegulatoryBody.Should().Be(RegulatoryBody.UAF);
        alert.Category.Should().Be(RegulatoryCategory.AML);
        alert.Title.Should().Contain("mensual");
    }
}
