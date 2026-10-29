using Application.DTOs.Common;
using Application.DTOs.Maintenances;

namespace Application.Interfaces;

public interface IMaintenanceService
{
    Task<ApiResponse<IEnumerable<MaintenanceDto>>> GetAllAsync(
        int page, int limit, string? status, string? applicationName,
        string? squad, string sortBy, string order);
    Task<MaintenanceDto> GetByIdAsync(Guid id);
    Task<MaintenanceDto> CreateAsync(CreateMaintenanceRequest request);
    Task<MaintenanceDto> UpdateAsync(Guid id, UpdateMaintenanceRequest request);
    Task DeleteAsync(Guid id);
    Task<object> GetStatsAsync();
}
