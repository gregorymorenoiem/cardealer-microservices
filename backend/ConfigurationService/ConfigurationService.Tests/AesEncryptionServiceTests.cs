using ConfigurationService.Infrastructure.Services;
using FluentAssertions;

namespace ConfigurationService.Tests;

public class AesEncryptionServiceTests
{
    private readonly AesEncryptionService _encryptionService;

    public AesEncryptionServiceTests()
    {
        _encryptionService = new AesEncryptionService("TestEncryptionKey123!");
    }

    [Fact]
    public void Encrypt_ShouldEncryptString()
    {
        // Arrange
        var plainText = "SecretPassword123";

        // Act
        var encrypted = _encryptionService.Encrypt(plainText);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
        encrypted.Should().NotBe(plainText);
    }

    [Fact]
    public void Decrypt_ShouldDecryptString()
    {
        // Arrange
        var plainText = "SecretPassword123";
        var encrypted = _encryptionService.Encrypt(plainText);

        // Act
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void EncryptDecrypt_RoundTrip_ShouldPreserveValue()
    {
        // Arrange
        var original = "MySecretValue!@#$%^&*()";

        // Act
        var encrypted = _encryptionService.Encrypt(original);
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(original);
    }

    [Fact]
    public void Encrypt_NullValue_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _encryptionService.Encrypt(null!));
    }
}
