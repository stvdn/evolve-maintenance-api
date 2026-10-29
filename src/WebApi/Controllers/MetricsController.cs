using Application.DTOs.Common;
using Application.DTOs.Metrics;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/metrics")]
[Authorize]
[Produces("application/json")]
public class MetricsController : ControllerBase
{
    private readonly IMetricService _service;

    public MetricsController(IMetricService service)
    {
        _service = service;
    }

    /// <summary>Registrar métricas de calidad de código</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MetricDto>), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> Create([FromBody] CreateMetricRequest request)
    {
        var result = await _service.CreateAsync(request);
        return StatusCode(201, ApiResponse<MetricDto>.Success(result));
    }

    /// <summary>Obtener métricas actuales de un mantenimiento</summary>
    [HttpGet("maintenances/{maintenanceId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MetricDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> GetByMaintenance(Guid maintenanceId)
    {
        var result = await _service.GetByMaintenanceIdAsync(maintenanceId);
        return Ok(ApiResponse<MetricDto>.Success(result));
    }

    /// <summary>Historial de métricas de un mantenimiento</summary>
    [HttpGet("maintenances/{maintenanceId:guid}/history")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MetricDto>>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> GetHistory(
        Guid maintenanceId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var result = await _service.GetHistoryAsync(maintenanceId, page, limit);
        return Ok(result);
    }

    /// <summary>Obtener alertas de métricas bajo umbral</summary>
    [HttpGet("alerts")]
    [Authorize(Roles = "Architect,Manager")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] decimal minCoverage = 80,
        [FromQuery] int maxVulnerabilities = 0,
        [FromQuery] int maxComplexity = 20)
    {
        var result = await _service.GetAlertsAsync(minCoverage, maxVulnerabilities, maxComplexity);
        return Ok(result);
    }
}
