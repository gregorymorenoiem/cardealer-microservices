// =====================================================
// C12: ComplianceIntegrationService - Tests Unitarios
// Integración con Entes Reguladores de República Dominicana
// DGII, UAF, Pro-Consumidor, OGTIC, TSS, DGA, etc.
// =====================================================

using FluentAssertions;
using ComplianceIntegrationService.Domain.Entities;
using ComplianceIntegrationService.Domain.Enums;
using Xunit;

namespace ComplianceIntegrationService.Tests;

public class ComplianceIntegrationServiceTests
{
    // =====================================================
    // Tests de RegulatoryBody Enum - Entes Reguladores RD
    // =====================================================

    [Fact]
    public void RegulatoryBody_ShouldHave_DGII()
    {
        // DGII - Dirección General de Impuestos Internos
        RegulatoryBody.DGII.Should().BeDefined();
        ((int)RegulatoryBody.DGII).Should().Be(1);
    }

    [Fact]
    public void RegulatoryBody_ShouldHave_UAF()
    {
        // UAF - Unidad de Análisis Financiero (Anti-Lavado)
        RegulatoryBody.UAF.Should().BeDefined();
        ((int)RegulatoryBody.UAF).Should().Be(2);
    }

    [Fact]
    public void RegulatoryBody_ShouldHave_ProConsumidor()
    {
        // Pro-Consumidor - Protección del Consumidor
        RegulatoryBody.ProConsumidor.Should().BeDefined();
        ((int)RegulatoryBody.ProConsumidor).Should().Be(3);
    }

    [Fact]
    public void RegulatoryBody_ShouldHave_AllDominicanAuthorities()
    {
        // Verificar todos los entes reguladores dominicanos
        RegulatoryBody.DGII.Should().BeDefined();
        RegulatoryBody.UAF.Should().BeDefined();
        RegulatoryBody.ProConsumidor.Should().BeDefined();
        RegulatoryBody.SuperintendenciaBancos.Should().BeDefined();
        RegulatoryBody.INDOTEL.Should().BeDefined();
        RegulatoryBody.ProCompetencia.Should().BeDefined();
        RegulatoryBody.OGTIC.Should().BeDefined();
        RegulatoryBody.DGA.Should().BeDefined();
        RegulatoryBody.MedioAmbiente.Should().BeDefined();
        RegulatoryBody.MinisterioTrabajo.Should().BeDefined();
        RegulatoryBody.TSS.Should().BeDefined();
        RegulatoryBody.INFOTEP.Should().BeDefined();
        RegulatoryBody.CamaraComercio.Should().BeDefined();
    }

    // =====================================================
    // Tests de IntegrationType Enum
    // =====================================================

    [Fact]
    public void IntegrationType_ShouldHave_AllTypes()
    {
        IntegrationType.ApiRest.Should().BeDefined();
        IntegrationType.WebServiceSoap.Should().BeDefined();
        IntegrationType.Sftp.Should().BeDefined();
        IntegrationType.Email.Should().BeDefined();
        IntegrationType.WebPortal.Should().BeDefined();
        IntegrationType.FlatFile.Should().BeDefined();
        IntegrationType.XmlFile.Should().BeDefined();
        IntegrationType.JsonFile.Should().BeDefined();
        IntegrationType.DirectDatabase.Should().BeDefined();
    }

    [Fact]
    public void IntegrationType_ShouldHave_CorrectValues()
    {
        ((int)IntegrationType.ApiRest).Should().Be(1);
        ((int)IntegrationType.WebServiceSoap).Should().Be(2);
        ((int)IntegrationType.Sftp).Should().Be(3);
        ((int)IntegrationType.Email).Should().Be(4);
        ((int)IntegrationType.WebPortal).Should().Be(5);
    }

    // =====================================================
    // Tests de IntegrationStatus Enum
    // =====================================================

    [Fact]
    public void IntegrationStatus_ShouldHave_AllStates()
    {
        IntegrationStatus.Active.Should().BeDefined();
        IntegrationStatus.Inactive.Should().BeDefined();
        IntegrationStatus.Configuring.Should().BeDefined();
        IntegrationStatus.Testing.Should().BeDefined();
        IntegrationStatus.Error.Should().BeDefined();
        IntegrationStatus.Maintenance.Should().BeDefined();
        IntegrationStatus.Deprecated.Should().BeDefined();
    }

    [Fact]
    public void IntegrationStatus_ShouldHave_CorrectValues()
    {
        ((int)IntegrationStatus.Active).Should().Be(1);
        ((int)IntegrationStatus.Inactive).Should().Be(2);
        ((int)IntegrationStatus.Configuring).Should().Be(3);
        ((int)IntegrationStatus.Testing).Should().Be(4);
        ((int)IntegrationStatus.Error).Should().Be(5);
    }

    // =====================================================
    // Tests de CredentialType Enum
    // =====================================================

    [Fact]
    public void CredentialType_ShouldHave_AllTypes()
    {
        CredentialType.BasicAuth.Should().BeDefined();
        CredentialType.BearerToken.Should().BeDefined();
        CredentialType.ApiKey.Should().BeDefined();
        CredentialType.Certificate.Should().BeDefined();
        CredentialType.OAuth2.Should().BeDefined();
        CredentialType.DigitalSignature.Should().BeDefined();
        CredentialType.Hmac.Should().BeDefined();
    }

    [Fact]
    public void CredentialType_ShouldHave_CorrectValues()
    {
        ((int)CredentialType.BasicAuth).Should().Be(1);
        ((int)CredentialType.BearerToken).Should().Be(2);
        ((int)CredentialType.ApiKey).Should().Be(3);
        ((int)CredentialType.Certificate).Should().Be(4);
        ((int)CredentialType.OAuth2).Should().Be(5);
        ((int)CredentialType.DigitalSignature).Should().Be(6);
    }

    // =====================================================
    // Tests de TransmissionStatus Enum
    // =====================================================

    [Fact]
    public void TransmissionStatus_ShouldHave_AllStates()
    {
        TransmissionStatus.Pending.Should().BeDefined();
        TransmissionStatus.InProgress.Should().BeDefined();
        TransmissionStatus.Success.Should().BeDefined();
        TransmissionStatus.Failed.Should().BeDefined();
        TransmissionStatus.Partial.Should().BeDefined();
        TransmissionStatus.Cancelled.Should().BeDefined();
        TransmissionStatus.Retrying.Should().BeDefined();
        TransmissionStatus.Expired.Should().BeDefined();
    }

    [Fact]
    public void TransmissionStatus_ShouldHave_CorrectValues()
    {
        ((int)TransmissionStatus.Pending).Should().Be(1);
        ((int)TransmissionStatus.InProgress).Should().Be(2);
        ((int)TransmissionStatus.Success).Should().Be(3);
        ((int)TransmissionStatus.Failed).Should().Be(4);
    }

    // =====================================================
    // Tests de SyncFrequency Enum
    // =====================================================

    [Fact]
    public void SyncFrequency_ShouldHave_AllOptions()
    {
        SyncFrequency.RealTime.Should().BeDefined();
        SyncFrequency.Hourly.Should().BeDefined();
        SyncFrequency.Daily.Should().BeDefined();
        SyncFrequency.Weekly.Should().BeDefined();
        SyncFrequency.Monthly.Should().BeDefined();
        SyncFrequency.Quarterly.Should().BeDefined();
        SyncFrequency.Yearly.Should().BeDefined();
        SyncFrequency.OnDemand.Should().BeDefined();
    }

    [Fact]
    public void SyncFrequency_ShouldHave_CorrectValues()
    {
        ((int)SyncFrequency.RealTime).Should().Be(1);
        ((int)SyncFrequency.Hourly).Should().Be(2);
        ((int)SyncFrequency.Daily).Should().Be(3);
        ((int)SyncFrequency.Weekly).Should().Be(4);
        ((int)SyncFrequency.Monthly).Should().Be(5);
    }

    // =====================================================
    // Tests de ReportType Enum - Reportes Regulatorios RD
    // =====================================================

    [Fact]
    public void ReportType_ShouldHave_UAFReports()
    {
        // UAF - Ley 155-17
        ReportType.ROS.Should().BeDefined();           // Reporte de Operaciones Sospechosas
        ReportType.DebidaDiligencia.Should().BeDefined();
        ReportType.InformeAnualPLD.Should().BeDefined();
        
        ((int)ReportType.ROS).Should().Be(1);
    }

    [Fact]
    public void ReportType_ShouldHave_DGIIReports()
    {
        // DGII - Ley 11-92
        ReportType.DeclaracionIR.Should().BeDefined();
        ReportType.ReporteITBIS.Should().BeDefined();
        ReportType.FacturaElectronica.Should().BeDefined();
        ReportType.Reporte606.Should().BeDefined();
        ReportType.Reporte607.Should().BeDefined();
        ReportType.Reporte608.Should().BeDefined();
        ReportType.Reporte609.Should().BeDefined();
        
        ((int)ReportType.Reporte606).Should().Be(5);
        ((int)ReportType.Reporte607).Should().Be(6);
    }

    [Fact]
    public void ReportType_ShouldHave_TSSReports()
    {
        // TSS - Seguridad Social
        ReportType.DeclaracionTSS.Should().BeDefined();
        
        ((int)ReportType.DeclaracionTSS).Should().Be(9);
    }

    // =====================================================
    // Tests de LogSeverity Enum
    // =====================================================

    [Fact]
    public void LogSeverity_ShouldHave_AllLevels()
    {
        LogSeverity.Debug.Should().BeDefined();
        LogSeverity.Info.Should().BeDefined();
        LogSeverity.Warning.Should().BeDefined();
        LogSeverity.Error.Should().BeDefined();
        LogSeverity.Critical.Should().BeDefined();
    }

    [Fact]
    public void LogSeverity_ShouldBe_InCorrectOrder()
    {
        // Debug < Info < Warning < Error < Critical
        ((int)LogSeverity.Debug).Should().Be(1);
        ((int)LogSeverity.Info).Should().Be(2);
        ((int)LogSeverity.Warning).Should().Be(3);
        ((int)LogSeverity.Error).Should().Be(4);
        ((int)LogSeverity.Critical).Should().Be(5);
    }

    // =====================================================
    // Tests de DataDirection Enum
    // =====================================================

    [Fact]
    public void DataDirection_ShouldHave_AllDirections()
    {
        DataDirection.Outbound.Should().BeDefined();
        DataDirection.Inbound.Should().BeDefined();
        DataDirection.Bidirectional.Should().BeDefined();
        
        ((int)DataDirection.Outbound).Should().Be(1);
        ((int)DataDirection.Inbound).Should().Be(2);
        ((int)DataDirection.Bidirectional).Should().Be(3);
    }

    // =====================================================
    // Tests de IntegrationConfig Entity
    // =====================================================

    [Fact]
    public void IntegrationConfig_ShouldCreate_ForDGII()
    {
        // Arrange & Act
        var config = new IntegrationConfig
        {
            Name = "DGII Virtual Office API",
            RegulatoryBody = RegulatoryBody.DGII,
            IntegrationType = IntegrationType.ApiRest,
            EndpointUrl = "https://api.dgii.gov.do",
            Status = IntegrationStatus.Active
        };

        // Assert
        config.Should().NotBeNull();
        config.Name.Should().Be("DGII Virtual Office API");
        config.RegulatoryBody.Should().Be(RegulatoryBody.DGII);
        config.IntegrationType.Should().Be(IntegrationType.ApiRest);
    }

    [Fact]
    public void IntegrationConfig_ShouldCreate_ForUAF_SFTP()
    {
        // Arrange & Act - UAF uses SFTP for ROS reports
        var config = new IntegrationConfig
        {
            Name = "UAF SFTP Server",
            RegulatoryBody = RegulatoryBody.UAF,
            IntegrationType = IntegrationType.Sftp,
            EndpointUrl = "sftp://sftp.uaf.gob.do",
            SyncFrequency = SyncFrequency.Monthly
        };

        // Assert
        config.RegulatoryBody.Should().Be(RegulatoryBody.UAF);
        config.IntegrationType.Should().Be(IntegrationType.Sftp);
        config.SyncFrequency.Should().Be(SyncFrequency.Monthly);
    }

    [Fact]
    public void IntegrationConfig_ShouldCreate_ForTSS_WebService()
    {
        // Arrange & Act - TSS uses SOAP Web Services
        var config = new IntegrationConfig
        {
            Name = "TSS Web Service",
            RegulatoryBody = RegulatoryBody.TSS,
            IntegrationType = IntegrationType.WebServiceSoap,
            EndpointUrl = "https://ws.tss.gob.do",
            SyncFrequency = SyncFrequency.Monthly
        };

        // Assert
        config.RegulatoryBody.Should().Be(RegulatoryBody.TSS);
        config.IntegrationType.Should().Be(IntegrationType.WebServiceSoap);
    }

    [Fact]
    public void IntegrationConfig_ShouldDefaultTo_Configuring()
    {
        // Arrange & Act
        var config = new IntegrationConfig();

        // Assert
        config.Status.Should().Be(IntegrationStatus.Configuring);
    }

    [Fact]
    public void IntegrationConfig_ShouldDefaultTo_SandboxMode()
    {
        // Arrange & Act
        var config = new IntegrationConfig();

        // Assert
        config.IsSandboxMode.Should().BeTrue();
    }

    [Fact]
    public void IntegrationConfig_ShouldHave_DefaultTimeoutAndRetries()
    {
        // Arrange & Act
        var config = new IntegrationConfig();

        // Assert
        config.TimeoutSeconds.Should().Be(30);
        config.MaxRetries.Should().Be(3);
        config.RetryIntervalSeconds.Should().Be(60);
    }

    // =====================================================
    // Tests de DataTransmission Entity
    // =====================================================

    [Fact]
    public void DataTransmission_ShouldCreate_ForDGII606()
    {
        // Arrange & Act
        var transmission = new DataTransmission
        {
            IntegrationConfigId = Guid.NewGuid(),
            TransmissionCode = "TX-DGII-606-2026-001",
            ReportType = ReportType.Reporte606,
            Direction = DataDirection.Outbound,
            RecordCount = 150
        };

        // Assert
        transmission.Should().NotBeNull();
        transmission.ReportType.Should().Be(ReportType.Reporte606);
        transmission.Direction.Should().Be(DataDirection.Outbound);
        transmission.Status.Should().Be(TransmissionStatus.Pending);
    }

    [Fact]
    public void DataTransmission_ShouldCreate_ForUAF_ROS()
    {
        // Arrange & Act - ROS Report for UAF
        var transmission = new DataTransmission
        {
            IntegrationConfigId = Guid.NewGuid(),
            TransmissionCode = "TX-UAF-ROS-2026-001",
            ReportType = ReportType.ROS,
            Direction = DataDirection.Outbound
        };

        // Assert
        transmission.ReportType.Should().Be(ReportType.ROS);
        transmission.Direction.Should().Be(DataDirection.Outbound);
    }

    [Fact]
    public void DataTransmission_ShouldDefaultTo_Pending()
    {
        // Arrange & Act
        var transmission = new DataTransmission();

        // Assert
        transmission.Status.Should().Be(TransmissionStatus.Pending);
    }

    [Fact]
    public void DataTransmission_ShouldDefaultTo_Outbound()
    {
        // Arrange & Act
        var transmission = new DataTransmission();

        // Assert
        transmission.Direction.Should().Be(DataDirection.Outbound);
    }

    [Fact]
    public void DataTransmission_ShouldDefaultTo_AttemptCountOne()
    {
        // Arrange & Act
        var transmission = new DataTransmission();

        // Assert
        transmission.AttemptCount.Should().Be(1);
    }

    // =====================================================
    // Tests de IntegrationCredential Entity
    // =====================================================

    [Fact]
    public void IntegrationCredential_ShouldCreate_WithCertificate()
    {
        // Arrange & Act
        var credential = new IntegrationCredential
        {
            IntegrationConfigId = Guid.NewGuid(),
            Name = "DGII e-CF Certificate",
            CredentialType = CredentialType.Certificate,
            CertificateThumbprint = "ABC123DEF456",
            Environment = "Production"
        };

        // Assert
        credential.Should().NotBeNull();
        credential.CredentialType.Should().Be(CredentialType.Certificate);
        credential.Environment.Should().Be("Production");
    }

    [Fact]
    public void IntegrationCredential_ShouldCreate_WithAPIKey()
    {
        // Arrange & Act
        var credential = new IntegrationCredential
        {
            IntegrationConfigId = Guid.NewGuid(),
            Name = "DGII API Key",
            CredentialType = CredentialType.ApiKey,
            Environment = "Sandbox"
        };

        // Assert
        credential.CredentialType.Should().Be(CredentialType.ApiKey);
        credential.Environment.Should().Be("Sandbox");
    }

    [Fact]
    public void IntegrationCredential_ShouldCreate_WithDigitalSignature()
    {
        // Arrange & Act - Ley 339-22 Digital Signature
        var credential = new IntegrationCredential
        {
            IntegrationConfigId = Guid.NewGuid(),
            Name = "Firma Digital RD",
            CredentialType = CredentialType.DigitalSignature,
            CertificateThumbprint = "FirmaDigitalRD2026",
            Environment = "Production"
        };

        // Assert
        credential.CredentialType.Should().Be(CredentialType.DigitalSignature);
    }

    [Fact]
    public void IntegrationCredential_ShouldDefaultTo_Primary()
    {
        // Arrange & Act
        var credential = new IntegrationCredential();

        // Assert
        credential.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void IntegrationCredential_ShouldDefaultTo_Sandbox()
    {
        // Arrange & Act
        var credential = new IntegrationCredential();

        // Assert
        credential.Environment.Should().Be("Sandbox");
    }

    // =====================================================
    // Tests de Scenarios de Integración RD
    // =====================================================

    [Theory]
    [InlineData(RegulatoryBody.DGII, IntegrationType.ApiRest)]
    [InlineData(RegulatoryBody.UAF, IntegrationType.Sftp)]
    [InlineData(RegulatoryBody.TSS, IntegrationType.WebServiceSoap)]
    [InlineData(RegulatoryBody.DGA, IntegrationType.XmlFile)]
    public void RegulatoryIntegrations_ShouldBeConfigurable(
        RegulatoryBody body, 
        IntegrationType integrationType)
    {
        // Arrange & Act
        var config = new IntegrationConfig
        {
            Name = $"{body} Integration",
            RegulatoryBody = body,
            IntegrationType = integrationType,
            EndpointUrl = $"https://{body.ToString().ToLower()}.gob.do"
        };

        // Assert
        config.RegulatoryBody.Should().Be(body);
        config.IntegrationType.Should().Be(integrationType);
    }

    [Theory]
    [InlineData(ReportType.Reporte606)]
    [InlineData(ReportType.Reporte607)]
    [InlineData(ReportType.Reporte608)]
    [InlineData(ReportType.Reporte609)]
    public void DGIIReports_ShouldCoverAllFormats(ReportType reportType)
    {
        // Arrange & Act
        var transmission = new DataTransmission
        {
            IntegrationConfigId = Guid.NewGuid(),
            TransmissionCode = $"TX-{reportType}-2026",
            ReportType = reportType,
            Direction = DataDirection.Outbound
        };

        // Assert
        transmission.ReportType.Should().Be(reportType);
        transmission.Direction.Should().Be(DataDirection.Outbound);
        reportType.ToString().Should().Contain("Reporte");
    }
}
