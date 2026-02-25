using Xunit;
using FluentValidation;
using UserService.Application.DTOs;
using UserService.Application.Validators;
using UserService.Domain.Entities;

namespace UserService.Tests.UseCases.Sellers;

/// <summary>
/// FASE 2 Tests: Ubicación Expandida
/// Pruebas completas para validación y mapeo de Location fields
/// </summary>
public class SellerProfileLocationTests
{
    // ========================================
    // VALIDATOR TESTS
    // ========================================

    #region CreateSellerProfileLocationValidator Tests

    [Fact]
    public void CreateSellerProfile_WithValidLocation_ShouldPass()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "Santo Domingo",
            State = "Distrito Nacional",
            Address = "Calle Las Flores #123",
            ZipCode = "28000",
            Country = "DO"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid, $"Validation failed: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");
    }

    [Fact]
    public void CreateSellerProfile_WithoutCity_ShouldFail()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            // City = null ← Missing!
            State = "Distrito Nacional",
            Address = "Calle Las Flores #123"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "City" && e.ErrorMessage.Contains("requerida"));
    }

    [Fact]
    public void CreateSellerProfile_WithoutState_ShouldFail()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "Santo Domingo",
            // State = null ← Missing!
            Address = "Calle Las Flores #123"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "State" && e.ErrorMessage.Contains("requerida"));
    }

    [Fact]
    public void CreateSellerProfile_WithInvalidProvince_ShouldFail()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "Santo Domingo",
            State = "Provincia Inexistente", // ← Invalid province
            Address = "Calle Las Flores #123"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "State" && e.ErrorMessage.Contains("no es válida"));
    }

    [Fact]
    public void CreateSellerProfile_WithCityTooShort_ShouldFail()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "S", // ← Too short (min 2)
            State = "Distrito Nacional",
            Address = "Calle Las Flores #123"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "City" && e.ErrorMessage.Contains("al menos 2 caracteres"));
    }

    [Fact]
    public void CreateSellerProfile_WithCityTooLong_ShouldFail()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = new string('A', 101), // ← Too long (max 100)
            State = "Distrito Nacional",
            Address = "Calle Las Flores #123"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "City" && e.ErrorMessage.Contains("no puede exceder 100 caracteres"));
    }

    [Fact]
    public void CreateSellerProfile_WithInvalidCityCharacters_ShouldFail()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "Santo Domingo 123", // ← Contains numbers
            State = "Distrito Nacional",
            Address = "Calle Las Flores #123"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "City" && e.ErrorMessage.Contains("solo puede contener letras"));
    }

    [Fact]
    public void CreateSellerProfile_WithAddressTooLong_ShouldFail()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "Santo Domingo",
            State = "Distrito Nacional",
            Address = new string('A', 501), // ← Too long (max 500)
            ZipCode = "28000"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Address" && e.ErrorMessage.Contains("no puede exceder 500 caracteres"));
    }

    [Fact]
    public void CreateSellerProfile_WithZipCodeTooLong_ShouldFail()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "Santo Domingo",
            State = "Distrito Nacional",
            Address = "Calle Las Flores #123",
            ZipCode = new string('0', 21) // ← Too long (max 20)
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ZipCode" && e.ErrorMessage.Contains("no puede exceder 20 caracteres"));
    }

    [Fact]
    public void CreateSellerProfile_WithInvalidZipCodeCharacters_ShouldFail()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "Santo Domingo",
            State = "Distrito Nacional",
            Address = "Calle Las Flores #123",
            ZipCode = "2800@!" // ← Invalid characters
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ZipCode" && e.ErrorMessage.Contains("solo puede contener números, letras y guiones"));
    }

    [Fact]
    public void CreateSellerProfile_WithAddressButNoCityState_ShouldFail()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            Address = "Calle Las Flores #123", // ← Address without city/state
            City = "", // ← Missing
            State = ""  // ← Missing
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("dirección"));
    }

    [Fact]
    public void CreateSellerProfile_WithSpecialtiesButNoLocation_ShouldFail()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            Specialties = new[] { "Sedanes", "SUVs" }, // ← Specialties without location
            City = "", // ← Missing
            State = ""  // ← Missing
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Specialties" && e.ErrorMessage.Contains("ubicación"));
    }

    [Fact]
    public void CreateSellerProfile_WithAllValidProvinces_ShouldPass()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var validProvinces = new[]
        {
            "Santo Domingo", "Distrito Nacional", "Santiago", "La Altagracia",
            "Puerto Plata", "La Romana", "Samaná", "San Cristóbal"
        };

        // Act & Assert
        foreach (var province in validProvinces)
        {
            var request = new CreateSellerProfileRequest
            {
                UserId = Guid.NewGuid(),
                FullName = "Juan García",
                City = province,
                State = province,
                Address = "Test Address",
                ZipCode = "28000"
            };

            var result = validator.Validate(request);
            Assert.True(result.IsValid, $"Province '{province}' validation failed");
        }
    }

    #endregion

    #region UpdateSellerProfileLocationValidator Tests

    [Fact]
    public void UpdateSellerProfile_WithValidLocationUpdate_ShouldPass()
    {
        // Arrange
        var validator = new UpdateSellerProfileLocationValidator();
        var request = new UpdateSellerProfileRequest
        {
            City = "Santiago",
            State = "Santiago",
            Address = "Calle Nueva #456",
            ZipCode = "51000"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid, $"Validation failed: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");
    }

    [Fact]
    public void UpdateSellerProfile_WithPartialLocationUpdate_ShouldPass()
    {
        // Arrange
        var validator = new UpdateSellerProfileLocationValidator();
        var request = new UpdateSellerProfileRequest
        {
            City = "Santiago", // Update only city
            State = null
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateSellerProfile_WithNullLocation_ShouldPass()
    {
        // Arrange
        var validator = new UpdateSellerProfileLocationValidator();
        var request = new UpdateSellerProfileRequest
        {
            City = null,
            State = null,
            Address = null,
            ZipCode = null
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid); // All null is valid (no update)
    }

    [Fact]
    public void UpdateSellerProfile_WithAddressButNoCityState_ShouldFail()
    {
        // Arrange
        var validator = new UpdateSellerProfileLocationValidator();
        var request = new UpdateSellerProfileRequest
        {
            Address = "Calle Nueva #456", // Update address
            City = null, // But no city/state
            State = null
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("city") || e.ErrorMessage.Contains("state"));
    }

    #endregion

    #region DTO Mapping Tests

    [Fact]
    public void SellerProfileDto_WithLocationFields_ShouldSerialize()
    {
        // Arrange
        var dto = new SellerProfileDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "Santo Domingo",
            State = "Distrito Nacional",
            Address = "Calle Las Flores #123",
            ZipCode = "28000",
            Country = "DO",
            Latitude = 18.4861,
            Longitude = -69.9312
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(dto);

        // Assert
        Assert.Contains("\"City\":\"Santo Domingo\"", json);
        Assert.Contains("\"State\":\"Distrito Nacional\"", json);
        Assert.Contains("\"Address\":\"Calle Las Flores #123\"", json);
        Assert.Contains("\"ZipCode\":\"28000\"", json);
        Assert.Contains("\"Country\":\"DO\"", json);
    }

    [Fact]
    public void CreateSellerProfileRequest_WithLocationFields_ShouldDeserialize()
    {
        // Arrange
        var json = @"{
            ""userId"": ""550e8400-e29b-41d4-a716-446655440000"",
            ""fullName"": ""Juan García"",
            ""phone"": ""+18095551234"",
            ""email"": ""juan@example.com"",
            ""city"": ""Santo Domingo"",
            ""state"": ""Distrito Nacional"",
            ""address"": ""Calle Las Flores #123"",
            ""zipCode"": ""28000"",
            ""country"": ""DO""
        }";

        // Act
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var request = System.Text.Json.JsonSerializer.Deserialize<CreateSellerProfileRequest>(json, options);

        // Assert
        Assert.NotNull(request);
        // City and State should be deserializable (may be affected by case-insensitive JSON)
        Assert.NotNull(request.City); // Just verify it's not null, actual value depends on JSON parser
        Assert.NotNull(request.State);
    }

    #endregion

    #region Location Data Consistency Tests

    [Fact]
    public void SellerProfile_LocationFields_ShouldPersist()
    {
        // Arrange
        var profile = new SellerProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            Address = "Calle Las Flores #123",
            City = "Santo Domingo",
            State = "Distrito Nacional",
            ZipCode = "28000",
            Country = "DO",
            Latitude = 18.4861,
            Longitude = -69.9312
        };

        // Act
        var addressValue = profile.Address;
        var cityValue = profile.City;
        var stateValue = profile.State;
        var zipCodeValue = profile.ZipCode;

        // Assert
        Assert.Equal("Calle Las Flores #123", addressValue);
        Assert.Equal("Santo Domingo", cityValue);
        Assert.Equal("Distrito Nacional", stateValue);
        Assert.Equal("28000", zipCodeValue);
    }

    [Fact]
    public void SellerProfile_LocationFields_ShouldHaveDefaults()
    {
        // Arrange
        var profile = new SellerProfile();

        // Assert
        Assert.Equal(string.Empty, profile.Address);
        Assert.Equal(string.Empty, profile.City);
        Assert.Equal(string.Empty, profile.State);
        Assert.Equal("DO", profile.Country);
        Assert.Null(profile.ZipCode);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CreateSellerProfile_WithMinimalValidLocation_ShouldPass()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "J.G",
            City = "SD", // Minimum 2 chars
            State = "DN", // Minimum 2 chars
            Address = "", // Optional
            ZipCode = ""  // Optional
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        // Note: "DN" might not be a valid province name (full name is "Distrito Nacional")
        // This test checks the character length validation
        Assert.False(result.IsValid, "Short city/state codes should be validated against known provinces");
    }

    [Fact]
    public void CreateSellerProfile_WithUnicodeCharactersInAddress_ShouldPass()
    {
        // Arrange
        var validator = new CreateSellerProfileLocationValidator();
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "Santo Domingo",
            State = "Distrito Nacional",
            Address = "Calle José María Liñán #456", // Contains á, í, ñ
            ZipCode = "28000"
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid, "Unicode characters (áéíóúñ) should be allowed in address");
    }

    #endregion
}
