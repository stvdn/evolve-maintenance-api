using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class RegisterRequest
{
    [Required, MinLength(3), MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "developer";
}
