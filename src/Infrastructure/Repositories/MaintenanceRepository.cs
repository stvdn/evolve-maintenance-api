using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MaintenanceRepository : IMaintenanceRepository
{
    private readonly AppDbContext _context;

    public MaintenanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Maintenance> Items, int Total)> GetAllAsync(
        int page, int limit, MaintenanceStatus? status, string? applicationName,
        string? squad, string sortBy, string order)
    {
        var query = _context.Maintenances.AsQueryable();

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);
        if (!string.IsNullOrEmpty(applicationName))
            query = query.Where(m => m.ApplicationName.Contains(applicationName));
        if (!string.IsNullOrEmpty(squad))
            query = query.Where(m => m.Squad.Contains(squad));

        var total = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "estimatedhours" => order == "asc" ? query.OrderBy(m => m.EstimatedHours) : query.OrderByDescending(m => m.EstimatedHours),
            "status" => order == "asc" ? query.OrderBy(m => m.Status) : query.OrderByDescending(m => m.Status),
            "applicationname" => order == "asc" ? query.OrderBy(m => m.ApplicationName) : query.OrderByDescending(m => m.ApplicationName),
            _ => order == "asc" ? query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt)
        };

        var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();
        return (items, total);
    }

    public async Task<Maintenance?> GetByIdAsync(Guid id)
    {
        return await _context.Maintenances.FindAsync(id);
    }

    public async Task<Maintenance> CreateAsync(Maintenance maintenance)
    {
        _context.Maintenances.Add(maintenance);
        await _context.SaveChangesAsync();
        return maintenance;
    }

    public async Task<Maintenance> UpdateAsync(Maintenance maintenance)
    {
        _context.Maintenances.Update(maintenance);
        await _context.SaveChangesAsync();
        return maintenance;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Maintenances.FindAsync(id);
        if (entity != null)
        {
            _context.Maintenances.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<object> GetStatsAsync()
    {
        var all = await _context.Maintenances.ToListAsync();
        var now = DateTime.UtcNow;

        return new
        {
            totalMaintenances = all.Count,
            byStatus = new
            {
                draft = all.Count(m => m.Status == MaintenanceStatus.Draft),
                in_progress = all.Count(m => m.Status == MaintenanceStatus.InProgress),
                in_review = all.Count(m => m.Status == MaintenanceStatus.InReview),
                completed = all.Count(m => m.Status == MaintenanceStatus.Completed),
                cancelled = all.Count(m => m.Status == MaintenanceStatus.Cancelled)
            },
            averageHours = all.Any() ? Math.Round(all.Average(m => m.EstimatedHours), 1) : 0,
            thisMonth = new
            {
                created = all.Count(m => m.CreatedAt.Month == now.Month && m.CreatedAt.Year == now.Year),
                completed = all.Count(m => m.Status == MaintenanceStatus.Completed && m.UpdatedAt.Month == now.Month && m.UpdatedAt.Year == now.Year)
            }
        };
    }
}
