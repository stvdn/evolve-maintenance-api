using System.Security.Claims;
using Application.DTOs.Auth;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Registrar nuevo usuario</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return StatusCode(201, ApiResponse<AuthResponse>.Success(result));
    }

    /// <summary>Iniciar sesión</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.Success(result));
    }

    /// <summary>Obtener perfil del usuario actual</summary>
    [HttpGet("/api/v1/users/me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var result = await _authService.GetProfileAsync(userId);
        return Ok(ApiResponse<UserDto>.Success(result));
    }

    /// <summary>Actualizar perfil del usuario actual</summary>
    [HttpPatch("/api/v1/users/me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        var result = await _authService.UpdateProfileAsync(userId, request.FullName, request.Password);
        return Ok(ApiResponse<UserDto>.Success(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Token inválido");
        return Guid.Parse(claim);
    }
}

public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? Password { get; set; }
}
