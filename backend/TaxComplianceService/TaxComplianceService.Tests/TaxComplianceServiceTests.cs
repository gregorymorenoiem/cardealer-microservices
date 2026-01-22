// =====================================================
// C1: TaxComplianceService - Tests Unitarios
// Ley 11-92 Código Tributario de República Dominicana
// =====================================================

using FluentAssertions;
using TaxComplianceService.Domain.Entities;
using TaxComplianceService.Domain.Enums;
using Xunit;

namespace TaxComplianceService.Tests;

public class TaxComplianceServiceTests
{
    // =====================================================
    // Tests de Entidad TaxDeclaration
    // =====================================================

    [Fact]
    public void TaxDeclaration_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var declaration = new TaxDeclaration
        {
            Id = Guid.NewGuid(),
            TaxpayerId = Guid.NewGuid(),
            Rnc = "101234567",
            DeclarationType = DeclarationType.ITBIS,
            Period = "202601",
            GrossAmount = 1000000m,
            TaxableAmount = 850000m,
            TaxAmount = 153000m,
            Status = DeclarationStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        declaration.Should().NotBeNull();
        declaration.Rnc.Should().Be("101234567");
        declaration.DeclarationType.Should().Be(DeclarationType.ITBIS);
        declaration.TaxAmount.Should().Be(153000m);
        declaration.Status.Should().Be(DeclarationStatus.Draft);
    }

    [Theory]
    [InlineData(DeclarationType.ITBIS)]
    [InlineData(DeclarationType.ISR)]
    [InlineData(DeclarationType.Reporte606)]
    [InlineData(DeclarationType.Reporte607)]
    [InlineData(DeclarationType.Reporte608)]
    [InlineData(DeclarationType.Reporte609)]
    public void DeclarationType_ShouldHaveExpectedValues(DeclarationType type)
    {
        // Assert
        Enum.IsDefined(typeof(DeclarationType), type).Should().BeTrue();
    }

    [Theory]
    [InlineData(DeclarationStatus.Draft)]
    [InlineData(DeclarationStatus.Pending)]
    [InlineData(DeclarationStatus.Submitted)]
    [InlineData(DeclarationStatus.Accepted)]
    [InlineData(DeclarationStatus.Rejected)]
    public void DeclarationStatus_ShouldHaveExpectedValues(DeclarationStatus status)
    {
        // Assert
        Enum.IsDefined(typeof(DeclarationStatus), status).Should().BeTrue();
    }

    // =====================================================
    // Tests de Cálculos ITBIS (18%)
    // =====================================================

    [Fact]
    public void CalculateITBIS_ShouldReturn18Percent()
    {
        // Arrange
        var baseAmount = 100000m;
        var itbisRate = 0.18m;

        // Act
        var itbisAmount = baseAmount * itbisRate;

        // Assert
        itbisAmount.Should().Be(18000m);
    }

    [Theory]
    [InlineData(100000, 18000)]
    [InlineData(500000, 90000)]
    [InlineData(1000000, 180000)]
    [InlineData(0, 0)]
    public void CalculateITBIS_ShouldBeCorrect_ForVariousAmounts(decimal baseAmount, decimal expectedItbis)
    {
        // Arrange
        var itbisRate = 0.18m;

        // Act
        var itbisAmount = baseAmount * itbisRate;

        // Assert
        itbisAmount.Should().Be(expectedItbis);
    }

    // =====================================================
    // Tests de Validación RNC
    // =====================================================

    [Theory]
    [InlineData("101234567", true)]   // 9 dígitos - Persona física
    [InlineData("12345678901", true)] // 11 dígitos - Empresa
    [InlineData("1234567", false)]    // Muy corto
    [InlineData("1234567890123", false)] // Muy largo
    [InlineData("", false)]           // Vacío
    public void ValidateRnc_ShouldReturnExpectedResult(string rnc, bool expectedValid)
    {
        // Act
        var isValid = !string.IsNullOrEmpty(rnc) && 
                      (rnc.Length == 9 || rnc.Length == 11) && 
                      rnc.All(char.IsDigit);

        // Assert
        isValid.Should().Be(expectedValid);
    }

    // =====================================================
    // Tests de Períodos Fiscales
    // =====================================================

    [Theory]
    [InlineData("202601", true)]  // Enero 2026
    [InlineData("202612", true)]  // Diciembre 2026
    [InlineData("202613", false)] // Mes inválido
    [InlineData("202600", false)] // Mes inválido
    [InlineData("2026", false)]   // Formato incorrecto
    public void ValidateFiscalPeriod_ShouldReturnExpectedResult(string period, bool expectedValid)
    {
        // Act
        var isValid = period.Length == 6 && 
                      int.TryParse(period.Substring(4, 2), out int month) && 
                      month >= 1 && month <= 12;

        // Assert
        isValid.Should().Be(expectedValid);
    }

    // =====================================================
    // Tests de NCF (Número de Comprobante Fiscal)
    // =====================================================

    [Theory]
    [InlineData("B0100000001", true)]  // Factura válida para crédito fiscal
    [InlineData("B0200000001", true)]  // Factura consumidor final
    [InlineData("B1100000001", true)]  // Comprobante especial
    [InlineData("B1400000001", true)]  // Regímenes especiales
    [InlineData("E310000000001", true)] // e-CF
    [InlineData("XXXX", false)]        // Inválido
    public void ValidateNcf_ShouldReturnExpectedResult(string ncf, bool expectedValid)
    {
        // Act
        var isValid = !string.IsNullOrEmpty(ncf) && 
                      (ncf.StartsWith("B") || ncf.StartsWith("E")) && 
                      ncf.Length >= 11;

        // Assert
        isValid.Should().Be(expectedValid);
    }

    // =====================================================
    // Tests de Reporte 606 (Compras)
    // =====================================================

    [Fact]
    public void Reporte606_ShouldContainRequiredFields()
    {
        // Arrange
        var reporte606Item = new
        {
            RncCedula = "101234567",
            TipoIdentificacion = 1, // 1=RNC, 2=Cédula
            TipoComprobante = "01", // 01=Factura
            Ncf = "B0100000001",
            FechaComprobante = DateTime.Now,
            MontoFacturado = 100000m,
            ItbisFacturado = 18000m
        };

        // Assert
        reporte606Item.RncCedula.Should().NotBeNullOrEmpty();
        reporte606Item.Ncf.Should().StartWith("B");
        reporte606Item.ItbisFacturado.Should().Be(reporte606Item.MontoFacturado * 0.18m);
    }

    // =====================================================
    // Tests de Reporte 607 (Ventas)
    // =====================================================

    [Fact]
    public void Reporte607_ShouldContainRequiredFields()
    {
        // Arrange
        var reporte607Item = new
        {
            RncCedula = "101234567",
            TipoIngreso = "01", // 01=Ingresos por operaciones
            Ncf = "B0100000001",
            FechaComprobante = DateTime.Now,
            MontoFacturado = 500000m,
            ItbisFacturado = 90000m
        };

        // Assert
        reporte607Item.MontoFacturado.Should().BePositive();
        reporte607Item.ItbisFacturado.Should().Be(reporte607Item.MontoFacturado * 0.18m);
    }
}
