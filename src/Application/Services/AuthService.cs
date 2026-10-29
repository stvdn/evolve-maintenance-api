using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository userRepository, IJwtService jwtService, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            throw new InvalidOperationException($"El email {request.Email} ya está registrado");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            throw new ArgumentException($"Rol inválido: {request.Role}");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLower(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role
        };

        await _userRepository.CreateAsync(user);

        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role.ToString());

        return new AuthResponse
        {
            User = MapToDto(user),
            Token = token,
            ExpiresIn = "8h"
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLower())
            ?? throw new UnauthorizedAccessException("Email o contraseña incorrectos");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email o contraseña incorrectos");

        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role.ToString());

        return new AuthResponse
        {
            User = MapToDto(user),
            Token = token,
            ExpiresIn = "8h"
        };
    }

    public async Task<UserDto> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado");
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, string? fullName, string? password)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        if (!string.IsNullOrEmpty(fullName))
            user.FullName = fullName;
        if (!string.IsNullOrEmpty(password))
            user.PasswordHash = _passwordHasher.Hash(password);

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return MapToDto(user);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.ToString().ToLower(),
        CreatedAt = user.CreatedAt
    };
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
