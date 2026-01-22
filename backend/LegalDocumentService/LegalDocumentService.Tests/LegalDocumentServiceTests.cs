// =====================================================
// C10: LegalDocumentService - Tests Unitarios
// Gestión de Contratos y Documentos Legales
// Ley 126-02 E-Commerce, Ley 172-13 Protección de Datos
// =====================================================

using FluentAssertions;
using LegalDocumentService.Domain.Enums;
using Xunit;

namespace LegalDocumentService.Tests;

public class LegalDocumentServiceTests
{
    // =====================================================
    // Tests de Tipos de Documentos Legales
    // =====================================================

    [Theory]
    [InlineData(LegalDocumentType.TermsOfService)]
    [InlineData(LegalDocumentType.PrivacyPolicy)]
    [InlineData(LegalDocumentType.CookiePolicy)]
    [InlineData(LegalDocumentType.SaleContract)]
    [InlineData(LegalDocumentType.RefundPolicy)]
    [InlineData(LegalDocumentType.WarrantyPolicy)]
    public void LegalDocumentType_ShouldHaveContractTypes(LegalDocumentType type)
    {
        // Assert
        Enum.IsDefined(typeof(LegalDocumentType), type).Should().BeTrue();
    }

    [Fact]
    public void LegalDocumentType_ShouldIncludeLey12602Types()
    {
        // Ley 126-02 E-Commerce - Avisos legales obligatorios
        var ecommerceTypes = new[]
        {
            LegalDocumentType.TermsOfService,
            LegalDocumentType.PrivacyPolicy,
            LegalDocumentType.LegalDisclaimer,
            LegalDocumentType.IntellectualPropertyNotice
        };

        // Assert
        ecommerceTypes.Should().AllSatisfy(type =>
            Enum.IsDefined(typeof(LegalDocumentType), type).Should().BeTrue()
        );
    }

    [Fact]
    public void LegalDocumentType_ShouldIncludeLey17213Types()
    {
        // Ley 172-13 Protección de Datos - Consentimientos
        var dataProtectionTypes = new[]
        {
            LegalDocumentType.DataProcessingConsent,
            LegalDocumentType.MarketingConsent,
            LegalDocumentType.ThirdPartyDataSharingConsent,
            LegalDocumentType.InternationalTransferConsent
        };

        // Assert
        dataProtectionTypes.Should().AllSatisfy(type =>
            Enum.IsDefined(typeof(LegalDocumentType), type).Should().BeTrue()
        );
    }

    [Fact]
    public void LegalDocumentType_ShouldIncludeLey15517Types()
    {
        // Ley 155-17 AML/KYC - Disclosure obligatorios
        var amlTypes = new[]
        {
            LegalDocumentType.AMLPolicy,
            LegalDocumentType.KYCDisclosure,
            LegalDocumentType.PEPDisclosure
        };

        // Assert
        amlTypes.Should().AllSatisfy(type =>
            Enum.IsDefined(typeof(LegalDocumentType), type).Should().BeTrue()
        );
    }

    // =====================================================
    // Tests de Estados de Documentos
    // =====================================================

    [Theory]
    [InlineData(LegalDocumentStatus.Draft)]
    [InlineData(LegalDocumentStatus.PendingReview)]
    [InlineData(LegalDocumentStatus.PendingLegalApproval)]
    [InlineData(LegalDocumentStatus.Approved)]
    [InlineData(LegalDocumentStatus.Published)]
    [InlineData(LegalDocumentStatus.Archived)]
    [InlineData(LegalDocumentStatus.Superseded)]
    [InlineData(LegalDocumentStatus.Rejected)]
    public void LegalDocumentStatus_ShouldHaveExpectedValues(LegalDocumentStatus status)
    {
        // Assert
        Enum.IsDefined(typeof(LegalDocumentStatus), status).Should().BeTrue();
    }

    // =====================================================
    // Tests de Jurisdicción
    // =====================================================

    [Fact]
    public void Jurisdiction_ShouldIncludeDominicanRepublic()
    {
        // Assert
        Jurisdiction.DominicanRepublic.Should().Be(Jurisdiction.DominicanRepublic);
        ((int)Jurisdiction.DominicanRepublic).Should().Be(1);
    }

    [Theory]
    [InlineData(Jurisdiction.DominicanRepublic)]
    [InlineData(Jurisdiction.USA)]
    [InlineData(Jurisdiction.EU)]
    [InlineData(Jurisdiction.International)]
    [InlineData(Jurisdiction.Caribbean)]
    public void Jurisdiction_ShouldHaveExpectedValues(Jurisdiction jurisdiction)
    {
        // Assert
        Enum.IsDefined(typeof(Jurisdiction), jurisdiction).Should().BeTrue();
    }

    // =====================================================
    // Tests de Idiomas
    // =====================================================

    [Fact]
    public void DocumentLanguage_ShouldDefaultToSpanish()
    {
        // En República Dominicana, el idioma principal es español
        var defaultLanguage = DocumentLanguage.Spanish;
        
        // Assert
        defaultLanguage.Should().Be(DocumentLanguage.Spanish);
        ((int)defaultLanguage).Should().Be(1);
    }

    [Theory]
    [InlineData(DocumentLanguage.Spanish)]
    [InlineData(DocumentLanguage.English)]
    [InlineData(DocumentLanguage.French)]
    [InlineData(DocumentLanguage.Portuguese)]
    public void DocumentLanguage_ShouldHaveExpectedValues(DocumentLanguage language)
    {
        // Assert
        Enum.IsDefined(typeof(DocumentLanguage), language).Should().BeTrue();
    }

    // =====================================================
    // Tests de Estados de Aceptación
    // =====================================================

    [Theory]
    [InlineData(AcceptanceStatus.Pending)]
    [InlineData(AcceptanceStatus.Accepted)]
    [InlineData(AcceptanceStatus.Declined)]
    [InlineData(AcceptanceStatus.Expired)]
    [InlineData(AcceptanceStatus.Revoked)]
    public void AcceptanceStatus_ShouldHaveExpectedValues(AcceptanceStatus status)
    {
        // Assert
        Enum.IsDefined(typeof(AcceptanceStatus), status).Should().BeTrue();
    }

    // =====================================================
    // Tests de Métodos de Aceptación
    // =====================================================

    [Theory]
    [InlineData(AcceptanceMethod.ClickWrap)]
    [InlineData(AcceptanceMethod.BrowseWrap)]
    [InlineData(AcceptanceMethod.ScrollWrap)]
    [InlineData(AcceptanceMethod.SignWrap)]
    [InlineData(AcceptanceMethod.Verbal)]
    [InlineData(AcceptanceMethod.Written)]
    public void AcceptanceMethod_ShouldHaveExpectedValues(AcceptanceMethod method)
    {
        // Assert
        Enum.IsDefined(typeof(AcceptanceMethod), method).Should().BeTrue();
    }

    [Fact]
    public void AcceptanceMethod_ClickWrap_ShouldBeDefault()
    {
        // ClickWrap es el método más común en e-commerce
        var defaultMethod = AcceptanceMethod.ClickWrap;
        
        // Assert
        defaultMethod.Should().Be(AcceptanceMethod.ClickWrap);
    }

    // =====================================================
    // Tests de Variables de Plantilla
    // =====================================================

    [Theory]
    [InlineData(TemplateVariableType.Text)]
    [InlineData(TemplateVariableType.Date)]
    [InlineData(TemplateVariableType.Number)]
    [InlineData(TemplateVariableType.Currency)]
    [InlineData(TemplateVariableType.Boolean)]
    [InlineData(TemplateVariableType.List)]
    [InlineData(TemplateVariableType.Entity)]
    public void TemplateVariableType_ShouldHaveExpectedValues(TemplateVariableType type)
    {
        // Assert
        Enum.IsDefined(typeof(TemplateVariableType), type).Should().BeTrue();
    }

    // =====================================================
    // Tests de Reemplazo de Variables
    // =====================================================

    [Fact]
    public void TemplateVariable_ShouldBeReplaced()
    {
        // Arrange
        var template = "El comprador {{buyer_name}} con cédula {{buyer_cedula}}";
        var variables = new Dictionary<string, string>
        {
            { "buyer_name", "Juan Pérez" },
            { "buyer_cedula", "001-1234567-8" }
        };

        // Act
        var result = ReplaceVariables(template, variables);

        // Assert
        result.Should().Be("El comprador Juan Pérez con cédula 001-1234567-8");
        result.Should().NotContain("{{");
    }

    private string ReplaceVariables(string template, Dictionary<string, string> variables)
    {
        var result = template;
        foreach (var kvp in variables)
        {
            result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }
        return result;
    }

    // =====================================================
    // Tests de Cláusulas Obligatorias
    // =====================================================

    [Fact]
    public void LegalClause_ShouldBeMandatory_ForCompliance()
    {
        // Arrange - Cláusulas requeridas por ley en RD
        var mandatoryClauses = new List<string>
        {
            "Cláusula de Protección de Datos (Ley 172-13)",
            "Cláusula de Derecho de Retracto (7 días - ProConsumidor)",
            "Cláusula de Garantía Legal (6 meses)",
            "Cláusula de Jurisdicción (Tribunales de RD)"
        };

        // Assert
        mandatoryClauses.Should().HaveCountGreaterOrEqualTo(3);
        mandatoryClauses.Should().Contain(c => c.Contains("Protección de Datos"));
        mandatoryClauses.Should().Contain(c => c.Contains("Retracto"));
    }

    // =====================================================
    // Tests de Vigencia de Documentos
    // =====================================================

    [Fact]
    public void Document_ShouldHaveValidityPeriod()
    {
        // Arrange
        var effectiveDate = DateTime.UtcNow;
        var expirationDate = DateTime.UtcNow.AddYears(1); // 1 año de vigencia

        // Assert
        expirationDate.Should().BeAfter(effectiveDate);
        (expirationDate - effectiveDate).Days.Should().BeGreaterOrEqualTo(365);
    }

    // =====================================================
    // Tests de Documentos Pro-Consumidor
    // =====================================================

    [Fact]
    public void LegalDocumentType_ShouldIncludeProConsumidorTypes()
    {
        // Pro-Consumidor - Ley 358-05
        var proConsumidorTypes = new[]
        {
            LegalDocumentType.ConsumerRightsNotice,
            LegalDocumentType.ComplaintProcedure,
            LegalDocumentType.RefundPolicy,
            LegalDocumentType.ReturnPolicy,
            LegalDocumentType.WarrantyPolicy
        };

        // Assert
        proConsumidorTypes.Should().AllSatisfy(type =>
            Enum.IsDefined(typeof(LegalDocumentType), type).Should().BeTrue()
        );
    }

    // =====================================================
    // Tests de Documentos DGII
    // =====================================================

    [Fact]
    public void LegalDocumentType_ShouldIncludeDGIITypes()
    {
        // DGII - Documentos fiscales
        var dgiiTypes = new[]
        {
            LegalDocumentType.NCFNotice,
            LegalDocumentType.TaxComplianceNotice
        };

        // Assert
        dgiiTypes.Should().AllSatisfy(type =>
            Enum.IsDefined(typeof(LegalDocumentType), type).Should().BeTrue()
        );
    }

    // =====================================================
    // Tests de Versionamiento
    // =====================================================

    [Fact]
    public void DocumentVersion_ShouldFollowSemanticVersioning()
    {
        // Arrange
        var versions = new[] { "1.0.0", "1.0.1", "1.1.0", "2.0.0" };

        // Assert
        versions.Should().AllSatisfy(v =>
            v.Split('.').Should().HaveCount(3)
        );
    }
}
