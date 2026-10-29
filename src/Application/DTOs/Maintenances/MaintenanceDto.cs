namespace Application.DTOs.Maintenances;

public class MaintenanceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Squad { get; set; } = string.Empty;
    public string AssignedArchitect { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
