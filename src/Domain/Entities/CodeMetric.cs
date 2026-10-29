namespace Domain.Entities;

public class CodeMetric
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MaintenanceId { get; set; }
    public decimal CoveragePercentage { get; set; }
    public int CyclomaticComplexity { get; set; }
    public int CodeSmells { get; set; }
    public int Vulnerabilities { get; set; }
    public decimal DuplicatedLinesPercentage { get; set; }
    public int? LinesOfCode { get; set; }
    public int? TechnicalDebtMinutes { get; set; }
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Maintenance Maintenance { get; set; } = null!;
}
