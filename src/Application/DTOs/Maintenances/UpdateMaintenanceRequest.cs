using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Maintenances;

public class UpdateMaintenanceRequest
{
    [MinLength(5), MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MinLength(2), MaxLength(100)]
    public string? ApplicationName { get; set; }

    [Range(1, 300)]
    public int? EstimatedHours { get; set; }

    public string? Status { get; set; }

    [MinLength(2), MaxLength(100)]
    public string? Squad { get; set; }

    [EmailAddress]
    public string? AssignedArchitect { get; set; }
}
