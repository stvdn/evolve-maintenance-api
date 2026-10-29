using Application.DTOs.Common;
using Application.DTOs.Maintenances;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceRepository _repository;

    public MaintenanceService(IMaintenanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<MaintenanceDto>>> GetAllAsync(
        int page, int limit, string? status, string? applicationName,
        string? squad, string sortBy, string order)
    {
        MaintenanceStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status))
        {
            var normalized = status.Replace("_", "");
            if (!Enum.TryParse<MaintenanceStatus>(normalized, true, out var parsed))
                throw new ArgumentException($"Estado inválido: {status}");
            statusEnum = parsed;
        }

        var (items, total) = await _repository.GetAllAsync(
            page, limit, statusEnum, applicationName, squad, sortBy, order);

        var dtos = items.Select(MapToDto);
        var pagination = new PaginationDto { Page = page, Limit = limit, Total = total };

        return ApiResponse<IEnumerable<MaintenanceDto>>.SuccessWithPagination(dtos, pagination);
    }

    public async Task<MaintenanceDto> GetByIdAsync(Guid id)
    {
        var maintenance = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Mantenimiento con ID {id} no encontrado");
        return MapToDto(maintenance);
    }

    public async Task<MaintenanceDto> CreateAsync(CreateMaintenanceRequest request)
    {
        var maintenance = new Maintenance
        {
            Name = request.Name,
            Description = request.Description,
            ApplicationName = request.ApplicationName,
            EstimatedHours = request.EstimatedHours,
            Squad = request.Squad,
            AssignedArchitect = request.AssignedArchitect
        };

        var created = await _repository.CreateAsync(maintenance);
        return MapToDto(created);
    }

    public async Task<MaintenanceDto> UpdateAsync(Guid id, UpdateMaintenanceRequest request)
    {
        var maintenance = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Mantenimiento con ID {id} no encontrado");

        if (!string.IsNullOrEmpty(request.Name)) maintenance.Name = request.Name;
        if (request.Description != null) maintenance.Description = request.Description;
        if (!string.IsNullOrEmpty(request.ApplicationName)) maintenance.ApplicationName = request.ApplicationName;
        if (request.EstimatedHours.HasValue) maintenance.EstimatedHours = request.EstimatedHours.Value;
        if (!string.IsNullOrEmpty(request.Squad)) maintenance.Squad = request.Squad;
        if (!string.IsNullOrEmpty(request.AssignedArchitect)) maintenance.AssignedArchitect = request.AssignedArchitect;

        if (!string.IsNullOrEmpty(request.Status))
        {
            var normalized = request.Status.Replace("_", "");
            if (!Enum.TryParse<MaintenanceStatus>(normalized, true, out var newStatus))
                throw new ArgumentException($"Estado inválido: {request.Status}");
            maintenance.Status = newStatus;
        }

        maintenance.UpdatedAt = DateTime.UtcNow;
        var updated = await _repository.UpdateAsync(maintenance);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        var maintenance = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Mantenimiento con ID {id} no encontrado");

        if (maintenance.Status == MaintenanceStatus.Completed)
            throw new InvalidOperationException("No se puede eliminar un mantenimiento completado");

        await _repository.DeleteAsync(id);
    }

    public async Task<object> GetStatsAsync()
    {
        return await _repository.GetStatsAsync();
    }

    private static MaintenanceDto MapToDto(Maintenance m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Description = m.Description,
        ApplicationName = m.ApplicationName,
        EstimatedHours = m.EstimatedHours,
        Status = m.Status switch
        {
            MaintenanceStatus.Draft => "draft",
            MaintenanceStatus.InProgress => "in_progress",
            MaintenanceStatus.InReview => "in_review",
            MaintenanceStatus.Completed => "completed",
            MaintenanceStatus.Cancelled => "cancelled",
            _ => m.Status.ToString().ToLower()
        },
        Squad = m.Squad,
        AssignedArchitect = m.AssignedArchitect,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };
}
