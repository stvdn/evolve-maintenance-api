using Domain.Entities;

namespace Domain.Interfaces;

public interface IMetricRepository
{
    Task<CodeMetric> CreateAsync(CodeMetric metric);
    Task<CodeMetric?> GetLatestByMaintenanceIdAsync(Guid maintenanceId);
    Task<(IEnumerable<CodeMetric> Items, int Total)> GetHistoryByMaintenanceIdAsync(
        Guid maintenanceId, int page, int limit);
    Task<IEnumerable<object>> GetAlertsAsync(
        decimal minCoverage, int maxVulnerabilities, int maxComplexity);
}
