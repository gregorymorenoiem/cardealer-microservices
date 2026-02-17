using KYCService.Domain.Validators;

namespace KYCService.Tests.Validators;

/// <summary>
/// Tests unitarios para el validador de cédulas dominicanas
/// Verifica el algoritmo Módulo 10 y validaciones de formato JCE
/// </summary>
public class CedulaValidatorTests
{
    #region Validación de Formato

    [Theory]
    [InlineData("001-1234567-8", true)]
    [InlineData("0011234567-8", true)]
    [InlineData("00112345678", true)]
    [InlineData("001 1234567 8", true)]
    [InlineData("001.1234567.8", true)]
    public void CleanCedula_ShouldRemoveNonNumericCharacters(string input, bool _)
    {
        // Act
        var result = CedulaValidator.CleanCedula(input);

        // Assert
        result.Should().HaveLength(11);
        result.All(char.IsDigit).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void ValidateCedula_ShouldReturnError_WhenCedulaIsEmpty(string? cedula)
    {
        // Act
        var (isValid, error) = CedulaValidator.ValidateCedula(cedula!);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("requerida");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    public void ValidateCedula_ShouldReturnError_WhenLengthIsInvalid(string cedula)
    {
        // Act
        var (isValid, error) = CedulaValidator.ValidateCedula(cedula);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("11 dígitos");
    }

    [Theory]
    [InlineData("ABC-1234567-8", "11 dígitos")]  // After cleaning: "12345678" - only 8 digits
    [InlineData("001-123ABCD-8", "11 dígitos")]  // After cleaning: "0011238" - only 7 digits
    [InlineData("12345", "11 dígitos")]          // Too short
    public void ValidateCedula_ShouldReturnError_WhenContainsNonNumeric(string cedula, string expectedError)
    {
        // Note: CleanCedula removes all non-numeric characters first,
        // so the "solo números" error is only triggered if somehow
        // non-digits remain. This tests the length validation after cleaning.
        
        // Act
        var (isValid, error) = CedulaValidator.ValidateCedula(cedula);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain(expectedError);
    }

    #endregion

    #region Validación de Municipio

    [Theory]
    [InlineData("001")] // Distrito Nacional
    [InlineData("025")] // Santiago
    [InlineData("032")] // Santo Domingo
    [InlineData("037")] // Santo Domingo Este
    public void ValidateCedula_ShouldAccept_ValidMunicipioCodes(string municipio)
    {
        // Arrange
        var cedula = GenerateValidCedulaForMunicipio(int.Parse(municipio));

        // Act
        var (isValid, error) = CedulaValidator.ValidateCedula(cedula);

        // Assert
        isValid.Should().BeTrue($"Municipio {municipio} debería ser válido");
        error.Should().BeNull();
    }

    [Theory]
    [InlineData("000")] // No existe
    [InlineData("050")] // No existe
    [InlineData("099")] // No existe
    [InlineData("100")] // No existe
    public void ValidateCedula_ShouldReject_InvalidMunicipioCodes(string municipio)
    {
        // Arrange - Crear cédula con municipio inválido
        var cedula = $"{municipio}12345678";

        // Act
        var (isValid, error) = CedulaValidator.ValidateCedula(cedula);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("municipio inválido");
    }

    #endregion

    #region Validación de Dígito Verificador (Módulo 10)

    [Theory]
    [InlineData("00112345673")] // 0011234567 → check digit = 3
    [InlineData("00100000009")] // 0010000000 → check digit = 9
    [InlineData("02500000001")] // Santiago → check digit = 1
    [InlineData("03200000002")] // Santo Domingo → check digit = 2
    public void ValidateCedula_ShouldAccept_CorrectCheckDigit(string cedula)
    {
        // Act
        var result = CedulaValidator.ValidateDetailed(cedula);

        // Assert
        result.ChecksumValid.Should().BeTrue(
            $"Dígito verificador de {cedula} debería ser válido. " +
            $"Esperado: {result.ExpectedCheckDigit}, Actual: {result.ActualCheckDigit}");
    }

    [Theory]
    [InlineData("00112345670")] // Debería ser 3, no 0
    [InlineData("00112345671")] // Debería ser 3, no 1
    [InlineData("00112345679")] // Debería ser 3, no 9
    public void ValidateCedula_ShouldReject_IncorrectCheckDigit(string cedula)
    {
        // Act
        var result = CedulaValidator.ValidateDetailed(cedula);

        // Assert
        result.ChecksumValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("verificador"));
    }

    [Fact]
    public void ValidateDetailed_ShouldProvideAllValidationInfo()
    {
        // Arrange - Using cedula with correct check digit (3)
        var cedula = "001-1234567-3";

        // Act
        var result = CedulaValidator.ValidateDetailed(cedula);

        // Assert
        result.CleanedNumber.Should().Be("00112345673");
        result.FormattedNumber.Should().Be("001-1234567-3");
        result.Municipio.Should().Be(1);
        result.FormatValid.Should().BeTrue();
        result.MunicipioValid.Should().BeTrue();
        result.ChecksumValid.Should().BeTrue();
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Formateo de Cédula

    [Theory]
    [InlineData("00112345673", "001-1234567-3")]
    [InlineData("001-1234567-3", "001-1234567-3")]
    [InlineData("001 1234567 3", "001-1234567-3")]
    [InlineData("001.1234567.3", "001-1234567-3")]
    public void FormatCedula_ShouldReturnStandardFormat(string input, string expected)
    {
        // Act
        var result = CedulaValidator.FormatCedula(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567890123")]
    [InlineData("")]
    public void FormatCedula_ShouldReturnOriginal_WhenInvalidLength(string input)
    {
        // Act
        var result = CedulaValidator.FormatCedula(input);

        // Assert
        result.Should().Be(input);
    }

    #endregion

    #region Generación de Cédula de Prueba

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(32)]
    public void GenerateTestCedula_ShouldGenerateValidCedula(int municipio)
    {
        // Act
        var cedula = CedulaValidator.GenerateTestCedula(municipio);

        // Assert
        var result = CedulaValidator.ValidateDetailed(cedula);
        result.IsValid.Should().BeTrue($"Cédula generada {cedula} debería ser válida");
        result.Municipio.Should().Be(municipio);
    }

    [Fact]
    public void GenerateTestCedula_ShouldGenerateDifferentCedulas()
    {
        // Act
        var cedulas = Enumerable.Range(0, 10)
            .Select(_ => CedulaValidator.GenerateTestCedula())
            .ToList();

        // Assert
        cedulas.Distinct().Count().Should().BeGreaterThan(1, 
            "Debería generar cédulas diferentes");
    }

    #endregion

    #region Validación de Edad

    [Fact]
    public void ValidateAge_ShouldPass_WhenAgeIsValid()
    {
        // Arrange
        var dateOfBirth = DateTime.Today.AddYears(-25);

        // Act
        var (isValid, age, error) = CedulaValidator.ValidateAge(dateOfBirth);

        // Assert
        isValid.Should().BeTrue();
        age.Should().Be(25);
        error.Should().BeNull();
    }

    [Fact]
    public void ValidateAge_ShouldFail_WhenUnderMinimumAge()
    {
        // Arrange
        var dateOfBirth = DateTime.Today.AddYears(-17);

        // Act
        var (isValid, age, error) = CedulaValidator.ValidateAge(dateOfBirth, minimumAge: 18);

        // Assert
        isValid.Should().BeFalse();
        age.Should().Be(17);
        error.Should().Contain("18 años");
    }

    [Fact]
    public void ValidateAge_ShouldFail_WhenAgeIsUnrealistic()
    {
        // Arrange
        var dateOfBirth = DateTime.Today.AddYears(-150);

        // Act
        var (isValid, _, error) = CedulaValidator.ValidateAge(dateOfBirth);

        // Assert
        isValid.Should().BeFalse();
        error.Should().Contain("no es válida");
    }

    [Fact]
    public void ValidateAge_ShouldCalculateCorrectly_WhenBirthdayNotYetOccurred()
    {
        // Arrange - Fecha de nacimiento que aún no ha ocurrido este año
        var today = DateTime.Today;
        var futureMonthDay = today.AddMonths(1);
        var dateOfBirth = new DateTime(today.Year - 20, futureMonthDay.Month, Math.Min(futureMonthDay.Day, 28));

        // Act
        var (isValid, age, _) = CedulaValidator.ValidateAge(dateOfBirth);

        // Assert
        isValid.Should().BeTrue();
        age.Should().Be(19); // Aún no cumple 20
    }

    #endregion

    #region Casos de Cédulas Reales (Formato Válido)

    [Theory]
    [InlineData("001-0000000-0")] // Formato mínimo válido Distrito Nacional
    [InlineData("032-0000000-2")] // Santo Domingo
    [InlineData("025-0000000-4")] // Santiago
    public void ValidateCedula_ShouldHandle_RealWorldFormats(string cedula)
    {
        // Act
        var result = CedulaValidator.ValidateDetailed(cedula);

        // Assert
        result.FormatValid.Should().BeTrue();
        result.MunicipioValid.Should().BeTrue();
        // El checksum puede o no ser válido dependiendo del número
    }

    #endregion

    #region Helpers

    private static string GenerateValidCedulaForMunicipio(int municipio)
    {
        return CedulaValidator.GenerateTestCedula(municipio);
    }

    #endregion
}

/// <summary>
/// Tests para el resultado detallado de validación
/// </summary>
public class CedulaValidationResultTests
{
    [Fact]
    public void CedulaValidationResult_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var result = new CedulaValidationResult();

        // Assert
        result.IsValid.Should().BeFalse();
        result.FormatValid.Should().BeFalse();
        result.MunicipioValid.Should().BeFalse();
        result.ChecksumValid.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.CleanedNumber.Should().BeEmpty();
        result.FormattedNumber.Should().BeEmpty();
    }

    [Fact]
    public void CedulaValidationResult_ShouldAccumulateErrors()
    {
        // Arrange
        var result = new CedulaValidationResult();

        // Act
        result.Errors.Add("Error 1");
        result.Errors.Add("Error 2");

        // Assert
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Error 1");
        result.Errors.Should().Contain("Error 2");
    }
}
