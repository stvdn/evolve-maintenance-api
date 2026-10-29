using Application.DTOs.Common;
using Application.DTOs.Metrics;

namespace Application.Interfaces;

public interface IMetricService
{
    Task<MetricDto> CreateAsync(CreateMetricRequest request);
    Task<MetricDto> GetByMaintenanceIdAsync(Guid maintenanceId);
    Task<ApiResponse<IEnumerable<MetricDto>>> GetHistoryAsync(Guid maintenanceId, int page, int limit);
    Task<object> GetAlertsAsync(decimal minCoverage, int maxVulnerabilities, int maxComplexity);
}
