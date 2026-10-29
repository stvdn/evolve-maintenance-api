using Domain.Enums;

namespace Domain.Entities;

public class Maintenance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Draft;
    public string Squad { get; set; } = string.Empty;
    public string AssignedArchitect { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<CodeMetric> Metrics { get; set; } = new List<CodeMetric>();
}
