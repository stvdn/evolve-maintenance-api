using Application.DTOs.Auth;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserDto> GetProfileAsync(Guid userId);
    Task<UserDto> UpdateProfileAsync(Guid userId, string? fullName, string? password);
}

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, string role);
}
