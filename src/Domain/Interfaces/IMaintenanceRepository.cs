using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IMaintenanceRepository
{
    Task<(IEnumerable<Maintenance> Items, int Total)> GetAllAsync(
        int page, int limit, MaintenanceStatus? status = null,
        string? applicationName = null, string? squad = null,
        string sortBy = "createdAt", string order = "desc");
    Task<Maintenance?> GetByIdAsync(Guid id);
    Task<Maintenance> CreateAsync(Maintenance maintenance);
    Task<Maintenance> UpdateAsync(Maintenance maintenance);
    Task DeleteAsync(Guid id);
    Task<object> GetStatsAsync();
}
