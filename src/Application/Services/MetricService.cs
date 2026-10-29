using Application.DTOs.Common;
using Application.DTOs.Metrics;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class MetricService : IMetricService
{
    private readonly IMetricRepository _metricRepository;
    private readonly IMaintenanceRepository _maintenanceRepository;

    public MetricService(IMetricRepository metricRepository, IMaintenanceRepository maintenanceRepository)
    {
        _metricRepository = metricRepository;
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task<MetricDto> CreateAsync(CreateMetricRequest request)
    {
        var maintenance = await _maintenanceRepository.GetByIdAsync(request.MaintenanceId)
            ?? throw new KeyNotFoundException($"Mantenimiento con ID {request.MaintenanceId} no encontrado");

        var metric = new CodeMetric
        {
            MaintenanceId = request.MaintenanceId,
            CoveragePercentage = request.CoveragePercentage,
            CyclomaticComplexity = request.CyclomaticComplexity,
            CodeSmells = request.CodeSmells,
            Vulnerabilities = request.Vulnerabilities,
            DuplicatedLinesPercentage = request.DuplicatedLinesPercentage,
            LinesOfCode = request.LinesOfCode,
            TechnicalDebtMinutes = request.TechnicalDebtMinutes
        };

        var created = await _metricRepository.CreateAsync(metric);
        return MapToDto(created);
    }

    public async Task<MetricDto> GetByMaintenanceIdAsync(Guid maintenanceId)
    {
        var metric = await _metricRepository.GetLatestByMaintenanceIdAsync(maintenanceId)
            ?? throw new KeyNotFoundException($"No hay métricas para el mantenimiento {maintenanceId}");
        return MapToDto(metric);
    }

    public async Task<ApiResponse<IEnumerable<MetricDto>>> GetHistoryAsync(
        Guid maintenanceId, int page, int limit)
    {
        var (items, total) = await _metricRepository.GetHistoryByMaintenanceIdAsync(
            maintenanceId, page, limit);

        var dtos = items.Select(MapToDto);
        var pagination = new PaginationDto { Page = page, Limit = limit, Total = total };

        return ApiResponse<IEnumerable<MetricDto>>.SuccessWithPagination(dtos, pagination);
    }

    public async Task<object> GetAlertsAsync(decimal minCoverage, int maxVulnerabilities, int maxComplexity)
    {
        var alerts = await _metricRepository.GetAlertsAsync(minCoverage, maxVulnerabilities, maxComplexity);
        return new { status = "success", data = alerts, totalAlerts = alerts.Count() };
    }

    private static MetricDto MapToDto(CodeMetric m) => new()
    {
        Id = m.Id,
        MaintenanceId = m.MaintenanceId,
        CoveragePercentage = m.CoveragePercentage,
        CyclomaticComplexity = m.CyclomaticComplexity,
        CodeSmells = m.CodeSmells,
        Vulnerabilities = m.Vulnerabilities,
        DuplicatedLinesPercentage = m.DuplicatedLinesPercentage,
        LinesOfCode = m.LinesOfCode,
        TechnicalDebtMinutes = m.TechnicalDebtMinutes,
        MeasuredAt = m.MeasuredAt
    };
}
