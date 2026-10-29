using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Metrics;

public class CreateMetricRequest
{
    [Required]
    public Guid MaintenanceId { get; set; }

    [Required, Range(0, 100)]
    public decimal CoveragePercentage { get; set; }

    [Required, Range(0, int.MaxValue)]
    public int CyclomaticComplexity { get; set; }

    [Required, Range(0, int.MaxValue)]
    public int CodeSmells { get; set; }

    [Required, Range(0, int.MaxValue)]
    public int Vulnerabilities { get; set; }

    [Required, Range(0, 100)]
    public decimal DuplicatedLinesPercentage { get; set; }

    [Range(0, int.MaxValue)]
    public int? LinesOfCode { get; set; }

    [Range(0, int.MaxValue)]
    public int? TechnicalDebtMinutes { get; set; }
}
