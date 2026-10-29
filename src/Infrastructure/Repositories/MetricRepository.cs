using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MetricRepository : IMetricRepository
{
    private readonly AppDbContext _context;

    public MetricRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CodeMetric> CreateAsync(CodeMetric metric)
    {
        _context.CodeMetrics.Add(metric);
        await _context.SaveChangesAsync();
        return metric;
    }

    public async Task<CodeMetric?> GetLatestByMaintenanceIdAsync(Guid maintenanceId)
    {
        return await _context.CodeMetrics
            .Where(m => m.MaintenanceId == maintenanceId)
            .OrderByDescending(m => m.MeasuredAt)
            .FirstOrDefaultAsync();
    }

    public async Task<(IEnumerable<CodeMetric> Items, int Total)> GetHistoryByMaintenanceIdAsync(
        Guid maintenanceId, int page, int limit)
    {
        var query = _context.CodeMetrics
            .Where(m => m.MaintenanceId == maintenanceId)
            .OrderByDescending(m => m.MeasuredAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

        return (items, total);
    }

    public async Task<IEnumerable<object>> GetAlertsAsync(
        decimal minCoverage, int maxVulnerabilities, int maxComplexity)
    {
        // Get latest metric per maintenance
        var latestMetrics = await _context.CodeMetrics
            .Include(m => m.Maintenance)
            .GroupBy(m => m.MaintenanceId)
            .Select(g => g.OrderByDescending(m => m.MeasuredAt).First())
            .ToListAsync();

        var alerts = new List<object>();

        foreach (var metric in latestMetrics)
        {
            var metricAlerts = new List<object>();

            if (metric.CoveragePercentage < minCoverage)
                metricAlerts.Add(new
                {
                    type = "LOW_COVERAGE",
                    message = $"Cobertura de código ({metric.CoveragePercentage}%) por debajo del umbral ({minCoverage}%)",
                    severity = metric.CoveragePercentage < 50 ? "critical" : "high",
                    currentValue = metric.CoveragePercentage,
                    threshold = minCoverage
                });

            if (metric.Vulnerabilities > maxVulnerabilities)
                metricAlerts.Add(new
                {
                    type = "HIGH_VULNERABILITY",
                    message = $"Se detectaron {metric.Vulnerabilities} vulnerabilidades (umbral: {maxVulnerabilities})",
                    severity = "critical",
                    currentValue = metric.Vulnerabilities,
                    threshold = maxVulnerabilities
                });

            if (metric.CyclomaticComplexity > maxComplexity)
                metricAlerts.Add(new
                {
                    type = "HIGH_COMPLEXITY",
                    message = $"Complejidad ciclomática ({metric.CyclomaticComplexity}) por encima del umbral ({maxComplexity})",
                    severity = "high",
                    currentValue = metric.CyclomaticComplexity,
                    threshold = maxComplexity
                });

            if (metricAlerts.Count > 0)
                alerts.Add(new
                {
                    maintenanceId = metric.MaintenanceId,
                    maintenanceName = metric.Maintenance?.Name ?? "N/A",
                    alerts = metricAlerts,
                    measuredAt = metric.MeasuredAt
                });
        }

        return alerts;
    }
}
