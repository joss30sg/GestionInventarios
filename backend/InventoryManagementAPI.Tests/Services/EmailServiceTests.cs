using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using InventoryManagementAPI.Infrastructure.Services;

namespace InventoryManagementAPI.Tests.Services;

public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _loggerMock = new Mock<ILogger<EmailService>>();
        _emailService = new EmailService(_loggerMock.Object);
    }

    [Fact]
    public async Task SendEmailAsync_WithValidParameters_ReturnsTrue()
    {
        // Act - SMTP not configured in test env, simulates success
        var result = await _emailService.SendEmailAsync("test@example.com", "Test Subject", "<p>Body</p>");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithValidParameters_ReturnsTrue()
    {
        // Act
        var result = await _emailService.SendPasswordResetEmailAsync(
            "user@example.com",
            "reset-token-123",
            "https://example.com/reset?token=reset-token-123");

        // Assert
        result.Should().BeTrue();
    }
}
