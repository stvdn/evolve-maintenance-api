using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Maintenances;

public class CreateMaintenanceRequest
{
    [Required, MinLength(5), MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required, MinLength(2), MaxLength(100)]
    public string ApplicationName { get; set; } = string.Empty;

    [Required, Range(1, 300)]
    public int EstimatedHours { get; set; }

    [Required, MinLength(2), MaxLength(100)]
    public string Squad { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string AssignedArchitect { get; set; } = string.Empty;
}
