using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Maintenance> Maintenances => Set<Maintenance>();
    public DbSet<CodeMetric> CodeMetrics => Set<CodeMetric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        });

        // Maintenance
        modelBuilder.Entity<Maintenance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ApplicationName);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ApplicationName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Squad).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AssignedArchitect).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasMany(e => e.Metrics).WithOne(e => e.Maintenance)
                  .HasForeignKey(e => e.MaintenanceId).OnDelete(DeleteBehavior.Cascade);
        });

        // CodeMetric
        modelBuilder.Entity<CodeMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MaintenanceId);
            entity.HasIndex(e => new { e.MaintenanceId, e.MeasuredAt });
            entity.Property(e => e.CoveragePercentage).HasPrecision(5, 2);
            entity.Property(e => e.DuplicatedLinesPercentage).HasPrecision(5, 2);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed default architect user (password: Architect@123)
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            FullName = "Juan Pérez",
            Email = "juan.perez@bancoecuador.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Architect@123"),
            Role = UserRole.Architect,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Seed default developer user (password: Developer@123)
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
            FullName = "Carlos López",
            Email = "carlos.lopez@bancoecuador.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Developer@123"),
            Role = UserRole.Developer,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Seed manager user (password: Manager@123)
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
            FullName = "María García",
            Email = "maria.garcia@bancoecuador.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
            Role = UserRole.Manager,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
