using Xunit;
using FluentAssertions;
using InventoryManagementAPI.Infrastructure.Security;

namespace InventoryManagementAPI.Tests.Security;

/// <summary>
/// Pruebas unitarias para PasswordHasher
/// Cubre: Hashing, verificación, seguridad BCrypt
/// </summary>
public class PasswordHasherTests
{
    [Fact]
    public void Hash_WithValidPassword_ReturnsHashedString()
    {
        // Arrange
        const string password = "MySecurePassword123!";

        // Act
        var hash = PasswordHasher.Hash(password);

        // Assert
        hash.Should().NotBeNull();
        hash.Should().NotBe(password);  // No es plaintext
        hash.Should().StartWith("$2a$");  // Formato BCrypt
        hash.Length.Should().BeGreaterThan(30);
    }

    [Fact]
    public void Hash_WithSamePassword_ReturnsDifferentHashes()
    {
        // Arrange
        const string password = "MySecurePassword123!";

        // Act
        var hash1 = PasswordHasher.Hash(password);
        var hash2 = PasswordHasher.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2);  // Diferentes salts
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        const string password = "MySecurePassword123!";
        var hash = PasswordHasher.Hash(password);

        // Act
        var isValid = PasswordHasher.Verify(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        // Arrange
        const string correctPassword = "CorrectPassword123!";
        const string wrongPassword = "WrongPassword123!";
        var hash = PasswordHasher.Hash(correctPassword);

        // Act
        var isValid = PasswordHasher.Verify(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyPassword_ReturnsFalse()
    {
        // Arrange
        var hash = PasswordHasher.Hash("SomePassword123!");

        // Act
        var isValid = PasswordHasher.Verify(string.Empty, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithNullPassword_ReturnsFalse()
    {
        // Arrange
        var hash = PasswordHasher.Hash("SomePassword123!");

        // Act
        var isValid = PasswordHasher.Verify(null!, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithInvalidHash_ReturnsFalse()
    {
        // Arrange
        const string password = "SomePassword123!";
        const string invalidHash = "not-a-valid-bcrypt-hash";

        // Act & Assert
        var isValid = PasswordHasher.Verify(password, invalidHash);
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Hash_WithLongPassword_Succeeds()
    {
        // Arrange
        var longPassword = new string('a', 100);

        // Act
        var hash = PasswordHasher.Hash(longPassword);

        // Assert
        hash.Should().NotBeNull();
        var isValid = PasswordHasher.Verify(longPassword, hash);
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("P@ssw0rd123!")]
    [InlineData("ComplexP@ss123")]
    [InlineData("Super_Secure_Pass_2024!")]
    [InlineData("CyberSecurity#2024")]
    public void Hash_WithVaryingPasswords_AlwaysSucceeds(string password)
    {
        // Act
        var hash = PasswordHasher.Hash(password);
        var isValid = PasswordHasher.Verify(password, hash);

        // Assert
        isValid.Should().BeTrue();
        hash.Should().StartWith("$2a$");
    }
}
