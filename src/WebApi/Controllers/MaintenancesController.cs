using Application.DTOs.Common;
using Application.DTOs.Maintenances;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/maintenances")]
[Authorize]
[Produces("application/json")]
public class MaintenancesController : ControllerBase
{
    private readonly IMaintenanceService _service;

    public MaintenancesController(IMaintenanceService service)
    {
        _service = service;
    }

    /// <summary>Listar mantenimientos con paginación y filtros</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MaintenanceDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? applicationName = null,
        [FromQuery] string? squad = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string order = "desc")
    {
        var result = await _service.GetAllAsync(page, limit, status, applicationName, squad, sortBy, order);
        return Ok(result);
    }

    /// <summary>Obtener estadísticas de mantenimientos</summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Architect,Manager")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _service.GetStatsAsync();
        return Ok(new { status = "success", data = result });
    }

    /// <summary>Obtener detalle de un mantenimiento</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<MaintenanceDto>.Success(result));
    }

    /// <summary>Crear nuevo mantenimiento</summary>
    [HttpPost]
    [Authorize(Roles = "Architect")]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceDto>), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceRequest request)
    {
        var result = await _service.CreateAsync(request);
        return StatusCode(201, ApiResponse<MaintenanceDto>.Success(result));
    }

    /// <summary>Actualizar un mantenimiento</summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceDto>), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMaintenanceRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<MaintenanceDto>.Success(result));
    }

    /// <summary>Eliminar un mantenimiento</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Architect")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(new { status = "success", message = "Mantenimiento eliminado exitosamente" });
    }
}
