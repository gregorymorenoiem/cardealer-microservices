// =====================================================
// C6: VehicleRegistryService - Tests Unitarios
// Ley 63-17 Movilidad, Transporte y Tránsito (INTRANT)
// =====================================================

using FluentAssertions;
using VehicleRegistryService.Domain.Entities;
using VehicleRegistryService.Domain.Enums;
using Xunit;

namespace VehicleRegistryService.Tests;

public class VehicleRegistryServiceTests
{
    // =====================================================
    // Tests de Registro de Vehículos
    // =====================================================

    [Fact]
    public void VehicleRegistration_ShouldBeCreated_WithRequiredData()
    {
        // Arrange & Act
        var registration = new VehicleRegistration
        {
            Id = Guid.NewGuid(),
            PlateNumber = "A123456",
            Vin = "1HGCM82633A004352",
            Brand = "Toyota",
            Model = "Corolla",
            Year = 2024,
            Color = "Blanco",
            EngineNumber = "2ZR1234567",
            ChassisNumber = "JT123456789012345",
            OwnerIdentification = "00112345678",
            OwnerName = "Juan Pérez",
            RegistrationDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddYears(1),
            Status = RegistrationStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        registration.Should().NotBeNull();
        registration.PlateNumber.Should().NotBeNullOrEmpty();
        registration.Vin.Should().HaveLength(17); // VIN estándar
        registration.Status.Should().Be(RegistrationStatus.Active);
    }

    // =====================================================
    // Tests de Formato de Placa Dominicana
    // =====================================================

    [Theory]
    [InlineData("A123456", true)]   // Placa privada válida
    [InlineData("G123456", true)]   // Placa gobierno
    [InlineData("X123456", true)]   // Placa exonerada
    [InlineData("L123456", true)]   // Placa alquiler
    [InlineData("P123456", true)]   // Placa pública
    [InlineData("AB12345", false)]  // Formato inválido
    [InlineData("1234567", false)]  // Solo números
    [InlineData("ABCDEFG", false)]  // Solo letras
    public void PlateNumber_ShouldFollowDominicanFormat(string plate, bool expectedValid)
    {
        // Act
        var isValid = ValidatePlateFormat(plate);

        // Assert
        isValid.Should().Be(expectedValid);
    }

    private bool ValidatePlateFormat(string plate)
    {
        if (string.IsNullOrEmpty(plate) || plate.Length != 7) return false;
        var validPrefixes = new[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        return validPrefixes.Contains(plate[0]) && plate.Substring(1).All(char.IsDigit);
    }

    // =====================================================
    // Tests de Validación VIN (17 caracteres)
    // =====================================================

    [Theory]
    [InlineData("1HGCM82633A004352", true)]   // VIN válido (17 caracteres)
    [InlineData("JT123456789012345", true)]   // VIN japonés
    [InlineData("WDB1234567F123456", true)]   // VIN europeo
    [InlineData("1234567890123456", false)]   // 16 caracteres
    [InlineData("1HGCM82633A0043521", false)] // 18 caracteres
    [InlineData("", false)]                    // Vacío
    public void Vin_ShouldBe17Characters(string vin, bool expectedValid)
    {
        // Act
        var isValid = !string.IsNullOrEmpty(vin) && vin.Length == 17;

        // Assert
        isValid.Should().Be(expectedValid);
    }

    // =====================================================
    // Tests de Transferencia de Propiedad
    // =====================================================

    [Fact]
    public void OwnershipTransfer_ShouldBeCreated_WithRequiredFields()
    {
        // Arrange & Act
        var transfer = new OwnershipTransfer
        {
            Id = Guid.NewGuid(),
            VehicleRegistrationId = Guid.NewGuid(),
            PreviousOwnerIdentification = "00112345678",
            PreviousOwnerName = "Juan Pérez",
            NewOwnerIdentification = "00198765432",
            NewOwnerName = "María García",
            TransferDate = DateTime.UtcNow,
            TransferType = TransferType.Sale,
            NotaryDocument = "DOC-2026-00001",
            Status = TransferStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        transfer.Should().NotBeNull();
        transfer.PreviousOwnerIdentification.Should().NotBe(transfer.NewOwnerIdentification);
        transfer.NotaryDocument.Should().NotBeNullOrEmpty();
        transfer.Status.Should().Be(TransferStatus.Pending);
    }

    [Theory]
    [InlineData(TransferType.Sale)]
    [InlineData(TransferType.Donation)]
    [InlineData(TransferType.Inheritance)]
    [InlineData(TransferType.CourtOrder)]
    public void TransferType_ShouldHaveExpectedValues(TransferType type)
    {
        // Assert
        Enum.IsDefined(typeof(TransferType), type).Should().BeTrue();
    }

    // =====================================================
    // Tests de Marbete (Sticker de Circulación)
    // =====================================================

    [Fact]
    public void Marbete_ShouldBeCreated_WithValidPeriod()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;

        // Act
        var marbete = new Marbete
        {
            Id = Guid.NewGuid(),
            VehicleRegistrationId = Guid.NewGuid(),
            MarbeteNumber = $"M-{currentYear}-00001",
            Year = currentYear,
            IssueDate = DateTime.UtcNow,
            ExpirationDate = new DateTime(currentYear, 12, 31),
            Amount = 3000m, // RD$3,000 típico
            Status = MarbeteStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        marbete.Year.Should().Be(currentYear);
        marbete.ExpirationDate.Year.Should().Be(currentYear);
        marbete.Amount.Should().BePositive();
    }

    [Theory]
    [InlineData(MarbeteStatus.Active)]
    [InlineData(MarbeteStatus.Expired)]
    [InlineData(MarbeteStatus.Cancelled)]
    [InlineData(MarbeteStatus.Pending)]
    public void MarbeteStatus_ShouldHaveExpectedValues(MarbeteStatus status)
    {
        // Assert
        Enum.IsDefined(typeof(MarbeteStatus), status).Should().BeTrue();
    }

    // =====================================================
    // Tests de Inspección Técnica Vehicular
    // =====================================================

    [Fact]
    public void VehicleInspection_ShouldBeCreated_WithResults()
    {
        // Arrange & Act
        var inspection = new VehicleInspection
        {
            Id = Guid.NewGuid(),
            VehicleRegistrationId = Guid.NewGuid(),
            InspectionNumber = "INSP-2026-00001",
            InspectionDate = DateTime.UtcNow,
            InspectionCenter = "Centro de Inspección Santo Domingo",
            InspectorName = "Carlos Rodríguez",
            BrakesResult = InspectionResult.Pass,
            LightsResult = InspectionResult.Pass,
            EmissionsResult = InspectionResult.Pass,
            TiresResult = InspectionResult.Pass,
            OverallResult = InspectionResult.Pass,
            ValidUntil = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        inspection.OverallResult.Should().Be(InspectionResult.Pass);
        inspection.ValidUntil.Should().BeAfter(inspection.InspectionDate);
    }

    [Theory]
    [InlineData(InspectionResult.Pass)]
    [InlineData(InspectionResult.Fail)]
    [InlineData(InspectionResult.Conditional)]
    public void InspectionResult_ShouldHaveExpectedValues(InspectionResult result)
    {
        // Assert
        Enum.IsDefined(typeof(InspectionResult), result).Should().BeTrue();
    }

    // =====================================================
    // Tests de Multas y Sanciones
    // =====================================================

    [Fact]
    public void TrafficViolation_ShouldBeCreated_WithDetails()
    {
        // Arrange & Act
        var violation = new TrafficViolation
        {
            Id = Guid.NewGuid(),
            VehicleRegistrationId = Guid.NewGuid(),
            ViolationNumber = "MUL-2026-00001",
            ViolationType = ViolationType.Speeding,
            ViolationDate = DateTime.UtcNow,
            Location = "Autopista Duarte Km 25",
            FineAmount = 5000m,
            Status = ViolationStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        violation.ViolationNumber.Should().StartWith("MUL-");
        violation.FineAmount.Should().BePositive();
        violation.DueDate.Should().BeAfter(violation.ViolationDate);
    }

    [Theory]
    [InlineData(ViolationType.Speeding)]
    [InlineData(ViolationType.RedLight)]
    [InlineData(ViolationType.NoLicense)]
    [InlineData(ViolationType.NoInsurance)]
    [InlineData(ViolationType.ExpiredMarbete)]
    [InlineData(ViolationType.IllegalParking)]
    public void ViolationType_ShouldHaveExpectedValues(ViolationType type)
    {
        // Assert
        Enum.IsDefined(typeof(ViolationType), type).Should().BeTrue();
    }

    // =====================================================
    // Tests de Seguro Obligatorio
    // =====================================================

    [Fact]
    public void VehicleInsurance_ShouldBeValid_ForRegistration()
    {
        // Arrange & Act
        var insurance = new VehicleInsurance
        {
            Id = Guid.NewGuid(),
            VehicleRegistrationId = Guid.NewGuid(),
            PolicyNumber = "POL-2026-00001",
            InsuranceCompany = "Seguros Universal",
            CoverageType = CoverageType.ThirdParty,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            PremiumAmount = 15000m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        insurance.IsActive.Should().BeTrue();
        insurance.EndDate.Should().BeAfter(insurance.StartDate);
        insurance.CoverageType.Should().NotBe(CoverageType.None);
    }

    [Theory]
    [InlineData(CoverageType.ThirdParty)]      // Responsabilidad civil (obligatorio)
    [InlineData(CoverageType.Comprehensive)]   // Todo riesgo
    [InlineData(CoverageType.Collision)]       // Colisión
    public void CoverageType_ShouldHaveExpectedValues(CoverageType type)
    {
        // Assert
        Enum.IsDefined(typeof(CoverageType), type).Should().BeTrue();
    }
}
