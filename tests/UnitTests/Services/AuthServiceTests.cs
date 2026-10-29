using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Moq;
using FluentAssertions;

namespace UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _hasherMock = new Mock<IPasswordHasher>();
        _service = new AuthService(_userRepoMock.Object, _jwtServiceMock.Object, _hasherMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Test User",
            Email = "test@banco.com",
            Password = "Test@1234",
            Role = "developer"
        };

        _userRepoMock.Setup(r => r.ExistsByEmailAsync("test@banco.com")).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash("Test@1234")).Returns("hashed_password");
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("fake_jwt_token");

        // Act
        var result = await _service.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.User.FullName.Should().Be("Test User");
        result.User.Email.Should().Be("test@banco.com");
        result.User.Role.Should().Be("developer");
        result.Token.Should().Be("fake_jwt_token");
        result.ExpiresIn.Should().Be("8h");
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Test",
            Email = "existing@banco.com",
            Password = "Test@1234",
            Role = "developer"
        };

        _userRepoMock.Setup(r => r.ExistsByEmailAsync("existing@banco.com")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidRole_ThrowsArgumentException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Test",
            Email = "test@banco.com",
            Password = "Test@1234",
            Role = "invalid_role"
        };

        _userRepoMock.Setup(r => r.ExistsByEmailAsync("test@banco.com")).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Test User",
            Email = "test@banco.com",
            PasswordHash = "hashed_password",
            Role = UserRole.Architect
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@banco.com")).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("Test@1234", "hashed_password")).Returns(true);
        _jwtServiceMock.Setup(j => j.GenerateToken(user.Id, user.Email, "Architect")).Returns("jwt_token");

        // Act
        var result = await _service.LoginAsync(new LoginRequest { Email = "test@banco.com", Password = "Test@1234" });

        // Assert
        result.Token.Should().Be("jwt_token");
        result.User.Role.Should().Be("architect");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = new User
        {
            Email = "test@banco.com",
            PasswordHash = "hashed_password",
            Role = UserRole.Developer,
            FullName = "Test"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@banco.com")).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("wrong_password", "hashed_password")).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginRequest { Email = "test@banco.com", Password = "wrong_password" }));
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync("nobody@banco.com")).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginRequest { Email = "nobody@banco.com", Password = "any" }));
    }

    [Fact]
    public async Task GetProfileAsync_WhenExists_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, FullName = "Test", Email = "t@b.com", Role = UserRole.Manager, PasswordHash = "h" };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _service.GetProfileAsync(userId);

        // Assert
        result.Id.Should().Be(userId);
        result.Role.Should().Be("manager");
    }

    [Fact]
    public async Task GetProfileAsync_WhenNotExists_ThrowsKeyNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetProfileAsync(Guid.NewGuid()));
    }
}
