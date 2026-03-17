using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InventoryManagementAPI.Application.DTOs.Auth;
using InventoryManagementAPI.Application.Interfaces;
using InventoryManagementAPI.Domain.Entities;
using InventoryManagementAPI.Infrastructure.Data;
using InventoryManagementAPI.Infrastructure.Services;
using InventoryManagementAPI.Infrastructure.Security;

namespace InventoryManagementAPI.Tests.Services;

/// <summary>
/// Pruebas unitarias para AuthService
/// Cubre: Registro, Login, Autenticación
/// </summary>
public class AuthServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IJwtTokenService> _tokenServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Usa DbContext en memoria para testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _tokenServiceMock = new Mock<IJwtTokenService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(_context, _tokenServiceMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    #region Registro

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "SecurePass123!"
        };

        var expectedToken = "jwt-token-here";
        var expectedUser = new User
        {
            Id = 1,
            Username = request.Username,
            Email = request.Email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(expectedToken);

        _mapperMock.Setup(x => x.Map<UserResponse>(It.IsAny<User>()))
            .Returns(new UserResponse { Id = expectedUser.Id, Username = expectedUser.Username });

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().Be(expectedToken);
        result.Message.Should().Contain("Registro completado");

        // Verifica que el usuario se guardó en BD
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ReturnsFailure()
    {
        // Arrange
        var existingUser = new User
        {
            Username = "existinguser",
            Email = "existing@example.com",
            PasswordHash = PasswordHasher.Hash("Password123!"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new UserRegisterRequest
        {
            Username = "existinguser",  // ← Duplicado
            Email = "different@example.com",
            Password = "SecurePass123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("ya existe");
    }

    [Fact]
    public async Task RegisterAsync_WithNullRequest_ReturnsFailure()
    {
        // Act
        var result = await _authService.RegisterAsync(null!);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Datos incompletos");
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyUsername_ReturnsFailure()
    {
        // Arrange
        var request = new UserRegisterRequest
        {
            Username = "   ",  // ← Whitespace
            Email = "test@example.com",
            Password = "SecurePass123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Datos incompletos");
    }

    #endregion

    #region Login

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessResponse()
    {
        // Arrange
        const string username = "testuser";
        const string password = "SecurePass123!";
        const string hashedPassword = "$2a$12$hashed";  // BCrypt hash

        var user = new User
        {
            Id = 1,
            Username = username,
            Email = "test@example.com",
            PasswordHash = PasswordHasher.Hash(password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new UserLoginRequest
        {
            Username = username,
            Password = password
        };

        var expectedToken = "jwt-token-login";
        _tokenServiceMock.Setup(x => x.GenerateToken(user.Id, user.Username, It.IsAny<string>()))
            .Returns(expectedToken);

        _mapperMock.Setup(x => x.Map<UserResponse>(It.IsAny<User>()))
            .Returns(new UserResponse { Id = user.Id, Username = user.Username });

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().Be(expectedToken);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFailure()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = PasswordHasher.Hash("CorrectPassword123!"),
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new UserLoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword123!"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Credenciales inválidas");
    }

    [Fact]
    public async Task LoginAsync_WithNonexistentUser_ReturnsFailure()
    {
        // Arrange
        var request = new UserLoginRequest
        {
            Username = "nonexistentuser",
            Password = "AnyPassword123!"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Credenciales inválidas");
    }

    #endregion

    #region GetProfile

    [Fact]
    public async Task GetUserByIdAsync_WithValidUserId_ReturnsUserProfile()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(x => x.Map<UserResponse>(It.IsAny<User>()))
            .Returns(new UserResponse { Id = user.Id, Username = user.Username, Email = user.Email });

        // Act
        var result = await _authService.GetUserByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidUserId_ReturnsNull()
    {
        // Act
        var result = await _authService.GetUserByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
